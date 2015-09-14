Imports System.IO
Imports DiscordSharp
Imports DiscordSharp.Events
Imports Luigibot2
Imports Newtonsoft.Json

Module Main
    Dim WithEvents Client As New DiscordClient()
    Dim Settings As New Settings()
    Dim Encrypter As New SimpleAES()
    Dim RanCode As New RandomCodeGenerator()

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

        Dim LoginInfo As New DiscordLoginInformation()
        Dim EmailArray(1) As String
        EmailArray(0) = Settings.BotEmail
        Dim PasswordArray(1) As String
        PasswordArray(0) = Encrypter.DecryptString(Settings.BotPassword)
        LoginInfo.email = EmailArray
        LoginInfo.password = PasswordArray
        Client.LoginInformation = LoginInfo

        Dim token As String = Client.SendLoginRequest()
        If token IsNot "" Then
            Client.ConnectAndReadMessages()
        End If
        While True
        End While

    End Sub



    Sub OnMessage_EventHandler(sender As Object, e As DiscordMessageEventArgs) Handles Client.MessageReceived
        If e.message.content.StartsWith(Settings.CommandPrefix) Then
            Dim trimmedString As String = e.message.content.Replace("?"c, "")
            If trimmedString.StartsWith("status") Then
                Client.SendMessageToChannel("I work! In VB!", e.Channel)
            End If
        End If
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
