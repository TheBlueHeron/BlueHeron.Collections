Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Namespace Behaviour

    Partial Public Class BehaviourTreeBuilderBase(Of TBag As StateManager(Of TValue, TResult), TValue As {TypedValue, New}, TResult As {IBehaviourResult, New})

        ''' <summary>
        ''' A leaf node that executes an <see cref="Func(Of TBag, TResult)">action</see>.
        ''' </summary>
        <DebuggerDisplay("{Name}")>
        Private Class ActionNode
            Inherits LeafNodeBase(Of TBag, TValue, TResult)

#Region " Objects and variables "

            Private m_Action As Func(Of TBag, String, Single, Boolean, TResult)

#End Region

#Region " Public methods and functions "

            ''' <summary>
            ''' Creates an ActionNode from Json.
            ''' </summary>
            ''' <param name="json">Inout Json string</param>
            ''' <param name="actionSource">Instance of the object, that holds the functions to call</param>
            ''' <returns>An instance of type ActionNode</returns>
            Public Overloads Shared Function FromJson(json As String, actionSource As Object) As ActionNode

                Return DirectCast(LeafNodeBase(Of TBag, TValue, TResult).FromJson(json, actionSource), ActionNode)

            End Function

            ''' <summary>
            ''' Invoke this node's <see cref="Func(Of TBag, TResult)">action</see>.
            ''' </summary>
            ''' <param name="vb">A <see cref="Statemanager(Of TypedValue, TResult)"/> implementation of type TBag</param>
            ''' <param name="timeStamp"> A <see cref="Single" /> value representing passed time</param>
            ''' <returns>A TResult value</returns>
            Public Overrides Function Run(vb As TBag, timeStamp As Single) As TResult
                Dim rst As TResult

                If (Level And LogEventType.Start) = LogEventType.Start Then
                    vb.LogStarted(Name, rst, vb.Source, timeStamp)
                End If
                Try
                    rst = m_Action(vb, m_Name, timeStamp, (Level And LogEventType.ValueChange) = LogEventType.ValueChange)
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
            ''' Parses values stored in the given <see cref="JObject" /> to reconstitute this BehaviourTreeNode.
            ''' </summary>
            ''' <param name="jObject">A <see cref="JObject" /> containing this node's properties</param>
            ''' <param name="actionSource">Instance of the object, that holds the functions to call</param>
            Public Overrides Sub ReadJson(jObject As JObject, Optional actionSource As Object = Nothing)
                Dim errorMessage As String = String.Empty

                m_Action = CreateAction(jObject, actionSource, errorMessage)
                If String.IsNullOrEmpty(errorMessage) Then
                    Dim lvl As JToken = jObject(_O)(_L)

                    If Not lvl Is Nothing Then
                        Level = DirectCast(lvl.Value(Of Integer), LogEventType)
                    End If
                    m_Name = jObject(_O)(_N).Value(Of String)
                Else
                    m_Name = errorMessage
                End If

            End Sub

            ''' <summary>
            ''' Parses values stored in this BehaviourTreeNode into a <see cref="JObject"/>.
            ''' </summary>
            Public Overrides Function WriteJson() As JObject

                Return JObject.FromObject(New With {.a = m_Action.Method.Name, .o = Me})

            End Function

#End Region

#Region " Construction "

            ''' <summary>
            ''' Needed for Json deserialization.
            ''' </summary>
            Public Sub New()
            End Sub

            ''' <summary>
            ''' Creates a new action node.
            ''' </summary>
            ''' <param name="actionName">The name of this node</param>
            ''' <param name="action">The <see cref="Func(Of TBag, String, Single, Boolean, TResult)">action</see> to invoke</param>
            <DebuggerStepThrough()>
            Public Sub New(actionName As String, action As Func(Of TBag, String, Single, Boolean, TResult))

                m_Name = actionName
                m_Action = action

            End Sub

#End Region

        End Class

        ''' <summary>
        ''' An <see cref="ActionNode" />, extended with a weight value.
        ''' </summary>
        <DebuggerDisplay("{Name}")>
        Private Class WeightedActionNode
            Inherits ActionNode
            Implements IWeightedNode(Of TBag, TValue, TResult)

#Region " Properties "

            ''' <summary>
            ''' Weight value (default: 1).
            ''' </summary>
            <JsonProperty(PropertyName:="w")>
            Public Property Weight As Integer = 1 Implements IWeightedNode(Of TBag, TValue, TResult).Weight

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
            ''' Creates a new action node.
            ''' </summary>
            ''' <param name="actionName">The name of this node</param>
            ''' <param name="weight">The relative weight of the node (min: 0)</param>
            ''' <param name="action">The <see cref="Func(Of TBag, String, Single, Boolean, TResult)">action</see> to invoke</param>
            <DebuggerStepThrough()>
            Public Sub New(actionName As String, weight As Integer, action As Func(Of TBag, String, Single, Boolean, TResult))

                MyBase.New(actionName, action)
                Me.Weight = Math.Max(0, weight)

            End Sub

#End Region

        End Class

    End Class

End Namespace