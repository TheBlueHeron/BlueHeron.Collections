Imports System.Collections.Concurrent

''' <summary>
''' Writes the contents of the input buffer to the debug window.
''' </summary>
''' <typeparam name="TInput">Type of the elements in the input buffer</typeparam>
''' <remarks>Elements in the input buffer are written away simply by calling their .ToString() methods.</remarks>
Public Class DebugOutputPipe(Of TInput)
	Inherits PipeBase(Of TInput, String)

#Region " Objects and variables "

	Private m_Inputs As BlockingCollection(Of TInput)

#End Region

#Region " Public methods and functions "

	''' <summary>
	''' Cancels processing.
	''' </summary>
	Public Overrides Sub Cancel()

		m_Inputs.CompleteAdding()
		m_IsCompleted = True

	End Sub

	''' <summary>
	''' Connects this <see cref="IPipe">pipe</see> to the appropriate input buffer.
	''' </summary>
	''' <param name="pipeLine">The <see cref="Pipeline">Pipeline</see> in which this <see cref="IPipe">pipe</see> resides</param>
	Public Overrides Sub Connect(pipeLine As Pipeline)

		m_Inputs = GetInput(Of TInput)(pipeLine)
		' no need to connect to an output buffer

	End Sub

	''' <summary>
	''' Starts processing of elements in the input buffer.
	''' </summary>
	Public Overrides Sub Process()

		For Each result As TInput In m_Inputs.GetConsumingEnumerable
			Debug.WriteLine(result.ToString)
		Next

	End Sub

#End Region

End Class