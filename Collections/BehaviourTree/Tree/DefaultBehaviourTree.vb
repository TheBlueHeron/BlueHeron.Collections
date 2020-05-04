
Namespace Behaviour

	Partial Public Class BehaviourTreeBuilderBase(Of TBag As StateManager(Of TValue, TResult), TValue As {TypedValue, New}, TResult As {IBehaviourResult, New})

		''' <summary>
		''' Default implementation of a <see cref="IBehaviourTree(Of TBag, TValue, TResult)" />.
		''' </summary>
		<DebuggerDisplay("{Name}")>
		Friend Class DefaultBehaviourTree
			Inherits BehaviourTreeBase(Of TBag, TValue, TResult)

#Region " Construction "

			''' <summary>
			''' Needed for Json deserialization.
			''' </summary>
			Public Sub New()
				MyBase.New(String.Empty)
			End Sub

			''' <summary>
			''' Creates a new root node.
			''' </summary>
			''' <param name="actionName">The name of this node</param>
			<DebuggerStepThrough()>
			Public Sub New(actionName As String)

				MyBase.New(actionName)

			End Sub

#End Region

		End Class

	End Class

End Namespace