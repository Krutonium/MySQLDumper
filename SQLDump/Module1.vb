Imports MySql.Data.MySqlClient
Imports Newtonsoft.Json
Imports System.IO
Imports System.Text
Imports System.Globalization
Imports System.IO.Compression
Module Module1
    Dim JSON As New LoginStuff
    Dim MysqlConn As MySqlConnection

    Sub Main()
        If File.Exists("Config.JSON") = False Then
            File.WriteAllText("Config.JSON", JsonConvert.SerializeObject(JSON))
            Console.WriteLine("Wrote Config to Config.JSON, Press any key to close.")
            'Console.ReadKey()
            Environment.Exit(1)
        Else
            Try
                JSON = JsonConvert.DeserializeObject(Of LoginStuff)(File.ReadAllText("Config.JSON"))
                If JSON.FormComplete = False Then
                    Console.WriteLine("Please ensure that the Config file is fully filled out! FormComplete should equal True!")
                    Console.WriteLine("Press any key to close.")
                    'Console.ReadKey()
                    Environment.Exit(1)
                End If
                If JSON.JsonVer_DO_NOT_CHANGE <> 3 Then
                    JSON.JsonVer_DO_NOT_CHANGE = 3
                    File.WriteAllText("Config.JSON", JsonConvert.SerializeObject(JSON))
                    Console.WriteLine("JSON File Updated, Please check to see what changed and update as needed.")
                    'Console.ReadKey()
                    Environment.Exit(1)
                End If
            Catch ex As Exception
                Console.WriteLine(ex.ToString)
                Console.ReadKey()
                Environment.Exit(1)
            End Try
        End If
        MysqlConn = New MySqlConnection()
        MysqlConn.ConnectionString = "server=" & JSON.ServerURL & ";" _
    & "user id=" & JSON.Username & ";" _
    & "password=" & JSON.Password & ";" _
    & "database=" & JSON.Database
        Try
            MysqlConn.Open()
            Console.WriteLine("Connected to MySQL.")
        Catch ex As Exception
            Console.WriteLine("Could not connect to MySQL: " & vbNewLine & ex.ToString)
            'Console.ReadKey()
            Environment.Exit(1)
        End Try
        Dim cmd As New MySqlCommand
        cmd.Connection = MysqlConn
        Dim mb As MySqlBackup = New MySqlBackup(cmd)
        If JSON.WhereToSaveDB = "" Then
            If JSON.OverWriteDBFile = True Then
                SaveFile(JSON.SaveDBAs & ".sql", mb.ExportToString)
            Else
                SaveFile(CreateMeaningfulFileName(JSON.SaveDBAs, DateTime.Now) & ".sql", mb.ExportToString)
            End If
        Else
            If Directory.Exists(JSON.WhereToSaveDB) = False Then
                Directory.CreateDirectory(JSON.WhereToSaveDB)
            End If
            If JSON.OverWriteDBFile = True Then
                SaveFile(JSON.WhereToSaveDB & "/" & JSON.SaveDBAs & ".sql", mb.ExportToString)
            Else
                SaveFile(JSON.WhereToSaveDB & "/" & CreateMeaningfulFileName(JSON.SaveDBAs, DateTime.Now) & ".sql", mb.ExportToString)
            End If
        End If


        MysqlConn.Close()
        Console.WriteLine("Backup Complete!")
        'Console.ReadKey()
        Environment.Exit(0)
    End Sub

    Private Function SaveFile(ByVal PathAndName As String, ByVal Contents As String)
        If JSON.Compress = True Then
            PathAndName += ".gz"
            Dim array() As Byte = Encoding.ASCII.GetBytes(Contents)
            Dim c() As Byte = Compresss(array)
            File.WriteAllBytes(PathAndName, c)
            Return True
        Else
            File.WriteAllText(PathAndName, Contents)
            Return True
        End If
        Return False
    End Function
    Public Function Compresss(ByVal raw() As Byte) As Byte()
        Using memory As MemoryStream = New MemoryStream()
            Using gzip As GZipStream = New GZipStream(memory, CompressionMode.Compress, True)
                gzip.Write(raw, 0, raw.Length)
            End Using
            Return memory.ToArray()
        End Using
    End Function
    Private Function CreateMeaningfulFileName(friendlyName As String, [date] As DateTime) As String
        Dim sb As New StringBuilder()
        For Each s As String In friendlyName.Split(New Char() {" "c})
            sb.Append(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower()))
        Next
        sb.Append("_" + [date].ToString("yyyy-MM-dd_HH-mm"))
        Return sb.ToString()
    End Function

End Module




Public Class LoginStuff
    Public Property Username As String = ""
    Public Property Password As String = ""
    Public Property ServerURL As String = ""
    Public Property ServerPort As String = ""
    Public Property Database As String = ""
    Public Property SaveDBAs As String = "Backup"
    Public Property OverWriteDBFile As Boolean = True
    Public Property WhereToSaveDB As String = ""
    Public Property Compress As Boolean = False
    Public Property FormComplete As Boolean = False
    Public Property JsonVer_DO_NOT_CHANGE As Integer = 0

End Class
