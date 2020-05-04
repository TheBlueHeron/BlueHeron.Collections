
Namespace Behaviour

	''' <summary>
	''' A default behaviourTree builder, using a <see cref="DefaultStateManager" /> with <see cref="TypedValue"/> properties, and returning <see cref="BehaviourTreeStatus"/> results.
	''' </summary>
	Public Class BehaviourTreeBuilder
		Inherits BehaviourTreeBuilderBase(Of DefaultStateManager, TypedValue, BehaviourTreeResult)

#Region " Construction "

		''' <summary>
		''' Creates a new <see cref="BehaviourTreeBuilder" />, using a <see cref="DefaultStateManager" /> with <see cref="TypedValue"/> properties and <see cref="BehaviourTreeResult"/> results.
		''' </summary>
		<DebuggerStepThrough()>
		Public Sub New()

			MyBase.New

		End Sub

		''' <summary>
		''' Creates a new <see cref="BehaviourTreeBuilder"/>, using a <see cref="DefaultStateManager" /> with <see cref="TypedValue"/> properties, starting from the given <see cref="IParentBehaviourTreeNode"/>.
		''' </summary>
		''' <param name="parent">The parent <see cref="IParentBehaviourTreeNode"/> implementation</param>
		<DebuggerStepThrough()>
		Public Sub New(parent As IParentBehaviourTreeNode(Of DefaultStateManager, TypedValue, BehaviourTreeResult))

			MyBase.New(parent)

		End Sub

#End Region

	End Class

End Namespace