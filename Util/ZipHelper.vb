Imports System.IO
Imports System.IO.Compression
Imports System.Text

''' <summary>
''' Compresses and decompresses strings using GZip in System.IO.Compression.
''' </summary>
Public Class Compressor

	''' <summary>
	''' Compresses the given text.
	''' </summary>
	''' <param name="text">Input</param>
	''' <returns>Compressed output</returns>
	Public Shared Function Compress(text As String) As String

		Using memoryStream As New MemoryStream()
			Using zipStream As New GZipStream(memoryStream, CompressionLevel.Optimal)
				Using writer As New StreamWriter(zipStream)
					writer.Write(text)
				End Using
			End Using
			Return Encoding.Default.GetString(memoryStream.ToArray())
		End Using

	End Function

	''' <summary>
	''' Deompresses the given text.
	''' </summary>
	''' <param name="text">Compressed input</param>
	''' <returns>Decompressed output</returns>
	Public Shared Function Decompress(text As String) As String

		Using memoryStream As New MemoryStream(Encoding.Default.GetBytes(text))
			Using zipStream As New GZipStream(memoryStream, CompressionMode.Decompress)
				Using ms As New MemoryStream()
					zipStream.CopyTo(ms)
					Return Encoding.Default.GetString(ms.ToArray())
				End Using
			End Using
		End Using

	End Function

End Class