Imports System.Collections.Concurrent

''' <summary>
''' Abstract class from which all <see cref="IPipe">pipes</see> in a <see cref="Pipeline">Pipeline</see> must inherit.
''' </summary>
Public MustInherit Class PipeBase(Of TInput, TOutput)
	Implements IPipe

#Region " Objects and variables "

	Protected m_IsCompleted As Boolean

#End Region

#Region " Properties "

	''' <summary>
	''' The name of the input buffer.
	''' </summary>
	''' <value>String</value>
	''' <returns>String</returns>
	Public Property InputBufferName As String Implements IPipe.InputBufferName

	''' <summary>
	''' Determines the type of the elements in the input buffer.
	''' </summary>
	''' <returns><see cref="Type" /></returns>
	ReadOnly Property InputType As Type
		Get
			Return GetType(TInput)
		End Get
	End Property

	''' <summary>
	''' Determines whethe this <see cref="IPipe">pipe</see> has completed processing.
	''' </summary>
	''' <returns>Boolean</returns>
	Public ReadOnly Property IsCompleted As Boolean Implements IPipe.IsCompleted
		Get
			Return m_IsCompleted
		End Get
	End Property

	''' <summary>
	''' The name of the output buffer.
	''' </summary>
	''' <value>String</value>
	''' <returns>String</returns>
	Public Property OutputBufferName As String Implements IPipe.OutputBufferName

	''' <summary>
	''' Determines the type of the elements in the output buffer.
	''' </summary>
	''' <returns><see cref="Type" /></returns>
	ReadOnly Property OutputType As Type
		Get
			Return GetType(TOutput)
		End Get
	End Property

#End Region

#Region " Public methods and functions "

	''' <summary>
	''' Cancels further processing of the input buffer.
	''' </summary>
	Public MustOverride Sub Cancel() Implements IPipe.Cancel

	''' <summary>
	''' Connects this <see cref="IPipe">pipe</see> to the given <see cref="PipeLine">pipeline</see>.
	''' </summary>
	''' <param name="pipeLine">The <see cref="PipeLine">pipeline</see> to connect to</param>
	Public MustOverride Sub Connect(pipeLine As Pipeline) Implements IPipe.Connect

	''' <summary>
	''' Starts processing of the input buffer, pushing any output into the output buffer.
	''' </summary>
	Public MustOverride Sub Process() Implements IPipe.Process

#End Region

#Region " Private methods and functions "

	''' <summary>
	''' Returns the buffer with the given name, as it was registered in the given <see cref="PipeLine">pipeline</see>.
	''' </summary>
	''' <typeparam name="T">The type of the elements in the buffer</typeparam>
	''' <param name="pipeLine">The <see cref="PipeLine">pipeline</see> to retrieve the buffer from</param>
	''' <param name="bufferName">The name of the buffer, as it was registered in the given <see cref="PipeLine">pipeline</see></param>
	''' <returns>BlockingCollection(Of T)</returns>
	Private Function GetBuffer(Of T)(pipeLine As Pipeline, bufferName As String) As BlockingCollection(Of T)

		If pipeLine.Buffers.ContainsKey(bufferName) Then
			Dim buffer As Object = pipeLine.Buffers(bufferName)

			If TypeOf (buffer) Is BlockingCollection(Of T) Then
				Return DirectCast(buffer, BlockingCollection(Of T))
			Else
				Throw New ArgumentException(String.Format("Buffer '{0}' is of a different type than expected", bufferName))
			End If
		Else
			Throw New ArgumentException(String.Format("Buffer '{0}' is not present in the given pipeline", bufferName))
		End If

	End Function

	''' <summary>
	''' Returns the input buffer of this <see cref="IPipe">pipe</see>.
	''' </summary>
	''' <typeparam name="T">The type of the elements in the buffer</typeparam>
	''' <param name="pipeLine">The <see cref="PipeLine">pipeline</see> to retrieve the buffer from</param>
	''' <returns>BlockingCollection(Of T)</returns>
	Protected Function GetInput(Of T)(pipeLine As Pipeline) As BlockingCollection(Of T)

		Return GetBuffer(Of T)(pipeLine, InputBufferName)

	End Function

	''' <summary>
	''' Returns the output buffer of this <see cref="IPipe">pipe</see>.
	''' </summary>
	''' <typeparam name="T">The type of the elements in the buffer</typeparam>
	''' <param name="pipeLine">The <see cref="PipeLine">pipeline</see> to retrieve the buffer from</param>
	''' <returns>BlockingCollection(Of T)</returns>
	Protected Function GetOutput(Of T)(pipeLine As Pipeline) As BlockingCollection(Of T)

		Return GetBuffer(Of T)(pipeLine, OutputBufferName)

	End Function

#End Region

End Class