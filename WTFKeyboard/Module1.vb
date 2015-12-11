Imports System.IO

Module Module1

    Sub Main(args() As String)
        Dim searchPath = "../../../testcases"
        If args.Length > 0 Then
            searchPath = args(0)
        End If
        Console.WriteLine("searching {0}", searchPath)
        For Each langs In Directory.GetDirectories(searchPath)
            Dim langdi As New DirectoryInfo(langs)
            Console.WriteLine("language: {0}", langdi.Name)
            For Each widths In Directory.GetDirectories(langs)
                Dim di As New DirectoryInfo(widths)
                Dim li As New LayoutInfo(widths, Int32.Parse(di.Name))
                li.Do()
            Next
        Next

        Console.ReadKey()
    End Sub

End Module
