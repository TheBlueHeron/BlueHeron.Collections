
Namespace Behaviour

	''' <summary>
	''' Interface definition for a behaviour tree.
	''' </summary>
	''' <typeparam name="TBag">A <see cref="StateManager(Of TValue, TResult)" /> implementation</typeparam>
	''' <typeparam name="TValue">A <see cref="TypedValue" /> implementation</typeparam>
	Public Interface IBehaviourTree(Of TBag As StateManager(Of TValue, TResult), TValue As {TypedValue, New}, TResult As {IBehaviourResult, New})
		Inherits IParentBehaviourTreeNode(Of TBag, TValue, TResult)

		''' <summary>
		''' Returns the first <see cref="IBehaviourTreeNode(Of TBag, TValue, TResult)" /> with the given name.
		''' </summary>
		''' <param name="nodeName">Name of the <see cref="IBehaviourTreeNode(Of TBag, TValue, TResult)" /></param>
		''' <returns>The matching <see cref="IBehaviourTreeNode(Of TBag, TValue, TResult)" /> if it exists, else Null/Nothing</returns>
		Function Find(nodeName As String) As IBehaviourTreeNode(Of TBag, TValue, TResult)

	End Interface

End Namespace