Imports System.IO
Imports System.Text

Public Class frmMain

    Dim _imageBuffer As Bitmap = Nothing
    Dim _g As Graphics = Nothing

    Private Sub frmMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub

    Private Sub picBox_Click(sender As Object, e As EventArgs) Handles picBox.Click

    End Sub

    Private Sub picBox_Resize(sender As Object, e As EventArgs) Handles picBox.Resize
        If _g IsNot Nothing Then
            _g.Dispose()
            _g = Nothing
        End If
        If _imageBuffer IsNot Nothing Then
            _imageBuffer.Dispose()
            _imageBuffer = Nothing
        End If

        _imageBuffer = New Bitmap(1024, 1024)
        _g = Graphics.FromImage(_imageBuffer)
        picBox.Image = _imageBuffer

        _g.Clear(Color.White)
        _g.DrawString("hello, world", New Font("Consolas", 13), Brushes.Black, 0, 0)
        picBox.Refresh()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        For Each filename In Directory.GetFiles("testcases")
            Dim fi As New FileInfo(filename)

            Dim ext = fi.Extension.ToLower()
            If ext <> ".png" AndAlso ext <> ".jpg" Then
                Continue For
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
                End If

                Using g = Graphics.FromImage(img)
                    g.DrawRectangle(Pens.Pink, ki.Range)
                    For Each blk In ki.KeyW
                        g.DrawRectangle(Pens.Red, ki.GetAbsRect(blk.Rect))
                    Next
                    For Each blk In ki.KeyG
                        g.DrawRectangle(Pens.Green, ki.GetAbsRect(blk.Rect))
                    Next
                    For Each blk In ki.KeyB
                        g.DrawRectangle(Pens.Blue, ki.GetAbsRect(blk.Rect))
                    Next
                    For Each blk In ki.KeyG2
                        g.DrawRectangle(Pens.Green, ki.GetAbsRect(blk.Rect))
                    Next
                End Using
                img.Save(Path.Combine("result", fi.Name + ".png"))
                _g.DrawImage(img, 0, 0)
                picBox.Refresh()
            End Using
            'Return
        Next
    End Sub
End Class
