Imports System.Net

Namespace CloudFiles

    Public Class ContainerList

        ' Represents a list of containers from Cloud Files.
        ' You will have two separate lists, possibly. One from
        ' Dallas datacenter and one from Chicago.

        Private pContainers As String()

        Public ReadOnly Property Containers() As String()
            Get
                Return pContainers
            End Get
        End Property

        Sub New(ByVal ContainerListResponse As WebResponse)

            Dim Reader As New IO.StreamReader(ContainerListResponse.GetResponseStream)
            pContainers = Reader.ReadToEnd.Split(vbNewLine)
            Reader.Close()
            Reader.Dispose()

        End Sub

    End Class

End Namespace