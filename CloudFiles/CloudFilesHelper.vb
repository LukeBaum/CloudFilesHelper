Imports System.Net

Namespace CloudFiles

    Public Class CloudFilesHelper

        ' 3/27/2013

        ' A class that does the heavy lifting dealing with Cloud Files for you.
        ' Created because OpenStack.NET's support for Cloud Files doesn't exist yet.

        ' If the official OpenStack.NET project supports Cloud Files in the future,
        ' you may consider using that instead as it appears to be from and supported
        ' by the Rackspace folks and I can't really support this project.

        ' Check for progress here:
        ' https://github.com/rackspace/openstack.net/wiki/Feature-Support

        ' You are free to use this software under these terms (this is the license):
        ' 1. You can use the binary library or source code, modify the source code, and create
        '    derivative works in any kind of project, totally open or completely commercial.
        ' 2. I'm not responsible for anything that blows up or doesn't work. You cannot
        '    hold me liable for broken code or lost revenue. You have the source code, look
        '    at it.


#Region "Constants"

        ' In here lie constants that will change very rarely. Mostly API URLs that Rackspace may
        ' change in future API versions.
        Private Const AuthenticationUrl As String = "https://identity.api.rackspacecloud.com/v1.0"

#End Region


#Region "Variables"

        ' The Api Key is critical in getting an XAuthToken.
        Private pApiKey As String = Nothing
        Public Property ApiKey As String
            Get
                Return pApiKey
            End Get
            Set(value As String)
                pApiKey = value
            End Set
        End Property

        ' Equally important is the account name, available in the Control Panel.
        ' You cannot get an XAuth Token without both.
        Private pAccountName As String = Nothing
        Public Property AccountName As String
            Get
                Return pAccountName
            End Get
            Set(value As String)
                pAccountName = value
            End Set
        End Property

        ' The XAuthToken is sent in every authenticated call when operating on a container
        ' or file. It expires every 24 hours.
        Private pXAuthToken As String = Nothing
        Public Property XAuthToken As String
            Get
                Return pXAuthToken
            End Get
            Set(value As String)
                pXAuthToken = value
            End Set
        End Property

        ' It is useful to keep track of when the pXAuthToken was received. We know if it's older
        ' than 24 hours, we need to get a new one.
        Private pXAuthTokenReceivedOn As DateTime = Now.Subtract(New TimeSpan(48, 0, 0))
        Public Property XAuthTokenReceivedOn As DateTime
            Get
                Return pXAuthTokenReceivedOn
            End Get
            Set(value As DateTime)
                pXAuthTokenReceivedOn = value
            End Set
        End Property

        ' Let's make things even more convenient and report back whether or not our XAuth token is
        ' expired.
        Public ReadOnly Property XAuthTokenIsExpired As Boolean
            Get
                If DateDiff(DateInterval.Minute, Now, XAuthTokenReceivedOn) > 1439 Then
                    ' The interval is larger than 1439 minutes; we've hit 24 hours.
                    Return True
                Else
                    ' The interval is smaller than or equal to 1439 minutes; 24 hours hasn't passed yet.
                    Return False
                End If
            End Get
        End Property

        ' It is useful to know whether we are working internally or not. That is to say, are we making 
        ' these calls from a Rackspace-hosted server?
        ' If we are, and we use the "internal" hostname, no charges are applied to the account.
        Private pWorkingInternally As Boolean = False
        Public Property WorkingInternally As Boolean
            Get
                Return pWorkingInternally
            End Get
            Set(value As Boolean)
                pWorkingInternally = value
            End Set
        End Property

        ' pStorageUrl is the base string to access containers. Depending on where the container is
        ' stored--Dallas or Chicago--a slightly altered string is necessary.
        Private pStorageUrl As String = Nothing
        Public ReadOnly Property DallasStorageUrl As String
            Get
                If WorkingInternally Then
                    Return pStorageUrl.Replace("<location>", "dfw1").Replace("storage101.", "snet-storage101.")
                Else
                    Return pStorageUrl.Replace("<location>", "dfw1")
                End If
            End Get
        End Property
        Public ReadOnly Property ChicagoStorageUrl As String
            Get
                If WorkingInternally Then
                    Return pStorageUrl.Replace("<location>", "ord1").Replace("storage101.", "snet-storage101.")
                Else
                    Return pStorageUrl.Replace("<location>", "ord1")
                End If
            End Get
        End Property

        ' The CDN Management URL for each datacenter can be useful...
        Private pManagementUrl As String = Nothing

        Public ReadOnly Property DallasManagementUrl As String
            Get
                Return pManagementUrl
            End Get
        End Property

        Public ReadOnly Property ChicagoManagementUrl As String
            Get
                Return pManagementUrl.Replace("//cdn1.", "//cdn2.")
            End Get
        End Property

