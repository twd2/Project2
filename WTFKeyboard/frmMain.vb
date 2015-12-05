Imports System.IO
Imports System.Text
Imports System.Threading

Public Class frmMain

    Dim _imageBuffer As Bitmap = Nothing
    Dim _g As Graphics = Nothing

    Private Sub frmMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        PicResize()
    End Sub

    Private Sub PicResize()
        If picMain.Width <= 0 OrElse picMain.Height <= 0 Then
            Return
        End If

        If _g IsNot Nothing Then
            _g.Dispose()
            _g = Nothing
        End If
        If _imageBuffer IsNot Nothing Then
            _imageBuffer.Dispose()
            _imageBuffer = Nothing
        End If

        _imageBuffer = New Bitmap(picMain.Width, picMain.Height)
        _g = Graphics.FromImage(_imageBuffer)
        picMain.Image = _imageBuffer

        _g.Clear(Color.White)
        _g.DrawString("hello, world", New Font("Consolas", 13), Brushes.Black, 0, 30)
        picMain.Refresh()
    End Sub

    Private Sub Process(fi As FileInfo)
        Dim ext = fi.Extension.ToLower()
        If ext <> ".png" AndAlso ext <> ".jpg" Then
            Return
        End If

        Using img = Image.FromFile(fi.FullName)
            Dim ki = KeyboardInfo.FromImage(img)

            Dim ri = ki.ToRows()

#If DEBUG Then
            Dim sb0 As New StringBuilder

            For Each row In ri
                For Each key In row.Keys
                    sb0.Append(KeyInfo.TypeName(key.Type) + " ")
                Next
                sb0.AppendLine()
            Next
            Debug.Print(sb0.ToString())
