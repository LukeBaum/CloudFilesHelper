Imports System.Net

Namespace CloudFiles

    Public Class ObjectMetadata

        ' A class used for sending updated Container Metadata to Cloud Files.

        Private pHeaders As New List(Of X_Object_Meta_Header)

        Public Property Headers As List(Of X_Object_Meta_Header)
            Get
                Return Me.pHeaders
            End Get
            Set(value As List(Of X_Object_Meta_Header))
                pHeaders = value
            End Set
        End Property

    End Class

    Public Class X_Object_Meta_Header

        ' Represents and individual Metadata record.

        Private pNameNotIncludingPrefix As String
        Private pValue As String

        Public Property Value As String
            Get
                Return pValue
            End Get
            Set(value As String)
                pValue = value
            End Set
        End Property

        Public Property NameNotIncludingPrefix As String
            Get
                Return pNameNotIncludingPrefix
            End Get
            Set(value As String)
                pNameNotIncludingPrefix = value
            End Set
        End Property

        Public ReadOnly Property Name As String
            Get
                Return "X-Object-Meta-" & Me.NameNotIncludingPrefix
            End Get
        End Property

        Sub New(ByVal MetadataHeaderNameWithoutPrefix As String, ByVal MetadataValue As String)

            Me.pNameNotIncludingPrefix = MetadataHeaderNameWithoutPrefix
            Me.pValue = MetadataValue

        End Sub

    End Class

End Namespace