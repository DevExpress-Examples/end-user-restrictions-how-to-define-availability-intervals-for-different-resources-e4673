Imports System
Imports System.Windows.Media
Imports DevExpress.XtraScheduler

Namespace SchedulerResourcesAvailabilitiesWpf
    Public Class CellColorConverter
        Inherits System.Windows.Markup.MarkupExtension
        Implements System.Windows.Data.IMultiValueConverter

        Public Function Convert(ByVal values() As Object, ByVal targetType As Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IMultiValueConverter.Convert
            Dim intervalStart As Date = System.Convert.ToDateTime(values(0))
            Dim intervalEnd As Date = System.Convert.ToDateTime(values(1))
            Dim resourceId As String = System.Convert.ToString(values(2))
            Dim defaultBrush As Brush = DirectCast(values(3), Brush)

            If intervalStart > intervalEnd Then
                Dim temp As Date = intervalStart
                intervalStart = intervalEnd
                intervalEnd = temp
            End If

            Dim available As Boolean = ResourcesAvailabilities.IsIntervalAvailableForResource(resourceId, New TimeInterval(intervalStart, intervalEnd))

            If available Then
                Return defaultBrush
            Else
                Return defaultBrush.SetBrightness(0.85)
            End If
        End Function

        Public Function ConvertBack(ByVal value As Object, ByVal targetTypes() As Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object() Implements System.Windows.Data.IMultiValueConverter.ConvertBack
            Throw New NotImplementedException("DebugConverterFormatter::ConvertBack")
        End Function

        Public Overrides Function ProvideValue(ByVal serviceProvider As IServiceProvider) As Object
            Return Me
        End Function
    End Class

    Public Class HSBColor
        Public Property H() As Double
        Public Property S() As Double
        Public Property B() As Double
        Public Property A() As Byte

        Public Shared Function FromColor(ByVal rgbColor As Color) As HSBColor
            Dim result As New HSBColor()

            ' preserve alpha
            result.A = rgbColor.A

            ' convert R, G, B to numbers from 0 to 1
            Dim r As Double = rgbColor.R / 255R
            Dim g As Double = rgbColor.G / 255R

            Dim b_Renamed As Double = rgbColor.B / 255R

            Dim max As Double = Math.Max(r, Math.Max(g, b_Renamed))
            Dim min As Double = Math.Min(r, Math.Min(g, b_Renamed))

            ' hue
            If max = min Then
                result.H = 0
            ElseIf max = r Then
                result.H = (60 * (g - b_Renamed) / (max - min) + 360) Mod 360
            ElseIf max = g Then
                result.H = 60 * (b_Renamed - r) / (max - min) + 120
            Else
                result.H = 60 * (r - g) / (max - min) + 240
            End If

            ' saturation
            If max = 0 Then
                result.S = 0
            Else
                result.S = 1 - min / max
            End If

            ' brightness
            result.B = max

            Return result
        End Function

        Public Function ToColor() As Color
            Dim result As New Color()

            result.A = Me.A

            Dim hi As Integer = CInt((Math.Floor(Me.H / 60))) Mod 6
            Dim f As Double = Me.H / 60 - Math.Floor(Me.H / 60)

            Dim p As Double = Me.B * (1 - Me.S)
            Dim q As Double = Me.B * (1 - f * Me.S)
            Dim t As Double = Me.B * (1 - (1 - f) * Me.S)

            Select Case hi
                Case 0
                    result.R = CByte(Me.B * 255)
                    result.G = CByte(t * 255)
                    result.B = CByte(p * 255)
                Case 1
                    result.R = CByte(q * 255)
                    result.G = CByte(Me.B * 255)
                    result.B = CByte(p * 255)
                Case 2
                    result.R = CByte(p * 255)
                    result.G = CByte(Me.B * 255)
                    result.B = CByte(t * 255)
                Case 3
                    result.R = CByte(p * 255)
                    result.G = CByte(q * 255)
                    result.B = CByte(Me.B * 255)
                Case 4
                    result.R = CByte(t * 255)
                    result.G = CByte(p * 255)
                    result.B = CByte(Me.B * 255)
                Case 5
                    result.R = CByte(Me.B * 255)
                    result.G = CByte(p * 255)
                    result.B = CByte(q * 255)
            End Select

            Return result
        End Function
    End Class

    Public Module HelperExtensions
        <System.Runtime.CompilerServices.Extension> _
        Public Function SetBrightness(ByVal original As Brush, ByVal brightness As Double) As Brush
            If brightness < 0 OrElse brightness > 1 Then
                Throw New ArgumentOutOfRangeException("brightness", "brightness should be between 0 and 1")
            End If

            Dim result As Brush

            If TypeOf original Is SolidColorBrush Then
                Dim hsb As HSBColor = HSBColor.FromColor(CType(original, SolidColorBrush).Color)
                hsb.B = brightness
                result = New SolidColorBrush(hsb.ToColor())
            ElseIf TypeOf original Is GradientBrush Then
                result = original.Clone()
                ' change brightness of every gradient stop
                For Each gs As GradientStop In CType(result, GradientBrush).GradientStops
                    Dim hsb As HSBColor = HSBColor.FromColor(gs.Color)
                    hsb.B = brightness
                    gs.Color = hsb.ToColor()
                Next gs
            Else
                result = original.Clone()
            End If

            Return result
        End Function
    End Module
End Namespace