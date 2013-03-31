Imports System.Net

Namespace CloudFiles

    Public Class FileList

        ' Represents a list of files from Cloud Files.
        ' You will have two separate lists, possibly. One from
        ' Dallas datacenter and one from Chicago.

        ' Built the same as ContainerList...

        Private pFiles As String()

        Public ReadOnly Property Files() As String()
            Get
                Return pFiles
            End Get
        End Property

        Sub New(ByVal FileListResponse As WebResponse)

            Dim Reader As New IO.StreamReader(FileListResponse.GetResponseStream)
            pFiles = Reader.ReadToEnd.Split(vbNewLine)
            Reader.Close()
            Reader.Dispose()

        End Sub

    End Class

End Namespace