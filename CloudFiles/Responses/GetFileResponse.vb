Namespace CloudFiles

    Public Enum GetFileResponse

        ' Indicates whether or not the file was written to the filesystem.

        FileSuccessfullyWritten = 1
        FileNotWrittenAlreadyExistsLocally = 2
        FileNotWrittenDoesNotExistInCloudFiles = 3
        FileNotWrittenInvalidDestinationFolder = 4
        FileNotWrittenPermissionsOrIoException = 5
        FileNotWrittenUnknown = -1

    End Enum

End Namespace