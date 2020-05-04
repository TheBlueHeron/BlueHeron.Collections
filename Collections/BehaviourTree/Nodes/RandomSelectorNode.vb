Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Namespace Behaviour

	Partial Public Class BehaviourTreeBuilderBase(Of TBag As StateManager(Of TValue, TResult), TValue As {TypedValue, New}, TResult As {IBehaviourResult, New})

		''' <summary>
		''' Returns the TResult of a random child node's Run(...) call.
		''' </summary>
		Public Class RandomSelectorNode
			Inherits SelectorNode

#Region " Objects and variables "

			Private ReadOnly rnd As New Random(Environment.TickCount)

#End Region

#Region " Public methods and functions "

			''' <summary>
			''' Invokes the <see cref="Func(Of TBag, TResult)">action</see> on a random node of this node's child collection
			''' The value of this node's Run(...) is returned.
			''' </summary>
			''' <param name="vb">A <see cref="StateManager(Of TypedValue, TResult)"/> implementation</param>
			''' <param name="timeStamp"> A <see cref="Single" /> value representing passed time</param>
			''' <returns>A <see cref="BehaviourTreeStatus"/> value</returns>
			Public Overrides Function Run(vb As TBag, timeStamp As Single) As TResult
				Dim idx As Integer = CInt(Math.Floor(rnd.Next(0, (m_ChildNodes.Count * 100) - 1) / 100)) ' otherwise the top index gets never chosen
				Dim rst As New TResult With {.Result = BehaviourTreeStatus.Running}

				If (Level And LogEventType.Start) = LogEventType.Start Then
					vb.LogStarted(Name, rst, vb.Source, timeStamp)
				End If
				rst = m_ChildNodes(idx).Run(vb, timeStamp)
				If (Level And LogEventType.Finish) = LogEventType.Finish Then
					vb.LogFinished(Name, rst, vb.Source, timeStamp)
				End If

				Return rst

			End Function

#End Region

#Region " Construction "

			''' <summary>
			''' Needed by Json serializer.
			''' </summary>
			Public Sub New()
			End Sub

			''' <summary>
			''' Creates a new random selector node.
			''' </summary>
			''' <param name="actionName">The name of this node</param>
			<DebuggerStepThrough()>
			Public Sub New(actionName As String)

				MyBase.New(actionName)

			End Sub

#End Region

		End Class

		''' <summary>
		''' Returns the TResult of a random child node's Run(...) call. Extended with a weight value.
		''' </summary>
		Public Class WeightedRandomSelectorNode
			Inherits RandomSelectorNode
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

		''' <summary>
		''' Returns the <see cref="BehaviourTreeStatus"/> of a weighted random child node's Run(...) call.
		''' </summary>
		Public Class WeightedRandomNode
			Inherits SelectorNode

#Region " Objects and variables "

			Private m_Rnd As Random
			Private m_TotalWeight As Integer

#End Region

#Region " Public methods and functions "

			''' <summary>
			''' Overridden to accept only <see cref="IWeightedNode(Of TBag, TValue, TResult)" /> and <see cref="IWeightedParentNode(Of TBag, TValue, TResult)" /> objects.
			''' </summary>
			''' <param name="node">A <see cref="IWeightedNode(Of TBag, TValue, TResult)" /> or <see cref="IWeightedParentNode(Of TBag, TValue, TResult)" /> object</param>
			Public Overrides Sub AddChild(node As IBehaviourTreeNode(Of TBag, TValue, TResult))

				If Not GetType(IWeighted).IsAssignableFrom(node.GetType()) Then
					Throw New InvalidOperationException(err_NodeWeighted)
				End If
				MyBase.AddChild(node)
				m_TotalWeight += DirectCast(node, IWeighted).Weight

			End Sub

			''' <summary>
			''' Invokes the <see cref="Func(Of TBag, TResult)">action</see> on a weighted random node of this node's child collection.
			''' The value of this node's Run(...) is returned.
			''' </summary>
			''' <param name="vb">A <see cref="StateManager(Of TypedValue, TResult)"/> implementation</param>
			''' <param name="timeStamp"> A <see cref="Single" /> value representing passed time</param>
			''' <returns>A <see cref="BehaviourTreeStatus"/> value</returns>
			Public Overrides Function Run(vb As TBag, timeStamp As Single) As TResult
				Dim selected As IWeighted ' currently selected element
				Dim elements As IEnumerable(Of IWeighted) = Children.OfType(Of IWeighted)
				Dim rst As New TResult With {.Result = BehaviourTreeStatus.Running}

				If (Level And LogEventType.Start) = LogEventType.Start Then
					vb.LogStarted(Name, rst, vb.Source, timeStamp)
				End If
				If elements.Count = 0 Then
					rst.Result = BehaviourTreeStatus.Failure
				Else
					selected = elements(0) ' default selection
					For Each element As IWeighted In elements
						If m_Rnd.Next(m_TotalWeight + element.Weight) >= m_TotalWeight Then ' probability results from Weight / (TotalWeight + Weight)
							selected = element ' probability is higher, select this element
						End If
					Next
					rst = DirectCast(selected, IBehaviourTreeNode(Of TBag, TValue, TResult)).Run(vb, timeStamp)
				End If
				If (Level And LogEventType.Finish) = LogEventType.Finish Then
					vb.LogFinished(Name, rst, vb.Source, timeStamp)
				End If

				Return rst

			End Function

#End Region

#Region " Construction "

			''' <summary>
			''' Needed by Json serializer.
			''' </summary>
			Public Sub New()

				MyBase.New
				m_Rnd = New Random(Environment.TickCount)

			End Sub

			''' <summary>
			''' Creates a new random selector node.
			''' </summary>
			''' <param name="actionName">The name of this node</param>
			<DebuggerStepThrough()>
			Public Sub New(actionName As String)

				MyBase.New(actionName)
				m_Rnd = New Random(Environment.TickCount)

			End Sub

#End Region

		End Class

	End Class

End Namespace