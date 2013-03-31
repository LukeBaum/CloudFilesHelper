CloudFilesHelper "1.0"

ABOUT

Rackspace makes some awesome open products, but lately they seem to neglect two things in the grand scheme of things:
one of their own products, Cloud Files, and .NET developers. In a way, I suppose it makes sense to somewhat ignore
a proprietary development platform and focus on open ones. After all, Rackspace's stuff is built on OpenStack. But
even Cloud Files feels like a secondary citizen among its other products sometimes, with functionality missing from
some of the official SDKs and worse documentation here and there.
CloudFilesHelper is designed to help bridge the gap for us lowly .NET developers until OpenStack.NET supports Cloud
Files functionality.

SOME NOTES

- I created this project for my own use initially and decided to release it on github because there is no good SDK
  for .NET development for Cloud Files. That being said, this code probably isn't perfect. Feel free to improve it
  where possible. I have tested it in my use but other than that, it hasn't seen any use.
- The source code is in VB.NET. Yes, Visual Basic developers still exist. You can easily port the code to C# if you
  so desire and the binary will obviously work with any .NET project if added as a reference.
- If you have questions not answered here, email me. This isn't my day job though unfortunately.
- The Cloud Files API refers to Objects; CloudFilesHelper's functions may refer to Files instead. They are the same
  thing for code purposes.

LIBRARY COMPONENTS

CloudFilesHelper consists of 1 main class (CloudFilesHelper, go figure), 7 supporting classes, a Utility module, and
9 enumerations. All of the work is done with the CloudFilesHelper classes. When you use CloudFilesHelper to perform
an operation in Cloud Files, it will return one of the support objects or enumerations. Objects are returned when
you are expecting information; enumeration members are returned on operations where you really just need to know if
it worked or not.

EXAMPLES

Let's suppose we want to create a Container in our Cloud Files account and then upload a file to it. Here is how we
would do it after adding the Reference to our project:

' For sanity, put this statement at the top of the code file:

Imports Helpers.CloudFiles

' Let's skip ahead and pretend we're in a subroutine or method.

' We need an instance of a CloudFilesHelper object to do the lifting. We pass to it our Cloud Files API Key and our Cloud Files Account Name.

Dim Helper As New CloudFilesHelper("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", "accountname")

' The constructor gets us an X-Auth-Token to use and will keep using that token. If it detects that it is expired, it will get a new token in any call automatically; we do not need to manually authenticate again.

' Currently in the US, Cloud Files lets us put containers in a datacenter in either Dallas (by default) or Chicago. Let's create a container named "Files" at the Chicago datacenter.

Dim ContainerResult As CreateContainerResult = Helper.CreateContainer("Files", ServerLocation.Chicago)

' If ContainerResult = CreateContainerResult.ContainerCreatedSuccessfully then we know the Container was made. Since this is an example, who needs error checking? :P

' Let's upload a file.

Dim FileResult As StoreFileResult = Helper.StoreFile("Files", "C:\Users\Luke\Pictures\lol.gif", ServerLocation.Chicago)

' Oh what the heck. Let's take a look at how we could see if the file actually uploaded.

Select Case FileResult
	Case Is = StoreFileResult.FileStoredSuccessfully
		MessageBox.Show("File stored successfully.")
    Case Else
        MessageBox.Show("The file didn't get stored.")
End Select

' It is important to note that sometimes, an exception can be thrown. For example, if an Internet connection is
' interrupted and you can't reach Cloud Files, just about any call will throw an exception. So make sure to
' use error trapping. I considered putting my own exception handling in but decided against it.
