Public Class RowInfo

    Public Keys As New List(Of KeyInfo)

    Public ReadOnly Property Top As Integer
        Get
            Return Keys(0).Rect.Top
        End Get
    End Property

    Public ReadOnly Property Height As Integer
        Get
            Return Keys(0).Rect.Height
        End Get
    End Property

    Public ReadOnly Property Bottom As Integer
        Get
            Return Keys(0).Rect.Bottom
        End Get
    End Property

End Class
