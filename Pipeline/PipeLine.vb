Imports System.Collections.Concurrent
Imports System.Threading

''' <summary>
''' Object that concurrently runs code within a series of <see cref="IPipe">Pipe</see> modules, that are chained together by a series of input buffers and output buffers.
''' Each <see cref="IPipe">Pipe</see> gets its input from an input buffer that holds a collection of strongly-typed objects on which to perform code, and pushes the output of that code to an output buffer.
''' This output buffer serves as the input buffer for the next <see cref="IPipe">pipe</see> in the pipeline. Note that multiple <see cref="IPipe">pipes</see> may read from the same input buffer and multiple <see cref="IPipe">pipes</see> may write to the same output buffer.
''' The pipeline runs most efficiently when for each item in each 'stage' in the pipeline it takes about the same time to complete.
''' </summary>
''' <remarks>
''' <seealso>MSDN Pipelines: https://msdn.microsoft.com/en-us/library/ff963548.aspx </seealso>
''' <seealso>Concurrency SDK: https://msdn.microsoft.com/en-us/library/Hh543789(v=vs.120).aspx </seealso>
''' </remarks>
Public Class PipeLine
	Implements IDisposable

#Region " Objects and variables "

	Protected m_Buffers As Dictionary(Of String, Object)
	Protected m_CancelToken As CancellationToken
	Private m_Disposed As Boolean
	Protected m_Pipes As List(Of IPipe)
	Protected m_TaskFactory As TaskFactory

#End Region

#Region " Properties "

	''' <summary>
	''' Returns the collection of named buffers that were registered in this <see cref="PipeLine">pipeline</see>.
	''' </summary>
	''' <returns>IDictionary(Of String, Object)</returns>
	Public ReadOnly Property Buffers As IDictionary(Of String, Object)
		Get
			Return m_Buffers
		End Get
	End Property

	''' <summary>
	''' Returns the collection of <see cref="IPipe">pipes</see> that were added to this <see cref="PipeLine">pipeline</see>.
	''' </summary>
	''' <returns><see cref="IEnumerable(Of IPipe)" /></returns>
	Public ReadOnly Property Pipes As IEnumerable(Of IPipe)
		Get
			Return m_Pipes
		End Get
	End Property

#End Region

#Region " Public methods and functions "

	''' <summary>
	''' Adds a named buffer for elements of type T to the pipeline.
	''' </summary>
	''' <typeparam name="T">The type of the elements that will be stored in the buffer</typeparam>
	''' <param name="name">The name by which to identify the buffer</param>
	''' <param name="boundedCapacity">The bounded capacity of the buffer</param>
	Public Overridable Sub AddBuffer(Of T)(name As String, boundedCapacity As Integer)

		If m_Buffers.ContainsKey(name) Then
			Throw New ArgumentException(String.Format("A buffer with name '{0}' already exists in the pipeline", name))
		Else
			Dim coll As New BlockingCollection(Of T)(boundedCapacity)

			m_Buffers.Add(name, coll)
		End If

	End Sub

	''' <summary>
	''' Adds the given <see cref="IPipe">pipe</see> to the pipeline.
	''' The order in which <see cref="IPipe">pipes</see> are added is not important. They will be chained by corresponding input- and outputbuffers.
	''' </summary>
	''' <param name="pipe">The <see cref="IPipe">pipe</see> module to be added</param>
	Public Overridable Sub AddPipe(pipe As IPipe)

		m_Pipes.Add(pipe)

	End Sub

	''' <summary>
	''' Cancels further processing of input elements in the <see cref="PipeLine">pipeline</see>.
	''' </summary>
	''' <remarks></remarks>
	Public Overridable Sub Cancel()

		If m_Pipes.Count > 0 Then
			m_Pipes(0).Cancel()
		End If

	End Sub

	''' <summary>
	''' Connects all <see cref="IPipe">pipes</see> in the <see cref="PipeLine">pipeline</see> to their input- and outputbuffers.
	''' Then <see cref="System.Threading.Tasks.Task">tasks</see> are created for each <see cref="IPipe">pipe's</see> <see cref="IPipe.Process">Process</see> method, which are called simultaneously.
	''' </summary>
	Public Sub Run()

		If m_Pipes.Count > 0 Then
			Dim tasks As New List(Of Task)

			For Each pipe As IPipe In m_Pipes
				pipe.Connect(Me)
				tasks.Add(m_TaskFactory.StartNew(AddressOf pipe.Process, m_CancelToken))
			Next
			Task.WaitAll(tasks.ToArray)
		End If

	End Sub

#End Region

#Region " Private methods and functions "

	''' <summary>
	''' Callback function for cancellation events within the pipeline.
	''' </summary>
	Protected Overridable Sub OnCancelled()

	End Sub

#End Region

#Region " Construction and destruction "

	''' <summary>
	''' Creates a new Pipeline object.
	''' </summary>
	Public Sub New()

		m_Buffers = New Dictionary(Of String, Object)
		m_CancelToken = New CancellationToken(False)
		m_CancelToken.Register(AddressOf OnCancelled)
		m_Pipes = New List(Of IPipe)
		m_TaskFactory = New TaskFactory(m_CancelToken, TaskCreationOptions.LongRunning, TaskContinuationOptions.None, TaskScheduler.Default)

	End Sub

	''' <summary>
	''' Cleans up resources.
	''' </summary>
	Protected Sub Dispose(disposing As Boolean)

		If Not m_Disposed Then
			If disposing Then
				' TODO: dispose managed state (managed objects).
			End If
			m_Buffers = Nothing
			m_Pipes = Nothing
		End If
		m_Disposed = True

	End Sub

#End Region

#Region " IDisposable Support "

	''' <summary>
	''' Cleans up resources.
	''' </summary>
	Public Sub Dispose() Implements IDisposable.Dispose

		Dispose(True)
		GC.SuppressFinalize(Me)

	End Sub

#End Region

End Class