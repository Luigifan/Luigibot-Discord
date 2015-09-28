Imports DiscordSharp

Public Class Settings
    Public Property EightballEnabled As Boolean
    Public Property SlapEnabled As Boolean
    Public Property WelcomeUserEnabled As Boolean
    Public Property SeenEnabled As Boolean
    Public Property AllowedChannels As List(Of DiscordSharp.DiscordChannel)
    Public Property OwnerUserIDs
    Public Property CommandPrefix As Char

    Public Property BotEmail As String
    Public Property BotPassword As String 'This is encrypted

    Sub New()
        Dim _ownerUserIDs(20) As String
        OwnerUserIDs = _ownerUserIDs
        EightballEnabled = True
        SlapEnabled = True
        SeenEnabled = False
        AllowedChannels = New List(Of
            DiscordChannel)
        OwnerUserIDs(0) = 0
        CommandPrefix = "?"c

    End Sub
End Class