#End Region


#Region "Methods"

        ' NOTE: makes sure to catch exceptions when using this and any function that makes a call
        ' to Rackspace, because much can go wrong. A bad Internet connection for instance.

        Sub New(ByVal ApiKey As String, ByVal AccountName As String)

            ' A constructor for our helper class.
            ' Store the Account Name and ApiKey.

            ' We're also going to authenticate off the bat to go ahead and get an access token.
            Me.ApiKey = ApiKey
            Me.AccountName = AccountName

            Authenticate()

        End Sub

        Public Sub Authenticate()

            ' Authenticates your Account Name and Api Key against Rackspace's authentication servers.
            ' Hopefully you get an XAuthToken.

            ' Note: To check for actual success (not just an exception) check the value of
            ' XAuthToken and whether or not the date of XAuthTokenReceivedOn is close to Now.

            If Not Me.ApiKey Is Nothing And Not Me.AccountName Is Nothing Then

                Dim AuthenticationRequest As HttpWebRequest = HttpWebRequest.Create(New Uri(AuthenticationUrl))

                With AuthenticationRequest
                    .Headers.Add("X-Auth-Key", Me.ApiKey)
                    .Headers.Add("X-Auth-User", Me.AccountName)
                    .Method = "GET"
                End With

                Dim AuthenticationResponse As HttpWebResponse = AuthenticationRequest.GetResponse

                Me.XAuthToken = AuthenticationResponse.Headers("X-Auth-Token")
                Me.pManagementUrl = AuthenticationResponse.Headers("X-CDN-Management-Url")
                Me.pStorageUrl = AuthenticationResponse.Headers("X-Storage-Url").Replace(".dfw1.", ".<location>.")
                Me.XAuthTokenReceivedOn = Now

                ' Notice a little string replacement above, as the Storage Url will actually vary
                ' depending where the container is.

            End If

        End Sub

        Public Function GetAccountMetadata(ByVal ServerLocation As ServerLocation) As AccountMetadata

            ' Make sure we've got a valid token. Reauthenticate if need be.
            If Me.XAuthTokenIsExpired Then Me.Authenticate()

            Try

            Catch ex As WebException

                Dim Response As HttpWebResponse = ex.Response

                If Response.StatusCode = HttpStatusCode.Unauthorized Then



                End If

            End Try

            ' A request to get account metadata.
            Dim MetadataRequest As HttpWebRequest

            ' Change the target URL based on what server farm we're counting on.
            If ServerLocation = CloudFiles.ServerLocation.Chicago Then
                MetadataRequest = HttpWebRequest.Create(Me.ChicagoStorageUrl)
            Else
                MetadataRequest = HttpWebRequest.Create(Me.DallasStorageUrl)
            End If

            With MetadataRequest
                .Headers.Add("X-Auth-Token", Me.XAuthToken)
                .Method = "HEAD"
            End With

            Dim MetadataResponse As HttpWebResponse = MetadataRequest.GetResponse

            Return New AccountMetadata(MetadataResponse)

        End Function

        Public Function GetContainerList(ByVal ServerLocation As ServerLocation) As ContainerList

            ' Make sure we've got a valid token. Reauthenticate if need be.
            If Me.XAuthTokenIsExpired Then Me.Authenticate()

            ' A request to get a list of containers.
            Dim ContainerListRequest As HttpWebRequest

            ' Change the target URL based on what server farm we're counting on.
            If ServerLocation = CloudFiles.ServerLocation.Chicago Then
                ContainerListRequest = HttpWebRequest.Create(Me.ChicagoStorageUrl)
            Else
                ContainerListRequest = HttpWebRequest.Create(Me.DallasStorageUrl)
            End If

            With ContainerListRequest
                .Headers.Add("X-Auth-Token", Me.XAuthToken)
                .Method = "GET"
            End With

            Dim ContainerListResponse As HttpWebResponse = ContainerListRequest.GetResponse

            Return New ContainerList(ContainerListResponse)

        End Function

        Public Function ListObjectsInContainer(ByVal ContainerName As String, ByVal ServerLocation As ServerLocation) As FileList

            ' Make sure we've got a valid token. Reauthenticate if need be.
            If Me.XAuthTokenIsExpired Then Me.Authenticate()

            ' A request to get a list of files in a container.
            Dim FileListRequest As HttpWebRequest

            ' Change the target URL based on what server farm we're counting on.
            If ServerLocation = CloudFiles.ServerLocation.Chicago Then
                FileListRequest = HttpWebRequest.Create(Me.ChicagoStorageUrl & "/" & ContainerName)
            Else
                FileListRequest = HttpWebRequest.Create(Me.DallasStorageUrl & "/" & ContainerName)
            End If

            With FileListRequest
                .Headers.Add("X-Auth-Token", Me.XAuthToken)
                .Method = "GET"
            End With

            Dim FileListResponse As HttpWebResponse = FileListRequest.GetResponse

            Return New FileList(FileListResponse)

        End Function

        Public Function CreateContainer(ByVal NewContainerName As String, ByVal ServerLocation As ServerLocation) As CreateContainerResult

            ' Make sure we've got a valid token. Reauthenticate if need be.
            If Me.XAuthTokenIsExpired Then Me.Authenticate()

            ' A request to create the container.
            Dim CreateContainerRequest As HttpWebRequest

            ' Change the target URL based on what server farm we're counting on.
            If ServerLocation = CloudFiles.ServerLocation.Chicago Then
                CreateContainerRequest = HttpWebRequest.Create(Me.ChicagoStorageUrl & "/" & NewContainerName)
            Else
                CreateContainerRequest = HttpWebRequest.Create(Me.DallasStorageUrl & "/" & NewContainerName)
            End If

            With CreateContainerRequest
                .Headers.Add("X-Auth-Token", Me.XAuthToken)
                .Method = "PUT"
            End With

            Dim CreateContainerResponse As HttpWebResponse = CreateContainerRequest.GetResponse

            If CreateContainerResponse.StatusCode = HttpStatusCode.Created Then
                Return CreateContainerResult.ContainerCreatedSuccessfully
            ElseIf CreateContainerResponse.StatusCode = HttpStatusCode.Accepted Then
                Return CreateContainerResult.ContainerAlreadyExists
            Else
                Return CreateContainerResult.UnknownResultProbableFailure
            End If

        End Function

        Public Function CdnEnableContainer(ByVal ContainerName As String, ByVal ServerLocation As ServerLocation, Optional ByVal TimeToLive As Long = 259200) As CdnEnableContainerResult

            ' Make sure we've got a valid token. Reauthenticate if need be.
            If Me.XAuthTokenIsExpired Then Me.Authenticate()

            ' A request to CDN-enable the container.
            Dim CdnEnableRequest As HttpWebRequest

            ' Change the target URL based on what server farm we're counting on.
            If ServerLocation = CloudFiles.ServerLocation.Chicago Then
                CdnEnableRequest = HttpWebRequest.Create(Me.ChicagoManagementUrl & "/" & ContainerName)
            Else
                CdnEnableRequest = HttpWebRequest.Create(Me.DallasManagementUrl & "/" & ContainerName)
            End If

            With CdnEnableRequest
                .Method = "PUT"
                .Headers.Add("X-Auth-Token", Me.XAuthToken)
                .Headers.Add("X-CDN-Enabled", "True")
                .Headers.Add("X-TTL", CStr(TimeToLive))
            End With

            Dim CdnEnableContainerResponse As HttpWebResponse = CdnEnableRequest.GetResponse

            If CdnEnableContainerResponse.StatusCode = HttpStatusCode.Created Then
                Return CdnEnableContainerResult.CdnEnabledContainerSuccessfully
            ElseIf CdnEnableContainerResponse.StatusCode = HttpStatusCode.Accepted Then
                Return CdnEnableContainerResult.ContainerAlreadyCdnEnabled
            Else
                Return CdnEnableContainerResult.UnknownResultProbableFailure
            End If

        End Function

        Public Function CdnDisableContainer(ByVal ContainerName As String, ByVal ServerLocation As ServerLocation, Optional ByVal TimeToLive As Long = 259200) As CdnEnableContainerResult

            ' Make sure we've got a valid token. Reauthenticate if need be.
            If Me.XAuthTokenIsExpired Then Me.Authenticate()

            ' A request to CDN-disable the container.
            Dim CdnEnableRequest As HttpWebRequest

            ' Change the target URL based on what server farm we're counting on.
            If ServerLocation = CloudFiles.ServerLocation.Chicago Then
                CdnEnableRequest = HttpWebRequest.Create(Me.ChicagoManagementUrl & "/" & ContainerName)
            Else
                CdnEnableRequest = HttpWebRequest.Create(Me.DallasManagementUrl & "/" & ContainerName)
            End If

            With CdnEnableRequest
                .Method = "PUT"
                .Headers.Add("X-Auth-Token", Me.XAuthToken)
                .Headers.Add("X-CDN-Enabled", "False")
                .Headers.Add("X-TTL", CStr(TimeToLive))
            End With

            Dim CdnEnableContainerResponse As HttpWebResponse = CdnEnableRequest.GetResponse

            If CdnEnableContainerResponse.StatusCode = HttpStatusCode.Created Then
                Return CdnEnableContainerResult.CdnEnabledContainerSuccessfully
            ElseIf CdnEnableContainerResponse.StatusCode = HttpStatusCode.Accepted Then
                Return CdnEnableContainerResult.ContainerAlreadyCdnEnabled
            Else
                Return CdnEnableContainerResult.UnknownResultProbableFailure
            End If

        End Function

        Public Function DeleteContainer(ByVal ContainerToDelete As String, ByVal ServerLocation As ServerLocation) As DeleteContainerResult

            ' Make sure we've got a valid token. Reauthenticate if need be.
            If Me.XAuthTokenIsExpired Then Me.Authenticate()

            ' A request to delete the container.
            Dim DeleteContainerRequest As HttpWebRequest

            ' Change the target URL based on what server farm we're counting on.
            If ServerLocation = CloudFiles.ServerLocation.Chicago Then
                DeleteContainerRequest = HttpWebRequest.Create(Me.ChicagoStorageUrl & "/" & ContainerToDelete)
            Else
                DeleteContainerRequest = HttpWebRequest.Create(Me.DallasStorageUrl & "/" & ContainerToDelete)
            End If

            With DeleteContainerRequest
                .Headers.Add("X-Auth-Token", Me.XAuthToken)
                .Method = "DELETE"
            End With

            Dim DeleteContainerResponse As HttpWebResponse = DeleteContainerRequest.GetResponse

            If DeleteContainerResponse.StatusCode = HttpStatusCode.NoContent Then
                Return DeleteContainerResult.ContainerDeletedSuccessfully
            ElseIf DeleteContainerResponse.StatusCode = HttpStatusCode.NotFound Then
                Return DeleteContainerResult.ContainerDidNotExist
            ElseIf DeleteContainerResponse.StatusCode = HttpStatusCode.Conflict Then
                Return DeleteContainerResult.ContainerNotDeletedWasNotEmpty
            Else
                Return DeleteContainerResult.UnknownProbableFailure
            End If

        End Function

        Public Function UpdateContainerMetadata(ByVal ContainerName As String, ByVal Metadata As ContainerMetadata, ByVal ServerLocation As ServerLocation) As UpdateContainerMetadataResult

            ' Make sure we've got a valid token. Reauthenticate if need be.
            If Me.XAuthTokenIsExpired Then Me.Authenticate()

            ' A request to update the metadata.
            Dim UpdateContainerMetadataRequest As HttpWebRequest

            ' Change the target URL based on what server farm we're counting on.
            If ServerLocation = CloudFiles.ServerLocation.Chicago Then
                UpdateContainerMetadataRequest = HttpWebRequest.Create(Me.ChicagoStorageUrl & "/" & ContainerName)
            Else
                UpdateContainerMetadataRequest = HttpWebRequest.Create(Me.DallasStorageUrl & "/" & ContainerName)
            End If

            With UpdateContainerMetadataRequest
                .Headers.Add("X-Auth-Token", Me.XAuthToken)
                For Each Header As X_Container_Meta_Header In Metadata.Headers
                    .Headers.Add(Header.Name, Header.Value)
                Next
                .Method = "POST"
            End With

            Dim UpdateContainerMetadataResponse As HttpWebResponse = UpdateContainerMetadataRequest.GetResponse

            If UpdateContainerMetadataResponse.StatusCode = HttpStatusCode.NoContent Then
                Return UpdateContainerMetadataResult.ContainerMetadataUpdatedSuccessfully
            ElseIf UpdateContainerMetadataResponse.StatusCode = HttpStatusCode.NotFound Then
                Return UpdateContainerMetadataResult.ContainerNotUpdatedDoesNotExist
            Else
                Return UpdateContainerMetadataResult.UnknownProbableFailure
            End If

        End Function

        Public Function GetContainerMetadata(ByVal ContainerName As String, ByVal ServerLocation As String) As ContainerMetadata

            ' Make sure we've got a valid token. Reauthenticate if need be.
            If Me.XAuthTokenIsExpired Then Me.Authenticate()

            ' A request to read the metadata.
            Dim GetContainerMetadataRequest As HttpWebRequest

            ' Change the target URL based on what server farm we're counting on.
            If ServerLocation = CloudFiles.ServerLocation.Chicago Then
                GetContainerMetadataRequest = HttpWebRequest.Create(Me.ChicagoStorageUrl & "/" & ContainerName)
            Else
                GetContainerMetadataRequest = HttpWebRequest.Create(Me.DallasStorageUrl & "/" & ContainerName)
            End If

            With GetContainerMetadataRequest
                .Headers.Add("X-Auth-Token", Me.XAuthToken)
                .Method = "HEAD"
            End With

            Dim GetContainerMetadataResponse As HttpWebResponse = GetContainerMetadataRequest.GetResponse

            Dim MetadataResult As New ContainerMetadata

            For i As Integer = 0 To GetContainerMetadataResponse.Headers.Count - 1
                If GetContainerMetadataResponse.Headers.Keys(i).StartsWith("X-Container-Meta-") Then
                    MetadataResult.Headers.Add(New X_Container_Meta_Header(GetContainerMetadataResponse.Headers.Keys(i), GetContainerMetadataResponse.Headers.Item(i)))
                End If
            Next

            Return MetadataResult

        End Function

        Public Function GetFile(ByVal ContainerName As String, ByVal FileName As String, ByVal ServerLocation As ServerLocation) As Byte()

            ' Make sure we've got a valid token. Reauthenticate if need be.
            If Me.XAuthTokenIsExpired Then Me.Authenticate()

            ' A request to read the file.
            Dim FileRequest As HttpWebRequest

            ' Change the target URL based on what server farm we're counting on.
            If ServerLocation = CloudFiles.ServerLocation.Chicago Then
                FileRequest = HttpWebRequest.Create(Me.ChicagoStorageUrl & "/" & ContainerName & "/" & FileName)
            Else
                FileRequest = HttpWebRequest.Create(Me.DallasStorageUrl & "/" & ContainerName & "/" & FileName)
            End If

            With FileRequest
                .Headers.Add("X-Auth-Token", Me.XAuthToken)
                .Method = "GET"
            End With

            Dim FileResponse As HttpWebResponse = FileRequest.GetResponse

            If FileResponse.StatusCode = HttpStatusCode.OK Then

                Dim Reader As New IO.BinaryReader(FileResponse.GetResponseStream)
                Dim FileBytes() As Byte = Reader.ReadBytes(FileResponse.GetResponseStream.Length)
                Reader.Close()
                Reader.Dispose()

                Return FileBytes

            Else
                Return Nothing
            End If

        End Function

        Public Function GetFile(ByVal ContainerName As String, ByVal FileName As String, ByVal DestinationFolderPath As String, ByVal ServerLocation As ServerLocation, Optional ByVal OverwriteExistingFile As Boolean = False) As GetFileResponse

            ' This version of GetFile retrieves the file object and automatically writes it to the filesystem
            ' instead of just returning a byte array.

            ' Returns a boolean indicating success.

            If IO.Directory.Exists(DestinationFolderPath) Then

                ' Sanitize the DestinationFolderPath...
                If Not DestinationFolderPath.EndsWith("\") Then
                    DestinationFolderPath += "\"
                End If

                If IO.File.Exists(DestinationFolderPath & FileName) And OverwriteExistingFile = False Then

                    ' The file already exists on the local filesystem, and the user doesn't want to overwrite it.
                    ' Immediately return false.

                    Return GetFileResponse.FileNotWrittenAlreadyExistsLocally

                End If

                ' The local folder exists. That's a plus.

                ' Make sure we've got a valid token. Reauthenticate if need be.
                If Me.XAuthTokenIsExpired Then Me.Authenticate()

                ' A request to read the file.
                Dim FileRequest As HttpWebRequest

                ' Change the target URL based on what server farm we're counting on.
                If ServerLocation = CloudFiles.ServerLocation.Chicago Then
                    FileRequest = HttpWebRequest.Create(Me.ChicagoStorageUrl & "/" & ContainerName & "/" & FileName)
                Else
                    FileRequest = HttpWebRequest.Create(Me.DallasStorageUrl & "/" & ContainerName & "/" & FileName)
                End If

                With FileRequest
                    .Headers.Add("X-Auth-Token", Me.XAuthToken)
                    .Method = "GET"
                End With

                Dim FileResponse As HttpWebResponse = FileRequest.GetResponse

                If FileResponse.StatusCode = HttpStatusCode.OK Then

                    ' Looks like we got a file. Write it to the filesystem.

                    Try

                        Dim Reader As New IO.BinaryReader(FileResponse.GetResponseStream)
                        Dim FileBytes() As Byte = Reader.ReadBytes(FileResponse.GetResponseStream.Length)
                        Reader.Close()
                        Reader.Dispose()

                        Dim Writer As New IO.FileStream(DestinationFolderPath & FileName, IO.FileMode.CreateNew)
                        Writer.Write(FileBytes, 0, FileBytes.Length)
                        Writer.Close()
                        Writer.Dispose()

                        Return GetFileResponse.FileSuccessfullyWritten

                    Catch ex As Exception

                        ' We couldn't write the file for some reason.
                        Return GetFileResponse.FileNotWrittenPermissionsOrIoException

                    End Try

                Else

                    ' A file wasn't returned. We failed. Return false.
                    Return GetFileResponse.FileNotWrittenDoesNotExistInCloudFiles

                End If

            Else

                ' The provided folder doesn't exist on the local file system.
                ' Instant fail.

                Return GetFileResponse.FileNotWrittenInvalidDestinationFolder

            End If

        End Function

        Public Function StoreFile(ByVal ContainerName As String, ByVal FileNameAndExtension As String, ByVal FileContents As Byte(), ByVal ServerLocation As ServerLocation, Optional ByVal ExtraMetadata As ObjectMetadata = Nothing, Optional ByVal SendHash As Boolean = True) As StoreFileResult

            ' Two optional parameters are provided: ExtraMetadata and SendHash.

            ' ExtraMetadata provides a way to send extra headers. (X-Object-Meta-<whatever>)
            ' SendHash, when set to true, will calculate an MD5 Hash for the file automatically
            ' and send it to Cloud Files in the ETag header. If this checksum doesn't match one
            ' calculated by Cloud Files when it arrives, the file is rejected.
            ' Documentation for this is here: http://docs.rackspace.com/files/api/v1/cf-devguide/content/Create_Update_Object-d1e1965.html


            ' Make sure we've got a valid token. Reauthenticate if need be.
            If Me.XAuthTokenIsExpired Then Me.Authenticate()

            ' A request to store the file.
            Dim StoreFileRequest As HttpWebRequest

            ' Change the target URL based on what server farm we're counting on.
            If ServerLocation = CloudFiles.ServerLocation.Chicago Then
                StoreFileRequest = HttpWebRequest.Create(Me.ChicagoStorageUrl & "/" & ContainerName & "/" & FileNameAndExtension)
            Else
                StoreFileRequest = HttpWebRequest.Create(Me.DallasStorageUrl & "/" & ContainerName & "/" & FileNameAndExtension)
            End If

            With StoreFileRequest

                .Method = "PUT"
                .Headers("X-Auth-Token") = XAuthToken

                If SendHash Then

                    ' The user wants to send a hash to make sure the file arrives
                    ' in full without corruption.

                    .Headers.Add("ETag", Utilities.CalculateMd5Hash(FileContents))

                End If

                If Not ExtraMetadata Is Nothing Then

                    ' The user has sent some extra headers to send along.

                    For Each Header As X_Object_Meta_Header In ExtraMetadata.Headers
                        .Headers.Add(Header.Name, Header.Value)
                    Next

                End If

            End With

            Dim RequestStream As IO.Stream = StoreFileRequest.GetRequestStream
            RequestStream.Write(FileContents, 0, FileContents.Length)
            RequestStream.Close()

            Dim StoreFileResponse As HttpWebResponse = StoreFileRequest.GetResponse

            If StoreFileResponse.StatusCode = 201 Then

                ' File stored successfully.
                Return StoreFileResult.FileStoredSuccessfully

            ElseIf StoreFileResponse.StatusCode = 401 Then

                ' Authentication failed, apparently.
                Return StoreFileResult.FileNotStoredAuthenticationFailed

            ElseIf StoreFileResponse.StatusCode = 422 Then

                ' Hash didn't match.
                Return StoreFileResult.FileNotStoredChecksumDoesNotMatch

            Else

                ' Who knows...
                Return StoreFileResult.UnknownProbableFailure

            End If

        End Function

        Public Function StoreFile(ByVal ContainerName As String, ByVal LocalFileFullPath As String, ByVal ServerLocation As ServerLocation, Optional ByVal ExtraMetadata As ObjectMetadata = Nothing, Optional ByVal SendHash As Boolean = True) As StoreFileResult

            ' An easier-to-work-with version of the StoreFile function that takes a local file path
            ' and reads the file into a buffer for you.

            ' More appropriate for local programs than something running on a web server...

            If IO.File.Exists(LocalFileFullPath) Then

                ' Well, the file exists. That's a start.

                ' Make sure we've got a valid token. Reauthenticate if need be.
                If Me.XAuthTokenIsExpired Then Me.Authenticate()

                ' A request to store the file.
                Dim StoreFileRequest As HttpWebRequest

                ' Get just the name of the file, excluding the folder path...
                Dim FileName As String = LocalFileFullPath
                If FileName.Contains("\") Then FileName = FileName.Substring(FileName.LastIndexOf("\") + 1)

                ' Change the target URL based on what server farm we're counting on.
                If ServerLocation = CloudFiles.ServerLocation.Chicago Then
                    StoreFileRequest = HttpWebRequest.Create(Me.ChicagoStorageUrl & "/" & ContainerName & "/" & FileName)
                Else
                    StoreFileRequest = HttpWebRequest.Create(Me.DallasStorageUrl & "/" & ContainerName & "/" & FileName)
                End If

                Dim FileContents() As Byte = Utilities.GetBytesFromFile(LocalFileFullPath)

                With StoreFileRequest

                    .Method = "PUT"
                    .Headers("X-Auth-Token") = XAuthToken

                    If SendHash Then

                        ' The user wants to send a hash to make sure the file arrives
                        ' in full without corruption.

                        .Headers.Add("ETag", Utilities.CalculateMd5Hash(FileContents))

                    End If

                    If Not ExtraMetadata Is Nothing Then

                        ' The user has sent some extra headers to send along.

                        For Each Header As X_Object_Meta_Header In ExtraMetadata.Headers
                            .Headers.Add(Header.Name, Header.Value)
                        Next

                    End If

                End With

                Dim RequestStream As IO.Stream = StoreFileRequest.GetRequestStream
                RequestStream.Write(FileContents, 0, FileContents.Length)
                RequestStream.Close()

                Dim StoreFileResponse As HttpWebResponse = StoreFileRequest.GetResponse

                If StoreFileResponse.StatusCode = 201 Then

                    ' File stored successfully.
                    Return StoreFileResult.FileStoredSuccessfully

                ElseIf StoreFileResponse.StatusCode = 401 Then

                    ' Authentication failed, apparently.
                    Return StoreFileResult.FileNotStoredAuthenticationFailed

                ElseIf StoreFileResponse.StatusCode = 422 Then

                    ' Hash didn't match.
                    Return StoreFileResult.FileNotStoredChecksumDoesNotMatch

                Else

                    ' Who knows...
                    Return StoreFileResult.UnknownProbableFailure

                End If

            Else

                ' The local file doesn't even exist...
                Return StoreFileResult.FileNotStoredLocalFileDoesNotExist

            End If

        End Function

        Public Function CopyFile(ByVal SourceContainerName As String, ByVal SourceFileName As String, ByVal DestinationContainerName As String, ByVal DestinationFileName As String, ByVal ServerLocation As ServerLocation) As CopyFileResult

            ' Make sure we've got a valid token. Reauthenticate if need be.
            If Me.XAuthTokenIsExpired Then Me.Authenticate()

            ' A request to copy the file.
            Dim CopyFileRequest As HttpWebRequest

            ' Change the target URL based on what server farm we're counting on.
            If ServerLocation = CloudFiles.ServerLocation.Chicago Then
                CopyFileRequest = HttpWebRequest.Create(Me.ChicagoStorageUrl & "/" & SourceContainerName & "/" & SourceFileName)
            Else
                CopyFileRequest = HttpWebRequest.Create(Me.DallasStorageUrl & "/" & SourceContainerName & "/" & SourceFileName)
            End If

            With CopyFileRequest
                .Headers.Add("X-Auth-Token", Me.XAuthToken)
                .Headers.Add("Destination", "/" & DestinationContainerName & "/" & DestinationFileName)
                .Method = "COPY"
            End With

            Dim CopyFileResponse As HttpWebResponse = CopyFileRequest.GetResponse

            If CopyFileResponse.StatusCode = HttpStatusCode.Created Then
                Return CopyFileResult.FileSuccessfullyCopied
            Else
                Return CopyFileResult.FileCopyFailed
            End If

        End Function

        Public Function DeleteFile(ByVal ContainerName As String, ByVal FileName As String, ByVal ServerLocation As ServerLocation) As DeleteFileResult

            ' Make sure we've got a valid token. Reauthenticate if need be.
            If Me.XAuthTokenIsExpired Then Me.Authenticate()

            ' A request to copy the file.
            Dim DeleteFileRequest As HttpWebRequest

            ' Change the target URL based on what server farm we're counting on.
            If ServerLocation = CloudFiles.ServerLocation.Chicago Then
                DeleteFileRequest = HttpWebRequest.Create(Me.ChicagoStorageUrl & "/" & ContainerName & "/" & FileName)
            Else
                DeleteFileRequest = HttpWebRequest.Create(Me.DallasStorageUrl & "/" & ContainerName & "/" & FileName)
            End If

            With DeleteFileRequest
                .Headers.Add("X-Auth-Token", Me.XAuthToken)
                .Method = "DELETE"
            End With

            Dim DeleteFileResponse As HttpWebResponse = DeleteFileRequest.GetResponse

            If DeleteFileResponse.StatusCode = 204 Then
                Return DeleteFileResult.FileSuccessfullyDeleted
            Else
                Return DeleteFileResult.FileDidNotExist
            End If

        End Function

        Public Function GetFileMetadata(ByVal ContainerName As String, ByVal FileName As String, ByVal ServerLocation As String) As ObjectMetadata

            ' Make sure we've got a valid token. Reauthenticate if need be.
            If Me.XAuthTokenIsExpired Then Me.Authenticate()

            ' A request to read the metadata.
            Dim GetFileMetadataRequest As HttpWebRequest

            ' Change the target URL based on what server farm we're counting on.
            If ServerLocation = CloudFiles.ServerLocation.Chicago Then
                GetFileMetadataRequest = HttpWebRequest.Create(Me.ChicagoStorageUrl & "/" & ContainerName & "/" & FileName)
            Else
                GetFileMetadataRequest = HttpWebRequest.Create(Me.DallasStorageUrl & "/" & ContainerName & "/" & FileName)
            End If

            With GetFileMetadataRequest
                .Headers.Add("X-Auth-Token", Me.XAuthToken)
                .Method = "HEAD"
            End With

            Dim GetFileMetadataResponse As HttpWebResponse = GetFileMetadataRequest.GetResponse

            Dim MetadataResult As New ObjectMetadata

            For i As Integer = 0 To GetFileMetadataResponse.Headers.Count - 1
                If GetFileMetadataResponse.Headers.Keys(i).StartsWith("X-Object-Meta-") Then
                    MetadataResult.Headers.Add(New X_Object_Meta_Header(GetFileMetadataResponse.Headers.Keys(i), GetFileMetadataResponse.Headers.Item(i)))
                End If
            Next

            Return MetadataResult

        End Function

        Public Function UpdateFileMetadata(ByVal ContainerName As String, ByVal Metadata As ObjectMetadata, ByVal ServerLocation As ServerLocation) As UpdateObjectMetadataResult

            ' Make sure we've got a valid token. Reauthenticate if need be.
            If Me.XAuthTokenIsExpired Then Me.Authenticate()

            ' A request to update the metadata.
            Dim UpdateFileMetadataRequest As HttpWebRequest

            ' Change the target URL based on what server farm we're counting on.
            If ServerLocation = CloudFiles.ServerLocation.Chicago Then
                UpdateFileMetadataRequest = HttpWebRequest.Create(Me.ChicagoStorageUrl & "/" & ContainerName)
            Else
                UpdateFileMetadataRequest = HttpWebRequest.Create(Me.DallasStorageUrl & "/" & ContainerName)
            End If

            With UpdateFileMetadataRequest
                .Headers.Add("X-Auth-Token", Me.XAuthToken)
                For Each Header As X_Object_Meta_Header In Metadata.Headers
                    .Headers.Add(Header.Name, Header.Value)
                Next
                .Method = "POST"
            End With

            Dim UpdateFileMetadataResponse As HttpWebResponse = UpdateFileMetadataRequest.GetResponse

            If UpdateFileMetadataResponse.StatusCode = HttpStatusCode.Accepted Then
                Return UpdateObjectMetadataResult.ObjectMetadataUpdatedSuccessfully
            ElseIf UpdateFileMetadataResponse.StatusCode = HttpStatusCode.NotFound Then
                Return UpdateObjectMetadataResult.ObjectNotUpdatedDoesNotExist
            Else
                Return UpdateObjectMetadataResult.UnknownProbableFailure
            End If

        End Function

#End Region

    End Class

End Namespace




