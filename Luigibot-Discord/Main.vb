Imports System.IO
Imports System.Threading
Imports DiscordSharp
Imports DiscordSharp.Events
Imports [IF].Lastfm.Core.Api
Imports [IF].Lastfm.Core.Objects
Imports Luigibot2
Imports Newtonsoft.Json

Module Main
    Dim WithEvents Client As New DiscordClient()
    Dim Settings As New Settings()
    Dim Encrypter As New SimpleAES()
    Dim RanCode As New RandomCodeGenerator()

    Dim IsAuthenticating As Boolean = False

    Private EightballMessages As String() = New String() {"Signs point to yes.", "Yes.", "Reply hazy, try again.", "Without a doubt", "My sources say no", "As I see it, yes.",
    "You may rely on it.", "Concentrate and ask again", "Outlook not so good", "It is decidedly so", "Better not tell you now.", "Very doubtful",
    "Yes - definitely", "It is certain", "Cannot predict now", "Most likely", "Ask again later", "My reply is no",
    "Outlook good", "Don't count on it"}

    Sub Main()
        Console.ForegroundColor = ConsoleColor.Green
        Console.WriteLine("Welcome to Luigibot-Discord!")
        Console.ForegroundColor = ConsoleColor.White
        If (IsFirstRun()) Then
            Console.WriteLine("Welcome! As this is your first run, there are some things you will need to provide me.")
            Console.WriteLine("These are only a one time thing, after this, you will not be asked for this again unless you delete your Settings.json.")
            Console.Write("Press enter.." + vbNewLine)
            Console.ReadLine()
            Console.Write("If you don't have a Discord account for your bot, please make one now." + vbNewLine)
            Console.ReadLine()
            Console.Write(vbNewLine + "What is the bot's email?: ")
            Dim BotEmail As String = Console.ReadLine()
            Console.Write(vbNewLine + "Now, what is the bot's password? (Visible): ")
            Dim BotPassword As String = Console.ReadLine()
            Console.Clear()
            Console.WriteLine("Thank you, the password value will be encrypted in the Settings.json")
            Console.ReadLine()
            Settings.BotEmail = BotEmail
            Settings.BotPassword = Encrypter.EncryptToString(BotPassword)
            SaveSettings()
        Else
            LoadSettings()
        End If

        If Settings.OwnerUserIDs(0) = "0" Then
            IsAuthenticating = True
        End If

        Client.ClientPrivateInformation.email = Settings.BotEmail
        Client.ClientPrivateInformation.password = Encrypter.DecryptString(Settings.BotPassword)
        Connect()
        System.Windows.Forms.Application.Run()
    End Sub

    Sub Connect()
        Dim token As String = Client.SendLoginRequest()
        If token IsNot "" Then
            Client.ConnectAndReadMessages()
        End If
    End Sub

    Dim Code As String = RanCode.GenerateRandomCode()
    Sub OnConnect_EventHandler(sender As Object, e As DiscordConnectEventArgs) Handles Client.Connected
        Console.WriteLine("Connected as " + e.user.user.username)

        If IsAuthenticating Then
            Console.WriteLine("Please authenticate yourself as a bot administrator.")
            Console.WriteLine("To do this, please type the command '?authenticate " + Code + "' into any channel.")
            Console.WriteLine("If this works, you will get a message confirming your authentication as bot owner.")
        End If
    End Sub

    Dim ran As New Random(DateTime.Now.Millisecond)

    Sub Eightball(ByVal e As DiscordMessageEventArgs)
        If Settings.EightballEnabled Then
            Client.SendMessageToChannel(EightballMessages(ran.Next(0, EightballMessages.Count - 1)), e.Channel)
        End If
    End Sub

    Sub SocketClosed_EventHandler(sender As Object, e As DiscordSocketClosedEventArgs) Handles Client.SocketClosed
        Console.WriteLine("Socket closed? {0} (Code {1}; Clean: {2})", e.Reason, e.Code, e.WasClean)
        Thread.Sleep(30 * 1000)
        Console.WriteLine("trying reconnect...")
        Connect()
    End Sub

    Sub Usertyping_EventHandler(sender As Object, e As DiscordTypingStartEventArgs) Handles Client.UserTypingStart

    End Sub

    Sub UnknownMessage_EventHandler(sender As Object, e As UnknownMessageEventArgs) Handles Client.UnknownMessageTypeReceived
        Using sw As New StreamWriter(e.RawJson("t").ToString())
            sw.WriteLine(e.RawJson)
        End Using
        Client.SendMessageToUser("New message type '" + e.RawJson("t").ToString() + "' has been discovered.", Client.GetServersList.Find(Function(x) x.members.Find(Function(y) y.user.id = Settings.OwnerUserIDs(0)) IsNot Nothing).members.Find(Function(x) x.user.id = Settings.OwnerUserIDs(0)))
    End Sub

    Sub Lastfm(e As DiscordMessageEventArgs)
#If __MONOCS__ Then
        Client.SendMessageToChannel("Sorry, not on Mono :(", e.Channel)
