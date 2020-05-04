Imports System.Collections.Concurrent

''' <summary>
''' Definition of a pipe module within a pipeline.
''' Pipes are chained together through corresponding input and output buffers by name (and corresponding type).
''' </summary>
Public Interface IPipe

	''' <summary>
	''' Name of the input buffer.
	''' </summary>
	''' <value>String</value>
	''' <returns>String</returns>
	Property InputBufferName As String

	''' <summary>
	''' Determines whether this <see cref="IPipe">pipe</see> has completed processing its input.
	''' </summary>
	''' <returns>Boolean</returns>
	ReadOnly Property IsCompleted As Boolean

	''' <summary>
	''' Name of the input buffer.
	''' </summary>
	''' <value>String</value>
	''' <returns>String</returns>
	Property OutputBufferName As String

	''' <summary>
	''' Cancels processing.
	''' </summary>
	Sub Cancel()

	''' <summary>
	''' Connects this <see cref="IPipe">pipe</see> to the given <see cref="PipeLine">pipeline</see>.
	''' </summary>
	''' <param name="pipeLine">The <see cref="PipeLine">pipeline</see> to connect to</param>
	Sub Connect(pipeLine As Pipeline)

	''' <summary>
	''' Starts processing.
	''' </summary>
	Sub Process()

End Interface