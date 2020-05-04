Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Namespace Behaviour

	Partial Public Class BehaviourTreeBuilderBase(Of TBag As StateManager(Of TValue, TResult), TValue As {TypedValue, New}, TResult As {IBehaviourResult, New})

		''' <summary>
		''' A decorator for a leaf node that inverts the result of the <see cref="Func(Of TBag, TResult)">action</see> of its child node.
		''' </summary>
		<DebuggerDisplay("{Name}")>
		Public Class InverterNode
			Inherits ParentNodeBase(Of TBag, TValue, TResult)

#Region " Objects and variables "

			Private Const err_NoChild As String = "No child node present."
			Private Const err_OneChild As String = "A child node is already present."

			Private m_ChildNode As IBehaviourTreeNode(Of TBag, TValue, TResult)

#End Region

#Region " Properties "

			''' <summary>
			''' Returns an immutable collection of this node's child nodes.
			''' </summary>
			''' <returns>A <see cref="BehaviourTreeNodeCollection(Of TBag, TValue, TResult)"/></returns>
			<JsonIgnore()>
			Public Overrides ReadOnly Property Children As BehaviourTreeNodeCollection(Of TBag, TValue, TResult)
				Get
					Return New BehaviourTreeNodeCollection(Of TBag, TValue, TResult)(New List(Of IBehaviourTreeNode(Of TBag, TValue, TResult)) From {m_ChildNode})
				End Get
			End Property

#End Region

#Region " Public methods and functions "

			''' <summary>
			''' Adds the given <see cref="IBehaviourTreeNode"/> as child to this node.
			''' </summary>
			''' <param name="node">The <see cref="IBehaviourTreeNode"/> to add</param>
			Public Overrides Sub AddChild(node As IBehaviourTreeNode(Of TBag, TValue, TResult))

				If Not m_ChildNode Is Nothing Then
					Throw New InvalidOperationException(err_OneChild)
				End If
				m_ChildNode = node

			End Sub

			''' <summary>
			''' Creates an InverterNode from Json.
			''' </summary>
			''' <param name="json">Input Json string</param>
			''' <returns>An instance of type SequenceNode</returns>
			Public Overloads Shared Function FromJson(json As String, Optional actionSource As Object = Nothing) As InverterNode

				Return DirectCast(ParentNodeBase(Of TBag, TValue, TResult).FromJson(json, actionSource), InverterNode)

			End Function

			''' <summary>
			''' Parses values stored in the given <see cref="JObject" /> to reconstitute this BehaviourTreeNode.
			''' </summary>
			''' <param name="jObject">A <see cref="JObject" /> containing this node's properties</param>
			Public Overrides Sub ReadJson(jObject As JObject, Optional actionSource As Object = Nothing)

				ReadCommonJson(jObject, actionSource)

			End Sub

			''' <summary>
			''' Invokes the <see cref="Func(Of StateManager(Of TypedValue), TResult)">action</see> on this node's child node and invert the result.
			''' If the child node returns <see cref="BehaviourTreeStatus.Running" /> then that value is returned.
			''' </summary>
			''' <param name="vb">A <see cref="StateManager(Of TypedValue, TResult)"/> implementation</param>
			''' <param name="timeStamp"> A <see cref="Single" /> value representing passed time</param>
			''' <returns>A <see cref="BehaviourTreeStatus"/> value</returns>
			Public Overrides Function Run(vb As TBag, timeStamp As Single) As TResult

				If m_ChildNode Is Nothing Then
					Throw New InvalidOperationException(err_NoChild)
				End If

				Dim rst As TResult

				If (Level And LogEventType.Start) = LogEventType.Start Then
					vb.LogStarted(Name, rst, vb.Source, timeStamp)
				End If

				rst = m_ChildNode.Run(vb, timeStamp)

				If rst.Result = BehaviourTreeStatus.Failure Then
					rst.Result = BehaviourTreeStatus.Success
				ElseIf rst.Result = BehaviourTreeStatus.Success Then
					rst.Result = BehaviourTreeStatus.Failure
				End If
				If (Level And LogEventType.Finish) = LogEventType.Finish Then
					vb.LogFinished(Name, rst, vb.Source, timeStamp)
				End If

				Return rst

			End Function

			''' <summary>
			''' Parses values stored in this BehaviourTreeNode into a <see cref="JObject"/>.
			''' </summary>
			Public Overrides Function WriteJson() As JObject

				Return WriteCommonJson()

			End Function

#End Region

#Region " Construction "

			''' <summary>
			''' Needed for Json deserialization.
			''' </summary>
			Public Sub New()
			End Sub

			''' <summary>
			''' Creates a new inverter node.
			''' </summary>
			''' <param name="actionName">The name of this node</param>
			<DebuggerStepThrough()>
			Public Sub New(actionName As String)

				m_Name = actionName

			End Sub

#End Region

		End Class

		''' <summary>
		''' A decorator for a leaf node that inverts the result of the <see cref="Func(Of TBag, TResult)">action</see> of its child node. Extended with a weight property.
		''' </summary>
		<DebuggerDisplay("{Name}")>
		Public Class WeightedInverterNode
			Inherits InverterNode
			Implements IWeightedParentNode(Of TBag, TValue, TResult)

#Region " Properties "

			''' <summary>
			''' Weight value (default: 1).
			''' </summary>
			<JsonProperty(PropertyName:="w")>
			Public Property Weight As Integer = 1 Implements IWeightedParentNode(Of TBag, TValue, TResult).Weight

#End Region

#Region " Public methods and functions "

			''' <summary>
			''' Parses values stored in the given <see cref="JObject" /> to reconstitute this BehaviourTreeNode.
			''' </summary>
			''' <param name="jObject">A <see cref="JObject" /> containing this node's properties</param>
			''' <param name="actionSource">Instance of the object, that holds the functions to call</param>
			Public Overrides Sub ReadJson(jObject As JObject, Optional actionSource As Object = Nothing)

				MyBase.ReadJson(jObject, actionSource)
				Weight = jObject(_O)(_W).Value(Of Integer)

			End Sub

#End Region

#Region " Construction "

			''' <summary>
			''' Needed by Json serializer.
			''' </summary>
			Public Sub New()
				MyBase.New
			End Sub

			''' <summary>
			''' Creates a new inverter node.
			''' </summary>
			''' <param name="actionName">The name of this node</param>
			''' <param name="weight">The relative weight of the node (min: 0)</param>
			<DebuggerStepThrough()>
			Public Sub New(actionName As String, weight As Integer)

				MyBase.New(actionName)
				Me.Weight = Math.Max(0, weight)

			End Sub

#End Region

		End Class

	End Class

End Namespace