﻿Imports System.Runtime.InteropServices

Public Class BinaryData

    Public data As Boolean(,)
    Public xDPI, yDPI As Integer

    Public Sub New(width As Integer, height As Integer, xDPI As Integer, yDPI As Integer)
        ReDim data(height - 1, width - 1)
        Me.xDPI = xDPI
        Me.yDPI = yDPI
    End Sub

    Default Public Property raw(y As Integer, x As Integer) As Boolean
        Get
            Return data(y, x)
        End Get
        Set(value As Boolean)
            data(y, x) = value
        End Set
    End Property

    Public ReadOnly Property Width As Integer
        Get
            Return data.GetUpperBound(1) + 1
        End Get
    End Property

    Public ReadOnly Property Height As Integer
        Get
            Return data.GetUpperBound(0) + 1
        End Get
    End Property

    Public Shared Function FromBitmap(bmp As Bitmap, T As Double) As BinaryData
        Dim rd = RawData.FromBitmap(bmp)
        Return FromRawData(rd, T)
    End Function

    Public Shared Function FromBitmap(bmp As Bitmap, T1 As Double, T2 As Double) As BinaryData
        Dim rd = RawData.FromBitmap(bmp)
        Return FromRawData(rd, T1, T2)
    End Function

    Public Shared Function FromBitmap(bmp As Bitmap, T As Func(Of Integer, Integer, Integer, Boolean)) As BinaryData
        Dim rd = RawData.FromBitmap(bmp)
        Return FromRawData(rd, T)
    End Function

    Public Shared Function FromBitmap(bmp As Bitmap) As BinaryData
    Dim rd = RawData.FromBitmap(bmp)
        Return FromRawData(rd)
    End Function

    Public Shared Function FromRawData(rd As RawData, T As Double) As BinaryData
        Dim gd = GrayData.FromRawData(rd)
        Return FromGrayData(gd, T)
    End Function

    Public Shared Function FromRawData(rd As RawData, T1 As Double, T2 As Double) As BinaryData
        Dim gd = GrayData.FromRawData(rd)
        Return FromGrayData(gd, T1, T2)
    End Function

    Public Shared Function FromRawData(rd As RawData) As BinaryData
        Dim gd = GrayData.FromRawData(rd)
        Dim T = ImageProcessor.FindBinarizationThreshold(gd)
        Return FromGrayData(gd, T)
    End Function

    Public Shared Function FromRawData(r As RawData, T As Func(Of Integer, Integer, Integer, Boolean)) As BinaryData
        'Dim T = FindThreshold(gr, acc)
        Dim bin As New BinaryData(r.Width, r.Height, r.xDPI, r.yDPI)
        For y = 0 To r.Height - 1
            For x = 0 To r.Width - 1
                bin(y, x) = T(r(y, x, 0), r(y, x, 1), r(y, x, 2))
            Next
        Next
        Return bin
    End Function

    Public Shared Function FromGrayData(gr As GrayData, Optional T As Double = 128) As BinaryData
        'Dim T = FindThreshold(gr, acc)
        Dim bin As New BinaryData(gr.Width, gr.Height, gr.xDPI, gr.yDPI)
        For y = 0 To gr.Height - 1
            For x = 0 To gr.Width - 1
                bin(y, x) = gr(y, x) < T
            Next
        Next
        Return bin
    End Function

    Public Shared Function FromGrayData(gr As GrayData, Optional T1 As Double = 128, Optional T2 As Double = 128) As BinaryData
        'Dim T = FindThreshold(gr, acc)
        Dim bin As New BinaryData(gr.Width, gr.Height, gr.xDPI, gr.yDPI)
        For y = 0 To gr.Height - 1
            For x = 0 To gr.Width - 1
                bin(y, x) = gr(y, x) >= T1 AndAlso gr(y, x) <= T2
            Next
        Next
        Return bin
    End Function

    Public Function ToBitmap() As Bitmap
        Dim rd = RawData.Create(Width, Height, xDPI, yDPI)
        For y = 0 To rd.Height - 1
            For x = 0 To rd.Width - 1
                Dim a = IIf(data(y, x), 0, 255)
                rd(y, x, 0) = a
                rd(y, x, 1) = a
                rd(y, x, 2) = a
            Next
        Next
        Return rd.ToBitmap()
    End Function

End Class
