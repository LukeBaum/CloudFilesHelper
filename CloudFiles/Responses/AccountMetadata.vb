Imports System.Net

Namespace CloudFiles

    Public Class AccountMetadata

        ' A class that contains information returned when you ask Rackspace about general account
        ' metedata.

        Private pAccountObjectCount As Long = Nothing
        Private pTransactionId As String = Nothing
        Private pTransactionDate As DateTime = Nothing
        Private pAccountBytesUsed As Long = Nothing
        Private pAccountContainerCount As Long = Nothing

        Public ReadOnly Property AccountObjectCount As Long
            Get
                Return pAccountObjectCount
            End Get
        End Property

        Public ReadOnly Property TransactionId As String
            Get
                Return pTransactionId
            End Get
        End Property

        Public ReadOnly Property TransactionDate As DateTime
            Get
                Return pTransactionDate
            End Get
        End Property

        Public ReadOnly Property AccountBytesUsed As Long
            Get
                Return pAccountBytesUsed
            End Get
        End Property

        Public ReadOnly Property AccountContainerCount As Long
            Get
                Return pAccountContainerCount
            End Get
        End Property

        Sub New(ByVal MetadataResponse As WebResponse)

            ' Parse the web response and set the appropriate properties.

            Me.pAccountObjectCount = CLng(MetadataResponse.Headers("X-Account-Object-Count"))
            Me.pTransactionId = MetadataResponse.Headers("X-Trans-Id")
            Me.pTransactionDate = CDate(MetadataResponse.Headers("Date"))
            Me.pAccountBytesUsed = CLng(MetadataResponse.Headers("X-Account-Bytes-Used"))
            Me.pAccountContainerCount = CLng(MetadataResponse.Headers("X-Account-Container-Count"))

        End Sub

    End Class

End Namespace
