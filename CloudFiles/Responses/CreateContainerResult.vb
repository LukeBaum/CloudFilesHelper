Imports System.Net

Namespace CloudFiles

    Public Enum CreateContainerResult

        ' Enum indicated whether a container was created or not.

        ContainerCreatedSuccessfully = 201
        ContainerAlreadyExists = 202
        UnknownResultProbableFailure = -1

    End Enum

End Namespace