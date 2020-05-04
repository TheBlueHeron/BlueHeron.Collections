
Namespace Generic

	''' <summary>
	''' The possible types of BinaryHeap.
	''' </summary>
	Public Enum HeapType
		''' <summary>
		''' The highest number is the root node.
		''' </summary>
		MaxHeap
		''' <summary>
		''' The lowest number is the root node.
		''' </summary>
		MinHeap
	End Enum

	''' <summary>
	''' Enumeration of the kind of serialization to use for the object in the collection.
	''' </summary>
	Public Enum SerializationType
		Simple = 0
		Array = 1
		ICollection = 2
		Guid = 3
		Complex = 4
	End Enum

End Namespace

Namespace Behaviour

	''' <summary>
	''' The return type when invoking behaviour tree nodes.
	''' </summary>
	Public Enum BehaviourTreeStatus
		Success
		Failure
		Running
	End Enum

	''' <summary>
	''' Enumeration of possible events to be logged.
	''' </summary>
	<Flags>
	Public Enum LogEventType
		''' <summary>
		''' No logging will occur.
		''' </summary>
		None = 0
		''' <summary>
		''' Entry events will be logged.
		''' </summary>
		Start = 1
		''' <summary>
		''' StateManager property changes will be logged.
		''' </summary>
		ValueChange = 2
		''' <summary>
		''' Exit events will be logged.
		''' </summary>
		Finish = 4
		''' <summary>
		''' Errors will be logged.
		''' </summary>
		Faulted = 8
		''' <summary>
		''' All events will be logged.
		''' </summary>
		All = 15
	End Enum

End Namespace