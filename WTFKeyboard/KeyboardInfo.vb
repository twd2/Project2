Public Class KeyboardInfo

    Public Range As Rectangle
    Public Image As Bitmap
    Public ImageData As GrayData
    Public ImageBG, ImageW, ImageB, ImageG, ImageG2 As BinaryData
    Public KeyW, KeyB, KeyG, KeyG2 As List(Of Block)

    Public Sub New()

    End Sub

    Public Function GetAbsLocation(p As Point) As Point
        Return Range.Location + p
    End Function

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

    Private Sub MyBubbleSort(Of T)(lst As List(Of T), comp As Func(Of T, T, Boolean))
        For i = 0 To lst.Count - 1
            For j = i + 1 To lst.Count - 1
                If Not comp(lst(i), lst(j)) Then
                    Dim tmp As T
                    tmp = lst(i)
                    lst(i) = lst(j)
                    lst(j) = tmp
                End If
            Next
        Next
    End Sub


    Public Function ToRows() As List(Of RowInfo)
        Dim keys = ToKeys()

        keys.Sort(Function(a As KeyInfo, b As KeyInfo) As Boolean
                      Return a.Rect.Top < b.Rect.Top
                  End Function)

        Dim rows As New List(Of RowInfo)
        Dim i = 0
        Do While i < keys.Count
            Dim row As New RowInfo

            row.Keys.Add(keys(i))
            i += 1

            Do While i < keys.Count AndAlso Math.Abs(keys(i).Block.Top - row.Top) <= 5
                row.Keys.Add(keys(i))
                i += 1
            Loop

            row.Keys.Sort(Function(a As KeyInfo, b As KeyInfo) As Boolean
                              Return a.Rect.Left < b.Rect.Left
                          End Function)
            rows.Add(row)
        Loop

        Return rows
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

        ki.ImageBG = BinaryData.FromBitmap(img, options.BackgounrdMin, options.BackgroundMax)

        Dim blks = ImageProcessor.FindBlocks(ki.ImageBG)
        blks.Sort()
        blks.Reverse()
        Dim blkkb = blks(0) 'assume the biggest gray block is the keyboard area
        ki.Range = blkkb.Rect

        ki.Image = ImageProcessor.CutImage(img, ki.Range)
        ki.ImageData = GrayData.FromBitmap(ki.Image)

        Dim testFunc =
            Function(b As Block)
                Return (b.Acreage >= options.MinAcreage OrElse options.MinAcreage <= 0) AndAlso
                            (b.Acreage <= options.MaxAcreage OrElse options.MaxAcreage <= 0)
            End Function

        Dim removeFunc =
            Function(b As Block)
                Return Not testFunc(b)
            End Function

        ki.ImageW = BinaryData.FromGrayData(ki.ImageData, options.WhiteMin, options.WhiteMax) 'white key
        ki.KeyW = ImageProcessor.FindBlocks(ki.ImageW)
        ki.KeyW.RemoveAll(removeFunc)

        ki.ImageG = BinaryData.FromGrayData(ki.ImageData, options.GrayMin, options.GrayMax) 'gray key
        ki.KeyG = ImageProcessor.FindBlocks(ki.ImageG)
        ki.KeyG.RemoveAll(removeFunc)

        ki.ImageB = BinaryData.FromGrayData(ki.ImageData, options.BlueMin, options.BlueMax) 'blue key
        ki.KeyB = ImageProcessor.FindBlocks(ki.ImageB)
        ki.KeyB.RemoveAll(removeFunc)

        ki.ImageG2 = BinaryData.FromGrayData(ki.ImageData, options.Gray2Min, options.Gray2Max) 'gray2 key
        ki.KeyG2 = ImageProcessor.FindBlocks(ki.ImageG2)
        ki.KeyG2.RemoveAll(removeFunc)

        Return ki
    End Function


End Class
