Public Class KeyboardInfo

    Public Range As Rectangle
    Public Image As Bitmap
    Public ImageW, ImageB, ImageG, ImageG2 As BinaryData
    Public KeyW, KeyB, KeyG, KeyG2 As List(Of Block)

    Public Sub New()

    End Sub

    Public Function GetAbsRect(rect As Rectangle) As Rectangle
        Return New Rectangle(rect.Location + Range.Location, rect.Size)
    End Function

    Private Shared Sub AddKey(ki As List(Of KeyInfo), blk As Block, type As KeyInfo.KeyType)
        Dim k As New KeyInfo()
        k.Block = blk
        k.Rect = blk.Rect
        k.Type = type
        ki.Add(k)
    End Sub

    Private Sub mySort(keys As List(Of KeyInfo), comp As Func(Of KeyInfo, KeyInfo, Boolean))
        For i = 0 To keys.Count - 1
            For j = i + 1 To keys.Count - 1
                If Not comp(keys(i), keys(j)) Then
                    Dim tmp As KeyInfo
                    tmp = keys(i)
                    keys(i) = keys(j)
                    keys(j) = tmp
                End If
            Next
        Next
    End Sub


    Public Function ToRows() As List(Of RowInfo)
        Dim keys = ToKeys()

        'why this sort is bad
        'keys.Sort(Function(a As KeyInfo, b As KeyInfo) As Boolean
        '              Return (a.Rect.Top < b.Rect.Top) OrElse
        '              (a.Rect.Top = b.Rect.Top AndAlso a.Rect.Left < b.Rect.Left)
        '          End Function)

        mySort(keys, Function(a As KeyInfo, b As KeyInfo) As Boolean
                         Return (a.Rect.Top < b.Rect.Top) OrElse
                         (a.Rect.Top = b.Rect.Top AndAlso a.Rect.Left < b.Rect.Left)
                     End Function)

        Dim ri As New List(Of RowInfo)
        Dim i = 0
        Do While i < keys.Count
            Dim row As New RowInfo
            row.Keys.Add(keys(i))
            i += 1
            Do While i < keys.Count AndAlso Math.Abs(keys(i).Block.Top - row.Top) <= 1
                row.Keys.Add(keys(i))
                i += 1
            Loop

            If row.Keys.Count > 0 Then
                ri.Add(row)
            End If
        Loop

        Return ri
    End Function

    Public Function ToKeys() As List(Of KeyInfo)
        Dim ki As New List(Of KeyInfo)

        For Each k In KeyW
            AddKey(ki, k, KeyInfo.KeyType.W)
        Next
        For Each k In KeyB
            AddKey(ki, k, KeyInfo.KeyType.B)
        Next
        For Each k In KeyG
            AddKey(ki, k, KeyInfo.KeyType.G)
        Next
        For Each k In KeyG2
            AddKey(ki, k, KeyInfo.KeyType.G2)
        Next

        Return ki
    End Function

    Public Shared Function FromImage(img As Bitmap, Optional options As KeyboardSplitOptions = Nothing) As KeyboardInfo
        If options Is Nothing Then
            options = KeyboardSplitOptions.Default
        End If

        Dim ki As New KeyboardInfo

        Dim gdata = GrayData.FromBitmap(img)
#If DEBUG Then
        gdata.ToBitmap().Save("a.g.png")
#End If
        Dim data = BinaryData.FromGrayData(gdata, options.BackgounrdMin, options.BackgroundMax)

        Dim blks = ImageProcessor.FindBlocks(data)
        blks.Sort()
        blks.Reverse()
        Dim blkkb = blks(0) 'assume the biggest gray block is the keyboard area
        ki.Range = blkkb.Rect

        ki.Image = ImageProcessor.CutImage(img, ki.Range)

        Dim testfunc =
            Function(b As Block)
                Return (b.Acreage >= options.MinAcreage OrElse options.MinAcreage <= 0) AndAlso
                            (b.Acreage <= options.MaxAcreage OrElse options.MaxAcreage <= 0)
            End Function

        Dim removefunc =
            Function(b As Block)
                Return Not testfunc(b)
            End Function

        Dim dataW = BinaryData.FromBitmap(ki.Image, options.WhiteMin, options.WhiteMax) 'white key
        ki.KeyW = ImageProcessor.FindBlocks(dataW)
        ki.KeyW.RemoveAll(removefunc)

        Dim dataG = BinaryData.FromBitmap(ki.Image, options.GrayMin, options.GrayMax) 'gray key
        ki.KeyG = ImageProcessor.FindBlocks(dataG)
        ki.KeyG.RemoveAll(removefunc)

        Dim dataB = BinaryData.FromBitmap(ki.Image, options.BlueMin, options.BlueMax) 'blue key
        ki.KeyB = ImageProcessor.FindBlocks(dataB)
        ki.KeyB.RemoveAll(removefunc)

        Dim dataG2 = BinaryData.FromBitmap(ki.Image, options.Gray2Min, options.Gray2Max) 'gray key
        ki.KeyG2 = ImageProcessor.FindBlocks(dataG2)
        ki.KeyG2.RemoveAll(removefunc)

        Return ki
    End Function


End Class
