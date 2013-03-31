Namespace CloudFiles

    Public Enum StoreFileResult

        ' The result after trying to store a file (an object) in Cloud Files.

        FileStoredSuccessfully = 201
        FileNotStoredAuthenticationFailed = 401
        FileNotStoredChecksumDoesNotMatch = 422
        FileNotStoredLocalFileDoesNotExist = -1
        UnknownProbableFailure = -2

    End Enum

End Namespace


