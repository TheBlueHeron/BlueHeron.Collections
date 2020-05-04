Imports Newtonsoft.Json.Linq

Namespace Behaviour

	''' <summary>
	''' Interface definition for behaviour tree nodes.
	''' </summary>
	''' <typeparam name="TBag">A <see cref="StateManager(Of TValue, TResult)" /> implementation</typeparam>
	''' <typeparam name="TValue">A <see cref="TypedValue" /> implementation</typeparam>
	''' <typeparam name="TResult">The result of this node's run</typeparam>
	Public Interface IBehaviourTreeNode(Of TBag As StateManager(Of TValue, TResult), TValue As {TypedValue, New}, TResult As {IBehaviourResult, New})

		''' <summary>
		''' The minimum <see cref="LogEventType" /> that will be logged.
		''' </summary>
		Property Level As LogEventType

		''' <summary>
		''' The name of this node.
		''' </summary>
		ReadOnly Property Name As String

		''' <summary>
		''' Parses values stored in the given <see cref="JObject" /> to reconstitute this BehaviourTreeNode.
		''' </summary>
		''' <param name="jObject">A <see cref="JObject" /> containing this node's properties</param>
		''' <param name="actionSource">The instance that performs the action</param>
		Sub ReadJson(jObject As JObject, Optional actionSource As Object = Nothing)

		''' <summary>
		''' Update the behaviour tree.
		''' </summary>
		''' <param name="vb">A <see cref="StateManager(Of TypedValue, TResult)"/> implementation, representing state values</param>
		''' <param name="timeStamp"> A <see cref="Single" /> value representing passed time</param>
		''' <returns>The result of type TResult</returns>
		Function Run(vb As TBag, timeStamp As Single) As TResult

		''' <summary>
		''' Returns a Json string representation of this BehaviourTreeNode.
		''' </summary>
		Function ToJson() As String

		''' <summary>
		''' Fully qualified type name of this BehaviourTreeNode.
		''' </summary>
		ReadOnly Property Type As String

		''' <summary>
		''' Parses values stored in this BehaviourTreeNode into a <see cref="JObject" />.
		''' </summary>
		Function WriteJson() As JObject

	End Interface

End Namespace