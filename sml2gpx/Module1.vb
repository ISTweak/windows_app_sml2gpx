Imports System.Xml
Imports System.Xml.Serialization

Public Class extension
    <XmlElement(Namespace:="http://www.cluetrust.com/XML/GPXDATA/1/0")> Public temp As Double
    <XmlElement(Namespace:="http://www.cluetrust.com/XML/GPXDATA/1/0")> Public seaLevelPressure As Integer
End Class

Public Class trkpt
    <System.Xml.Serialization.XmlAttribute("lat")> Public lat As Double
    <System.Xml.Serialization.XmlAttribute("lon")> Public lon As Double
    Public ele As Double
    Public time As String
    Public extensions As New extension
End Class

Public Class trk
    Public name As String
    Public trkseg As New List(Of trkpt)()
End Class

Public Class gpx
    Public trk As New trk
End Class

Module Module1
    Sub Main(args As String())
        If args.Length = 0 Then
            OpenDir()
        Else
            For Each fn As String In args
                If fn.Substring(fn.Length - 3).ToLower = "sml" Then
                    ParseSml(fn)
                End If
            Next
        End If
    End Sub

    Private Sub OpenDir()
        Dim path As String = System.Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) & "\AppData\Roaming\Suunto\Moveslink2"
        If System.IO.Directory.Exists(path) Then
            System.Diagnostics.Process.Start("explorer.exe", """" & path & """")
        End If
    End Sub

    Private Sub ParseSml(fn As String)
        Dim lp As Integer = 0
        Dim tp As Double = 0.0
        Dim xgpx As New trk
        Dim xmlroot As New gpx
        Dim xmlDoc As New XmlDocument
        Dim ns As New XmlSerializerNamespaces

        xgpx.name = "Move"
        xmlDoc.Load(fn)

        Dim xmlSamples As XmlNodeList = xmlDoc.GetElementsByTagName("Sample")
        For Each xmlSample As XmlNode In xmlSamples
            Dim nodeSaml As XmlNodeList = xmlSample.ChildNodes
            For Each SamChiple As XmlNode In nodeSaml
                Select Case SamChiple.Name
                    Case "SeaLevelPressure"
                        lp = SamChiple.InnerText
                    Case "Temperature"
                        tp = K2C(SamChiple.InnerText)
                    Case "SampleType"
                        If SamChiple.InnerText = "gps-base" Then
                            Dim tk As trkpt = ConvNode(nodeSaml)
                            tk.extensions.seaLevelPressure = lp
                            tk.extensions.temp = tp
                            xgpx.trkseg.Add(tk)
                        End If
                End Select
            Next
        Next

        xmlroot.trk = xgpx
        ns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance")
        ns.Add("gpxdata", "http://www.cluetrust.com/XML/GPXDATA/1/0")
        ns.Add("gpxtpx", "http://www.garmin.com/xmlschemas/TrackPointExtension/v1")

        Dim serializer1 As New System.Xml.Serialization.XmlSerializer(GetType(gpx))
        Dim sw As New System.IO.StreamWriter(fn.Replace(".sml", ".gpx"), False, New System.Text.UTF8Encoding(False))
        serializer1.Serialize(sw, xmlroot, ns)
        sw.Close()
    End Sub

    Private Function ConvNode(nl As XmlNodeList) As trkpt
        Dim tk As New trkpt
        For Each chiled As XmlNode In nl
            Select Case chiled.Name
                Case "GPSAltitude"
                    tk.ele = chiled.InnerText
                Case "Latitude"
                    tk.lat = ToDegrees(chiled.InnerText)
                Case "Longitude"
                    tk.lon = ToDegrees(chiled.InnerText)
                Case "UTC"
                    tk.time = chiled.InnerText
            End Select
        Next
        Return tk
    End Function

    Private Function K2C(val As Double) As Double
        Return val - 273.15
    End Function

    Private Function ToDegrees(radians As Double) As Double
        Return radians * 180.0 / Math.PI
    End Function
End Module
