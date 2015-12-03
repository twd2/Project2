Public Class KeyInfo

    Enum KeyType
        W
        G
        B
        G2
        S 'space
    End Enum

    Public Shared Function TypeName(type As KeyType) As String
        Select Case type
            Case KeyType.W
                Return "W"
            Case KeyType.B
                Return "B"
            Case KeyType.G
                Return "G"
            Case KeyType.G2
                Return "G"
            Case KeyType.S
                Return "S"
            Case Else
                Return ""
        End Select
    End Function

    Public Type As KeyType
    Public Block As Block
    Public Rect As Rectangle

End Class
