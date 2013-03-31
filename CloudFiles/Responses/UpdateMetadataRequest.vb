Imports System.Net

Namespace CloudFiles

    Public Enum UpdateContainerMetadataResult

        ' Enum indicated whether a container's metadata was successfully
        ' updated or not.

        ContainerMetadataUpdatedSuccessfully = 204
        ContainerNotUpdatedDoesNotExist = 404
        UnknownProbableFailure = -1

    End Enum

End Namespace