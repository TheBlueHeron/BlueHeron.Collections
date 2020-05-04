
Namespace Behaviour

	''' <summary>
	''' Interface for objects that have a weight.
	''' </summary>
	Public Interface IWeighted

		''' <summary>
		''' The weight of the node.
		''' </summary>
		Property Weight As Integer

	End Interface

	''' <summary>
	''' Interface for <see cref="IBehaviourTreeNode"/> objects that have a weight.
	''' </summary>
	Public Interface IWeightedNode(Of TBag As StateManager(Of TValue, TResult), TValue As {TypedValue, New}, TResult As {IBehaviourResult, New})
		Inherits IBehaviourTreeNode(Of TBag, TValue, TResult), IWeighted

	End Interface

	''' <summary>
	''' Interface for <see cref="IParentBehaviourTreeNode"/> objects that have a weight.
	''' </summary>
	Public Interface IWeightedParentNode(Of TBag As StateManager(Of TValue, TResult), TValue As {TypedValue, New}, TResult As {IBehaviourResult, New})
		Inherits IParentBehaviourTreeNode(Of TBag, TValue, TResult), IWeighted

	End Interface

End Namespace