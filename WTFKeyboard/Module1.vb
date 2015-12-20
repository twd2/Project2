Imports System.IO
Imports System.Text

Module Module1

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
            Console.WriteLine(sb0.ToString())
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
                File.WriteAllText(fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length) + ".txt", sb.ToString())
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
        End Using
        'Return
    End Sub

    Sub Main(args() As String)
        Console.WriteLine("config: list.txt")
        Dim files = File.ReadAllLines("list.txt")
        For Each filename In files
            Dim fi As New FileInfo(filename)
            Console.WriteLine("processing {0}", fi.Name)
            If fi.Extension.ToLower() <> ".png" Then
                Continue For
            End If
            Process(fi)
        Next
        Console.WriteLine("done.")
        Console.ReadKey()
    End Sub

End Module
