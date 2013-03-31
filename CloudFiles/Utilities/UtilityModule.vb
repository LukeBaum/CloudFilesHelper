Namespace Utilities

    Module UtilityModule

        Public Function CalculateMd5Hash(ByRef FileBytes() As Byte) As String

            ' Calculates an MD5 hash on a file.

            Dim Hasher As New System.Security.Cryptography.MD5CryptoServiceProvider
            Dim HashBytes() As Byte = Hasher.ComputeHash(FileBytes)

            Dim Builder As New Text.StringBuilder
            For Each Character As Byte In HashBytes
                Builder.Append(Character.ToString("x2").ToLower())
            Next

            Return Builder.ToString

        End Function

        Public Function GetBytesFromFile(ByVal FilePath As String) As Byte()

            ' Reads a file into memory.

            Dim FileStream As New IO.FileStream(FilePath, IO.FileMode.Open)
            Dim Reader As New IO.BinaryReader(FileStream)

            Dim FileBytes() As Byte = Reader.ReadBytes(FileStream.Length)

            Reader.Close()
            Reader.Dispose()

            FileStream.Close()
            FileStream.Dispose()

            Return FileBytes

        End Function

    End Module

End Namespace


