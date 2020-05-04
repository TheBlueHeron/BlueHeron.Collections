
Namespace Behaviour

	''' <summary>
	''' Interface definition for <see cref="IBehaviourTreeNode(Of TBag, TValue, TResult)" /> results.
	''' </summary>
	Public Interface IBehaviourResult

		''' <summary>
		''' The result, represented as <see cref="BehaviourTreeStatus" /> value.
		''' </summary>
		Property Result As BehaviourTreeStatus

		''' <summary>
		''' Sets the properties of this object from string.
		''' </summary>
		''' <param name="input">String representation of this object</param>
		Sub FromString(input As String)

		''' <summary>
		''' Returns a string representation of this object.
		''' </summary>
		Function ToString() As String

	End Interface

End Namespace