
Namespace Behaviour

	''' <summary>
	''' Interface definition for behaviour tree nodes, that have one or more child nodes.
	''' </summary>
	''' <typeparam name="TBag">A <see cref="StateManager(Of TValue, TResult)" /> implementation</typeparam>
	''' <typeparam name="TValue">A <see cref="TypedValue" /> implementation</typeparam>
	''' <typeparam name="TResult">The result of this node's run</typeparam>
	Public Interface IParentBehaviourTreeNode(Of TBag As StateManager(Of TValue, TResult), TValue As {TypedValue, New}, TResult As {IBehaviourResult, New})
		Inherits IBehaviourTreeNode(Of TBag, TValue, TResult)

		''' <summary>
		''' Returns an immutable collection of this node's child nodes.
		''' </summary>
		''' <returns>A <see cref="IReadOnlyCollection(Of IBehaviourTreeNode)"/></returns>
		ReadOnly Property Children As BehaviourTreeNodeCollection(Of TBag, TValue, TResult)

		''' <summary>
		''' Adds a child <see cref="IBehaviourTreeNode"/> to this node.
		''' </summary>
		''' <param name="node">The <see cref="IBehaviourTreeNode"/> implementation to add</param>
		Sub AddChild(node As IBehaviourTreeNode(Of TBag, TValue, TResult))

	End Interface

End Namespace