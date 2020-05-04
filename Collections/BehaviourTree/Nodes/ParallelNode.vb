Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Namespace Behaviour

	Partial Public Class BehaviourTreeBuilderBase(Of TBag As StateManager(Of TValue, TResult), TValue As {TypedValue, New}, TResult As {IBehaviourResult, New})

		''' <summary>
		''' Runs child nodes in parallel.
		''' </summary>
		<DebuggerDisplay("{Name}")>
		Private Class ParallelNode
			Inherits ParentNodeBase(Of TBag, TValue, TResult)

#Region " Objects and variables "

			Private Const _F As String = "f"
			Private Const _S As String = "s"

			Private m_ChildNodes As List(Of IBehaviourTreeNode(Of TBag, TValue, TResult))
			Private m_NumRequiredToFail As Integer
			Private m_NumRequiredToSucceed As Integer

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

			''' <summary>
			''' Number of child failures required to terminate with failure.
			''' </summary>
			<JsonProperty(PropertyName:="f")>
			Public ReadOnly Property NumRequiredToFail As Integer
				Get
					Return m_NumRequiredToFail
				End Get
			End Property

			''' <summary>
			''' Number of child successes required to terminate with success.
			''' </summary>
			<JsonProperty(PropertyName:="s")>
			Public ReadOnly Property NumRequiredToSucceed As Integer
				Get
					Return m_NumRequiredToSucceed
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
			''' Creates a ParallelNode from Json.
			''' </summary>
			''' <param name="json">Input Json string</param>
			''' <returns>An instance of type ParallelNode</returns>
			Public Overloads Shared Function FromJson(json As String, Optional actionSource As Object = Nothing) As ParallelNode

				Return DirectCast(ParentNodeBase(Of TBag, TValue, TResult).FromJson(json, actionSource), ParallelNode)

			End Function

			''' <summary>
			''' Parses values stored in the given <see cref="JObject" /> to reconstitute this BehaviourTreeNode.
			''' </summary>
			''' <param name="jObject">A <see cref="JObject" /> containing this node's properties</param>
			Public Overrides Sub ReadJson(jObject As JObject, Optional actionSource As Object = Nothing)

				ReadCommonJson(jObject, actionSource)
				m_NumRequiredToFail = jObject(_O)(_F).Value(Of Integer)
				m_NumRequiredToSucceed = jObject(_O)(_S).Value(Of Integer)

			End Sub

			''' <summary>
			''' Invokes the <see cref="Func(Of TBag, TResult)">action</see> on all this node's children in parallel.
			''' Firstly, if the number of child nodes that succeed is equal to or greater than the <see cref="NumRequiredToSucceed"/> value, <see cref="BehaviourTreeStatus.Success"/> is returned.
			''' Then, if the number of child nodes that fail is equal to or greater than the <see cref="NumRequiredToFail"/> value, <see cref="BehaviourTreeStatus.Failure"/> is returned.
			''' Else, <see cref="BehaviourTreeStatus.Running" /> is returned.
			''' </summary>
			''' <param name="vb">A <see cref="StateManager(Of TypedValue, TResult)"/> implementation</param>
			''' <param name="timeStamp"> A <see cref="Single" /> value representing passed time</param>
			''' <returns>A <see cref="BehaviourTreeStatus"/> value</returns>
			Public Overrides Function Run(vb As TBag, timeStamp As Single) As TResult
				Dim failures As Integer = 0
				Dim successes As Integer = 0
				Dim rst As New TResult With {.Result = BehaviourTreeStatus.Running}

				If (Level And LogEventType.Start) = LogEventType.Start Then
					vb.LogStarted(Name, rst, vb.Source, timeStamp)
				End If
				For Each child As IBehaviourTreeNode(Of TBag, TValue, TResult) In m_ChildNodes ' TODO: Make truly parallel and count failures/successes accurately (.AsParallel is not enough)
					rst = child.Run(vb, timeStamp)

					Select Case rst.Result
						Case BehaviourTreeStatus.Success
							successes += 1
						Case BehaviourTreeStatus.Failure
							failures += 1
					End Select
				Next
				If (NumRequiredToSucceed > 0) AndAlso (successes >= NumRequiredToSucceed) Then
					rst.Result = BehaviourTreeStatus.Success
				End If
				If (NumRequiredToFail > 0) AndAlso (failures >= NumRequiredToFail) Then
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

				m_ChildNodes = New List(Of IBehaviourTreeNode(Of TBag, TValue, TResult))

			End Sub

			''' <summary>
			''' Creates a new sequence node.
			''' </summary>
			''' <param name="actionName">The name of this node</param>
			''' <param name="numFailuresRequired">Number of child failures required to terminate with failure</param>
			''' <param name="numSuccessesRequired">Number of child successes required to terminate with success</param>
			<DebuggerStepThrough()>
			Public Sub New(actionName As String, numFailuresRequired As Integer, numSuccessesRequired As Integer)

				Me.New
				m_Name = actionName
				m_NumRequiredToFail = numFailuresRequired
				m_NumRequiredToSucceed = numSuccessesRequired

			End Sub

#End Region

		End Class

		''' <summary>
		''' Runs child nodes in parallel. Extended with a weight value.
		''' </summary>
		<DebuggerDisplay("{Name}")>
		Private Class WeightedParallelNode
			Inherits ParallelNode
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
			''' Creates a new parallel node.
			''' </summary>
			''' <param name="actionName">The name of this node</param>
			''' <param name="weight">The relative weight of the node (min: 0)</param>
			''' <param name="numFailuresRequired">Number of child failures required to terminate with failure</param>
			''' <param name="numSuccessesRequired">Number of child successes required to terminate with success</param>
			<DebuggerStepThrough()>
			Public Sub New(actionName As String, weight As Integer, numFailuresRequired As Integer, numSuccessesRequired As Integer)

				MyBase.New(actionName, numFailuresRequired, numSuccessesRequired)
				Me.Weight = Math.Max(0, weight)

			End Sub

#End Region

		End Class

	End Class

End Namespace