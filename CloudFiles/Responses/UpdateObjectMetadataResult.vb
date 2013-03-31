Imports System.Net

Namespace CloudFiles

    Public Enum UpdateObjectMetadataResult

        ' Enum indicated whether a container's metadata was successfully
        ' updated or not.

        ObjectMetadataUpdatedSuccessfully = 204
        ObjectNotUpdatedDoesNotExist = 404
        UnknownProbableFailure = -1

    End Enum

End Namespace