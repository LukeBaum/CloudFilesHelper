Imports System.Net

Namespace CloudFiles

    Public Enum CdnEnableContainerResult

        ' Enum indicated whether a container was CDN-enabled or not.

        CdnEnabledContainerSuccessfully = 201
        ContainerAlreadyCdnEnabled = 202
        UnknownResultProbableFailure = -1

    End Enum

End Namespace