Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Namespace Behaviour

	Partial Public Class BehaviourTreeBuilderBase(Of TBag As StateManager(Of TValue, TResult), TValue As {TypedValue, New}, TResult As {IBehaviourResult, New})

		''' <summary>
		''' Selects the first child node that succeeds, i.e tries successive nodes until it finds one that doesn't fail.
		''' </summary>
		<DebuggerDisplay("{Name}")>
		Private Class SwitchNode
			Inherits ParentNodeBase(Of TBag, TValue, TResult)

#Region " Objects and variables "

			Private m_Action As Func(Of TBag, String, Single, Boolean, TResult)
			Protected m_ChildNodes As List(Of IBehaviourTreeNode(Of TBag, TValue, TResult))

#End Region

#Region " Properties "

			''' <summary>
			''' Returns an immutable collection of this node's child nodes.
			''' </summary>
			''' <returns>A <see cref="BehaviourTreeNodeCollection(Of TBag, TValue, TResult)"/></returns>
			<JsonIgnore()>
			Public Overrides ReadOnly Property Children As BehaviourTreeNodeCollection(Of TBag, TValue, TResult)
				Get
					Return New BehaviourTreeNodeCollection(Of TBag, TValue, TResult)(m_ChildNodes)
				End Get
			End Property

#End Region

#Region " Public methods and functions "

			''' <summary>
			''' Add the given <see cref="IBehaviourTreeNode"/> as child to this node.
			''' </summary>
			''' <param name="node">The <see cref="IBehaviourTreeNode"/> to add</param>
			Public Overrides Sub AddChild(node As IBehaviourTreeNode(Of TBag, TValue, TResult))

				If m_ChildNodes.Count = 2 Then
					Throw New InvalidOperationException(err_NodeCount)
				End If
				m_ChildNodes.Add(node)

			End Sub

			''' <summary>
			''' Parses values stored in the given <see cref="JObject" /> to reconstitute this BehaviourTreeNode.
			''' </summary>
			''' <param name="jObject">A <see cref="JObject" /> containing this node's properties</param>
			Public Overrides Sub ReadJson(jObject As JObject, Optional actionSource As Object = Nothing)
				Dim errorMessage As String = String.Empty

				ReadCommonJson(jObject, actionSource)
				m_Action = LeafNodeBase(Of TBag, TValue, TResult).CreateAction(jObject, actionSource, errorMessage)
				If Not String.IsNullOrEmpty(errorMessage) Then
					m_Name = errorMessage
				End If

			End Sub

			''' <summary>
			''' Invokes the configured <see cref="Func(Of TBag, TResult)">action</see>.
			''' If this returns <see cref="BehaviourTreeStatus.Success" />, the result of the first child node is returned, else the result of the second child node is returned.
			''' </summary>
			''' <param name="vb">A <see cref="StateManager(Of TypedValue, TResult)"/> implementation</param>
			''' <param name="timeStamp"> A <see cref="Single" /> value representing passed time</param>
			''' <returns>A <see cref="BehaviourTreeStatus"/> value</returns>
			Public Overrides Function Run(vb As TBag, timeStamp As Single) As TResult

				If m_ChildNodes.Count < 2 Then
					Throw New InvalidOperationException(err_NodeCount)
				End If

				Dim rst As New TResult With {.Result = BehaviourTreeStatus.Running}

				If (Level And LogEventType.Start) = LogEventType.Start Then
					vb.LogStarted(Name, rst, vb.Source, timeStamp)
				End If
				Try
					rst = If(m_Action(vb, m_Name, timeStamp, (Level And LogEventType.ValueChange) = LogEventType.ValueChange).Result = BehaviourTreeStatus.Success, m_ChildNodes(0).Run(vb, timeStamp), m_ChildNodes(1).Run(vb, timeStamp))
					If (Level And LogEventType.Finish) = LogEventType.Finish Then
						vb.LogFinished(Name, rst, vb.Source, timeStamp)
					End If
				Catch ex As Exception
					If (Level And LogEventType.Faulted) = LogEventType.Faulted Then
						vb.LogFaulted(Name, rst, vb.Source, timeStamp, ex)
					End If
				End Try

				Return rst

			End Function

			''' <summary>
			''' Parses values stored in this BehaviourTreeNode into a <see cref="JObject"/>.
			''' </summary>
			Public Overrides Function WriteJson() As JObject
				Dim obj As JObject = WriteCommonJson()

				obj.AddFirst(New JProperty(_A, m_Action.Method.Name)) ' add action node

				Return obj

			End Function

#End Region

#Region " Construction "

			''' <summary>
			''' Needed for Json deserialization.
			''' </summary>
			Public Sub New()

				m_ChildNodes = New List(Of IBehaviourTreeNode(Of TBag, TValue, TResult))

			End Sub

			''' <summary>
			''' Creates a new switch node.
			''' </summary>
			''' <param name="name">The name of this node</param>
			<DebuggerStepThrough()>
			Public Sub New(name As String, action As Func(Of TBag, String, Single, Boolean, TResult))

				Me.New
				m_Name = name
				m_Action = action

			End Sub

#End Region

		End Class

		''' <summary>
		''' Selects the first child node that succeeds, i.e tries successive nodes until it finds one that doesn't fail. Extended with a weight value.
		''' </summary>
		<DebuggerDisplay("{Name}")>
		Private Class WeightedSwitchNode
			Inherits SwitchNode
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
			''' Creates a new switch node.
			''' </summary>
			''' <param name="actionName">The name of this node</param>
			''' <param name="weight">The relative weight of the node (min: 0)</param>
			''' <param name="action">The <see cref="Func(Of TBag, BehaviourTreeStatus)">action</see> to invoke</param>
			<DebuggerStepThrough()>
			Public Sub New(actionName As String, weight As Integer, action As Func(Of TBag, String, Single, Boolean, TResult))

				MyBase.New(actionName, action)
				Me.Weight = Math.Max(0, weight)

			End Sub

#End Region

		End Class

	End Class

End Namespace