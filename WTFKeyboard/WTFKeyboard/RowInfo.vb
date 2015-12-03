Public Class RowInfo

    Public Keys As New List(Of KeyInfo)

    ReadOnly Property Top As Integer
        Get
            Return Keys(0).Rect.Top
        End Get
    End Property

    ReadOnly Property Height As Integer
        Get
            Return Keys(0).Rect.Height
        End Get
    End Property

    ReadOnly Property Bottom As Integer
        Get
            Return Keys(0).Rect.Bottom
        End Get
    End Property

End Class
