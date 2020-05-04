﻿Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Namespace Behaviour

	Partial Public Class BehaviourTreeBuilderBase(Of TBag As StateManager(Of TValue, TResult), TValue As {TypedValue, New}, TResult As {IBehaviourResult, New})

		''' <summary>
		''' Selects the first child node that succeeds, i.e tries successive nodes until it finds one that doesn't fail.
		''' </summary>
		<DebuggerDisplay("{Name}")>
		Public Class SelectorNode
			Inherits ParentNodeBase(Of TBag, TValue, TResult)

#Region " Objects and variables "

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

				m_ChildNodes.Add(node)

			End Sub

			''' <summary>
			''' Parses values stored in the given <see cref="JObject" /> to reconstitute this BehaviourTreeNode.
			''' </summary>
			''' <param name="jObject">A <see cref="JObject" /> containing this node's properties</param>
			Public Overrides Sub ReadJson(jObject As JObject, Optional actionSource As Object = Nothing)

				ReadCommonJson(jObject, actionSource)

			End Sub

			''' <summary>
			''' Invokes the <see cref="Func(Of TBag, TResult)">action</see> on all this node's children in sequence.
			''' The value of the first child node that returns a value other than <see cref="BehaviourTreeStatus.Failure" /> is returned immediately.
			''' This means that <see cref="BehaviourTreeStatus.Failure" /> is only returned when all children fail.
			''' </summary>
			''' <param name="vb">A <see cref="StateManager(Of TypedValue, TResult)"/> implementation</param>
			''' <param name="timeStamp"> A <see cref="Single" /> value representing passed time</param>
			''' <returns>A TResult value</returns>
			Public Overrides Function Run(vb As TBag, timeStamp As Single) As TResult
				Dim rst As New TResult With {.Result = BehaviourTreeStatus.Failure}

				If (Level And LogEventType.Start) = LogEventType.Start Then
					vb.LogStarted(Name, New TResult With {.Result = BehaviourTreeStatus.Running}, vb.Source, timeStamp)
				End If
				For Each child As IBehaviourTreeNode(Of TBag, TValue, TResult) In m_ChildNodes
					rst = child.Run(vb, timeStamp)

					If rst.Result <> BehaviourTreeStatus.Failure Then
						Exit For
					End If
				Next
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

				m_ChildNodes = New List(Of IBehaviourTreeNode(Of TBag, TValue, TResult))

			End Sub

			''' <summary>
			''' Creates a new selector node.
			''' </summary>
			''' <param name="actionName">The name of this node</param>
			<DebuggerStepThrough()>
			Public Sub New(actionName As String)

				Me.New
				m_Name = actionName

			End Sub

#End Region

		End Class

		''' <summary>
		''' Selects the first child node that succeeds, i.e tries successive nodes until it finds one that doesn't fail. Extended with a weight value.
		''' </summary>
		<DebuggerDisplay("{Name}")>
		Public Class WeightedSelectorNode
			Inherits SelectorNode
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
			''' Creates a new selector node.
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