#Else
        Dim split As String() = e.message.content.Split({" "c}, 2)
        If (split.Length > 0) Then
            Using lastfmClient As New LastfmClient("4de0532fe30150ee7a553e160fbbe0e0", "0686c5e41f20d2dc80b64958f2df0f0c", Nothing, Nothing)
                Try
                    Dim recentScrobbles = lastfmClient.User.GetRecentScrobbles(split(1).Trim(), Nothing, 1, 1)
                    Dim lastTrack As LastTrack = recentScrobbles.Result.Content(1)
                    Client.SendMessageToChannel(String.Format("**{0}** last listened to *{1}* by *{2}*", split(1), lastTrack.Name, lastTrack.ArtistName), e.Channel)
                Catch ex As Exception
                    Client.SendMessageToChannel("User *" + split(1) + "* not found!", e.Channel)
                End Try
            End Using
        Else
            Client.SendMessageToChannel("Whos' Last.FM am I checking?", e.Channel)
        End If

#End If
    End Sub

    Sub Changename(e As DiscordMessageEventArgs)
        Dim split As String() = e.message.content.Split({" "c}, 2)
        If split.Length > 0 Then
            'Client.ChangeBotUsername(split(1))
            Dim newUserInfo = Client.ClientPrivateInformation
            newUserInfo.username = split(1)
            Client.ChangeBotInformation(newUserInfo)
            Client.SendMessageToChannel("Name changed!", e.Channel)
        Else
            Client.SendMessageToChannel("Change bot's name to what?", e.Channel)
        End If
    End Sub

    Sub OnMessage_EventHandler(sender As Object, e As DiscordMessageEventArgs) Handles Client.MessageReceived
        Console.WriteLine(String.Format("<{0}> in #{1}: {2}", e.author.user.username, e.Channel.name, e.message.content))
        If e.message.content.StartsWith(Settings.CommandPrefix) Then
            Dim trimmedString As String = e.message.content.Replace("?"c, "")
            If trimmedString.StartsWith("status") Then
                Client.SendMessageToChannel("I work! In VB!", e.Channel)
            ElseIf trimmedString.StartsWith("changetopic") Then
                If e.author.user.id = Settings.OwnerUserIDs(0) Then
                    Dim split = e.message.content.Split({" "c}, 2)
                    If split.Length > 0 Then
                        ChangeChannelTopic(split(1), e.Channel)
                    End If
                End If
            ElseIf trimmedString.StartsWith("changename") Then
                If e.author.user.id = Settings.OwnerUserIDs(0) Then
                    Changename(e)
                End If
            ElseIf trimmedString.StartsWith("eightball") Or trimmedString.StartsWith("8ball") Then
                Eightball(e)
            ElseIf trimmedString.StartsWith("slap") Then
                If Settings.SlapEnabled Then
                    ''TODO: Do proper things with this..
                    Dim split As String() = e.message.content.Split({" "c}, 2)
                    If (split.Length > 0) Then
                        Client.SendMessageToChannel("\* **" + Client.Me.user.username + "** slaps **@" + split(1) + " ** around with a giant fishbot.", e.Channel)
                    End If
                End If
            ElseIf trimmedString.StartsWith("lastfm") Then
                Lastfm(e)
            ElseIf trimmedString.StartsWith("idunno") Then
                Client.SendMessageToChannel("¯\\_(ツ)_/¯", e.Channel)
            ElseIf trimmedString.StartsWith("selfdestruct") Then
                If e.author.user.id = Settings.OwnerUserIDs(0) Then
                    Client.SendMessageToChannel("Alluha akbar!", e.Channel)
                    Environment.Exit(0)
                End If
            ElseIf trimmedString.StartsWith("authenticate") Then
                If IsAuthenticating Then
                    Dim split As String() = e.message.content.Split({" "c}, 2)
                    If split.Length > 0 Then
                        If split(1).Trim() = Code Then
                            Client.SendMessageToChannel("Code confirmed! @" + e.author.user.username + ", you are now my administrator!", e.Channel)
                            Settings.OwnerUserIDs(0) = e.author.user.id
                            SaveSettings()
                            IsAuthenticating = False
                        Else
                            Client.SendMessageToChannel("Wrong code! Please check the console for the correct one", e.Channel)
                        End If
                    Else
                        Client.SendMessageToChannel("Nice try", e.Channel)
                    End If
                End If
            End If
        End If
    End Sub

    Sub ChangeChannelTopic(ByVal topic As String, ByVal channel As DiscordChannel)
        Client.ChangeChannelTopic(topic, channel)
    End Sub

    Function IsFirstRun() As Boolean
        If File.Exists("Settings.json") Then
            Return False
        Else
            Return True
        End If
    End Function

    Sub LoadSettings()
        Dim js As New JsonSerializer()
        js.Formatting = Formatting.Indented
        Using sr As New StreamReader("Settings.json")
            Using jsr As New JsonTextReader(sr)
                Settings = js.Deserialize(Of Settings)(jsr)
            End Using
        End Using
    End Sub

    Sub SaveSettings()
        Dim js As New JsonSerializer()
        js.Formatting = Formatting.Indented
        Using sw As New StreamWriter("Settings.json")
            Using jsw As New JsonTextWriter(sw)
                js.Serialize(jsw, Settings)
            End Using
        End Using
    End Sub

End Module