#End If
            If ri.Count > 0 Then
                Dim rows As New List(Of RowInfo)
                If ri(0).Top > 0 Then
                    Dim newrow As New RowInfo
                    newrow.Keys.Add(New KeyInfo With {.Type = KeyInfo.KeyType.S,
                                         .Rect = New Rectangle(0, 0, ki.Image.Width, ri(0).Top)})
                    rows.Add(newrow)
                End If

                For i = 0 To ri.Count - 1
                    If i > 0 Then
                        If ri(i).Top > ri(i - 1).Bottom + 1 Then
                            Dim newrow As New RowInfo
                            newrow.Keys.Add(New KeyInfo With {.Type = KeyInfo.KeyType.S,
                                         .Rect = New Rectangle(0, ri(i - 1).Bottom + 1, ki.Image.Width, ri(i).Top - ri(i - 1).Bottom - 1)})
                            rows.Add(newrow)
                        End If
                    End If

                    Dim row As New RowInfo

                    If ri(i).Keys(0).Rect.Left > 0 Then
                        row.Keys.Add(New KeyInfo With {.Type = KeyInfo.KeyType.S,
                                         .Rect = New Rectangle(0, ri(i).Top, ri(i).Keys(0).Rect.Left, ri(i).Height)})
                    End If
                    For j = 0 To ri(i).Keys.Count - 1
                        If j > 0 Then
                            If ri(i).Keys(j).Rect.Left > ri(i).Keys(j - 1).Rect.Right + 1 Then
                                row.Keys.Add(New KeyInfo With {.Type = KeyInfo.KeyType.S,
                                                 .Rect = New Rectangle(ri(i).Keys(j - 1).Rect.Right + 1, ri(i).Top, ri(i).Keys(j).Rect.Left - ri(i).Keys(j - 1).Rect.Right - 1, ri(i).Height)})
                            End If
                        End If
                        row.Keys.Add(ri(i).Keys(j))
                    Next

                    If ri(i).Keys.Last.Rect.Right + 1 < ki.Image.Width Then
                        row.Keys.Add(New KeyInfo With {.Type = KeyInfo.KeyType.S,
                                             .Rect = New Rectangle(ri(i).Keys.Last.Rect.Right + 1, ri(i).Top, ki.Image.Width - ri(i).Keys.Last.Rect.Right - 1, ri(i).Height)})
                    End If

                    rows.Add(row)

                Next

                If ri.Last.Bottom < ki.Image.Height Then
                    Dim newrow As New RowInfo
                    newrow.Keys.Add(New KeyInfo With {.Type = KeyInfo.KeyType.S,
                                         .Rect = New Rectangle(0, ri.Last.Bottom + 1, ki.Image.Width, ki.Image.Height - ri.Last.Bottom - 1)})
                    rows.Add(newrow)
                End If

                Dim sb As New StringBuilder
                sb.Append(rows.Count.ToString() + " ")
                For Each row In rows
                    sb.Append(row.Height.ToString() + " ")
                Next
                For Each row In rows
                    sb.Append(row.Keys.Count.ToString() + " ")
                Next
                sb.AppendLine()
                For Each row In rows
                    For Each key In row.Keys
                        sb.Append(key.Rect.Width.ToString() + " " + KeyInfo.TypeName(key.Type) + " ")
                    Next
                    sb.AppendLine()
                Next
                File.WriteAllText(Path.Combine("result", fi.Name + ".txt"), sb.ToString())
            Else
                File.WriteAllText(Path.Combine("result", fi.Name + ".txt"), "no keyboard")
            End If

            Dim myimg = img

            Using g = Graphics.FromImage(myimg)
                Dim font As New Font("Consolas", 30)

                g.DrawRectangle(Pens.Pink, ki.Range)

                g.TranslateTransform(ki.Range.Location.X, ki.Range.Location.Y)

                For Each blk In ki.KeyW
                    g.DrawRectangle(Pens.Red, blk.Rect)
                    g.DrawString("W", font, Brushes.Red, blk.Rect.Location)
                Next
                For Each blk In ki.KeyG
                    g.DrawRectangle(Pens.Green, blk.Rect)
                    g.DrawString("G", font, Brushes.Green, blk.Rect.Location)
                Next
                For Each blk In ki.KeyB
                    g.DrawRectangle(Pens.Blue, blk.Rect)
                    g.DrawString("B", font, Brushes.Blue, blk.Rect.Location)
                Next
                For Each blk In ki.KeyG2
                    g.DrawRectangle(Pens.Green, blk.Rect)
                    g.DrawString("G", font, Brushes.Green, blk.Rect.Location)
                Next

                g.ResetTransform()
            End Using

            ki.ImageData.ToBitmap().Save(Path.Combine("result", fi.Name + ".g.png"))
            myimg.Save(Path.Combine("result", fi.Name + ".png"))

            'picMain.BeginInvoke(Sub()
            '                        picMain.Image = myimg
            '                        picMain.Refresh()
            '                    End Sub)
        End Using
        'Return
    End Sub

    Private Sub Worker(files As Queue(Of FileInfo))
        Do While True
            Dim fi As FileInfo = Nothing
            SyncLock files
                If files.Count = 0 Then
                    Exit Do
                End If
                fi = files.Dequeue()
            End SyncLock

            If fi IsNot Nothing Then
                Try
                    Process(fi)
                Catch ex As Exception
                    Debug.Print(ex.ToString())
                    SyncLock files
                        files.Enqueue(fi)
                    End SyncLock
                End Try
            End If
        Loop

    End Sub

    Const ThreadCount = 8

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        'Process(New FileInfo("../../../testcases/IMG_0037.png"))
        'Return

        If Not Directory.Exists("result") Then
            Directory.CreateDirectory("result")
        End If

        Dim di As New DirectoryInfo("testcases")
        If Not di.Exists() Then
            di = New DirectoryInfo("../../../testcases")
        End If

        Dim Q As New Queue(Of FileInfo)
        Dim t(ThreadCount - 1) As Thread

        Dim files = di.GetFiles()

        For i = 0 To files.Count - 1
            Q.Enqueue(files(i))
        Next

        For i = 0 To ThreadCount - 1
            t(i) = New Thread(AddressOf Worker)
            t(i).Start(Q)
        Next

        For i = 0 To ThreadCount - 1
            t(i).Join()
        Next
    End Sub

    Private Sub picMain_SizeChanged(sender As Object, e As EventArgs) Handles picMain.SizeChanged
        PicResize()
    End Sub
End Class
