Namespace CloudFiles

    Public Enum ServerLocation

        ' Interesting note. When you authenticate, you get a link to storage and management
        ' servers. However, even the container you want to work on is in Chicago, it assumes
        ' a Dallas link. Use this enum to specify where you want to work where required.

        Dallas = 0
        Chicago = 1

    End Enum

End Namespace