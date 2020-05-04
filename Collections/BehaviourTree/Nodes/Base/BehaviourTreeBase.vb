Imports System.IO
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Namespace Behaviour

    ''' <summary>
    ''' Base class for <see cref="IBehaviourTree(Of TBag, TValue, TResult)"/> implementations.
    ''' </summary>
    ''' <typeparam name="TBag">A <see cref="StateManager(Of TValue, TResult)" /> implementation</typeparam>
    ''' <typeparam name="TValue">A <see cref="TypedValue" /> implementation</typeparam>
    Public MustInherit Class BehaviourTreeBase(Of TBag As StateManager(Of TValue, TResult), TValue As {TypedValue, New}, TResult As {IBehaviourResult, New})
        Inherits ParentNodeBase(Of TBag, TValue, TResult)
        Implements IBehaviourTree(Of TBag, TValue, TResult)

#Region " Objects and variables "

        Private m_StartNode As IBehaviourTreeNode(Of TBag, TValue, TResult)

#End Region

#Region " Properties "

        ''' <summary>
        ''' Returns an immutable collection of this node's child nodes.
        ''' </summary>
        ''' <returns>A <see cref="IReadOnlyCollection(Of IBehaviourTreeNode(Of TBag, TValue, TResult))"/></returns>
        <JsonIgnore()>
        Public Overrides ReadOnly Property Children As BehaviourTreeNodeCollection(Of TBag, TValue, TResult)
            Get
                Return New BehaviourTreeNodeCollection(Of TBag, TValue, TResult)({m_StartNode})
            End Get
        End Property

#End Region

#Region " Public methods and functions "

        ''' <summary>
        ''' Adds a child <see cref="IBehaviourTreeNode"/> to this node.
        ''' </summary>
        ''' <param name="node">The <see cref="IBehaviourTreeNode"/> implementation to add</param>
        Public Overrides Sub AddChild(node As IBehaviourTreeNode(Of TBag, TValue, TResult))

            m_StartNode = node

        End Sub

        ''' <summary>
        ''' Returns the first <see cref="IBehaviourTreeNode(Of TBag, TValue, TResult)" /> with the given name.
        ''' </summary>
        ''' <param name="nodeName">Name of the <see cref="IBehaviourTreeNode(Of TBag, TValue, TResult)" /></param>
        ''' <returns>The matching <see cref="IBehaviourTreeNode(Of TBag, TValue, TResult)" /> if it exists, else Null/Nothing</returns>
        Public Function Find(nodeName As String) As IBehaviourTreeNode(Of TBag, TValue, TResult) Implements IBehaviourTree(Of TBag, TValue, TResult).Find

            Return Find(nodeName, Children)

        End Function

        ''' <summary>
        ''' Creates a BehaviourTreeNode from Json.
        ''' </summary>
        ''' <param name="json">Input Json string</param>
        ''' <param name="actionSource">Instance of the object, that holds the functions to call</param>
        ''' <returns>An instance of type IParentBehaviourTreeNode</returns>
        Public Shared Shadows Function FromJson(json As String, Optional actionSource As Object = Nothing) As IBehaviourTree(Of TBag, TValue, TResult)
            Dim serializer As New JsonSerializer
            Dim root As IBehaviourTree(Of TBag, TValue, TResult) = Nothing

            Using reader As New StringReader(json)
                Using readerJson As New JsonTextReader(reader)
                    Try
                        Dim jObject As JObject = DirectCast(serializer.Deserialize(readerJson), JObject)
                        Dim objectToInstantiate As String = jObject(_O)(_T).Value(Of String)
                        Dim objectType As Type = System.Type.GetType(objectToInstantiate)

                        If GetType(IBehaviourTree(Of TBag, TValue, TResult)).IsAssignableFrom(objectType) Then
                            root = DirectCast(Activator.CreateInstance(objectType), IBehaviourTree(Of TBag, TValue, TResult))
                            root.ReadJson(jObject, actionSource)
                        End If
                    Catch ex As Exception
                        Throw New ArgumentException(String.Format(err_FromJson, root.GetType.ToString, json, ex.Message))
                    End Try
                End Using
            End Using

            Return root

        End Function

        ''' <summary>
        ''' Parses values stored in the given <see cref="JObject" /> to reconstitute this Behaviour Tree.
        ''' </summary>
        ''' <param name="jObject">A <see cref="JObject" /> containing this node's properties</param>
        Public Overrides Sub ReadJson(jObject As JObject, Optional actionSource As Object = Nothing)

            ReadCommonJson(jObject, actionSource)

        End Sub

        ''' <summary>
        ''' Update the behaviour tree.
        ''' </summary>
        ''' <param name="vb">A <see cref="StateManager(Of TypedValue, TResult)"/> implementation, representing state values and passed time</param>
        ''' <param name="timeStamp"> A <see cref="Single" /> value representing passed time</param>
        ''' <returns>The resulting TResult</returns>
        Public Overrides Function Run(vb As TBag, timeStamp As Single) As TResult

            Return m_StartNode.Run(vb, timeStamp)

        End Function

        ''' <summary>
        ''' Parses values stored in this Behaviour Tree into a <see cref="JObject"/>.
        ''' </summary>
        Public Overrides Function WriteJson() As JObject

            Return WriteCommonJson()

        End Function

#End Region

#Region " Private methods and functions "

        ''' <summary>
        ''' Recursively and depth-first goes through the given collection of nodes and returns the first <see cref="IBehaviourTreeNode(Of TBag, TValue, TResult)" /> it finds with the given name.
        ''' </summary>
        ''' <param name="nodeName">Name of the <see cref="IBehaviourTreeNode(Of TBag, TValue, TResult)" /></param>
        ''' <param name="children">The collection to recursively search through</param>
        ''' <returns>The matching <see cref="IBehaviourTreeNode(Of TBag, TValue, TResult)" /> if it exists, else Null/Nothing</returns>
        Private Function Find(nodeName As String, children As IEnumerable(Of IBehaviourTreeNode(Of TBag, TValue, TResult))) As IBehaviourTreeNode(Of TBag, TValue, TResult)
            Dim rst As IBehaviourTreeNode(Of TBag, TValue, TResult) = Nothing

            children.ForEach(Sub(n)
                                 If rst Is Nothing Then
                                     If n.Name = nodeName Then
                                         rst = n
                                     Else
                                         If GetType(IParentBehaviourTreeNode(Of TBag, TValue, TResult)).IsAssignableFrom(n.GetType) Then
                                             rst = Find(nodeName, DirectCast(n, IParentBehaviourTreeNode(Of TBag, TValue, TResult)).Children)
                                         End If
                                     End If
                                 End If
                             End Sub)

            Return rst

        End Function

#End Region

#Region " Construction "

        ''' <summary>
        ''' Creates a new root node.
        ''' </summary>
        ''' <param name="actionName">The name of this node</param>
        <DebuggerStepThrough()>
        Public Sub New(actionName As String)

            m_Name = actionName

        End Sub

#End Region

    End Class

End Namespace