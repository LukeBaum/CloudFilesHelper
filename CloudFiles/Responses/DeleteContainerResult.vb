Imports System.Net

Namespace CloudFiles

    Public Enum DeleteContainerResult

        ' Enum indicated whether a container was deleted or not.

        ContainerDeletedSuccessfully = 204
        ContainerDidNotExist = 404
        ContainerNotDeletedWasNotEmpty = 409
        UnknownProbableFailure = -1

    End Enum

End Namespace