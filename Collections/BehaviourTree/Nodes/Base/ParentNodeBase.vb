Imports System.ComponentModel
Imports System.IO
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Namespace Behaviour

    ''' <summary>
    ''' Base class for <see cref="IParentBehaviourTreeNode"/> implementations.
    ''' </summary>
    ''' <typeparam name="TBag">A <see cref="StateManager(Of TValue, TResult)" /> implementation</typeparam>
    ''' <typeparam name="TValue">A <see cref="TypedValue" /> implementation</typeparam>
    Public MustInherit Class ParentNodeBase(Of TBag As StateManager(Of TValue, TResult), TValue As {TypedValue, New}, TResult As {IBehaviourResult, New})
        Implements IParentBehaviourTreeNode(Of TBag, TValue, TResult)

#Region " Objects and variables "

        Protected m_Name As String

#End Region

#Region " Properties "

        ''' <summary>
        ''' Returns an immutable collection of this node's child nodes.
        ''' </summary>
        ''' <returns>A <see cref="IReadOnlyCollection(Of IBehaviourTreeNode(Of TBag, TValue, TResult))"/></returns>
        Public MustOverride ReadOnly Property Children As BehaviourTreeNodeCollection(Of TBag, TValue, TResult) Implements IParentBehaviourTreeNode(Of TBag, TValue, TResult).Children

        ''' <summary>
        ''' The minimum <see cref="LogEventType" /> that will be logged.
        ''' </summary>
        <JsonProperty(PropertyName:="l", DefaultValueHandling:=DefaultValueHandling.IgnoreAndPopulate, NullValueHandling:=NullValueHandling.Ignore), DefaultValue(GetType(LogEventType), "None")>
        Public Property Level As LogEventType = LogEventType.None Implements IBehaviourTreeNode(Of TBag, TValue, TResult).Level

        ''' <summary>
        ''' The name of this node.
        ''' </summary>
        <JsonProperty(PropertyName:="n")>
        Public ReadOnly Property Name As String Implements IBehaviourTreeNode(Of TBag, TValue, TResult).Name
            Get
                Return m_Name
            End Get
        End Property

        ''' <summary>
        ''' Fully qualified type name of this BehaviourTreeNode.
        ''' </summary>
        <JsonProperty(PropertyName:="t")>
        Public ReadOnly Property Type As String Implements IBehaviourTreeNode(Of TBag, TValue, TResult).Type
            Get
                Return Me.GetType.ToString
            End Get
        End Property

#End Region

#Region " Public methods and functions "

        ''' <summary>
        ''' Adds a child <see cref="IBehaviourTreeNode"/> to this node.
        ''' </summary>
        ''' <param name="node">The <see cref="IBehaviourTreeNode"/> implementation to add</param>
        Public MustOverride Sub AddChild(node As IBehaviourTreeNode(Of TBag, TValue, TResult)) Implements IParentBehaviourTreeNode(Of TBag, TValue, TResult).AddChild

        ''' <summary>
        ''' Creates a BehaviourTreeNode from Json.
        ''' </summary>
        ''' <param name="json">Input Json string</param>
        ''' <param name="actionSource">Instance of the object, that holds the functions to call</param>
        ''' <returns>An instance of type IParentBehaviourTreeNode</returns>
        Public Shared Function FromJson(json As String, Optional actionSource As Object = Nothing) As IParentBehaviourTreeNode(Of TBag, TValue, TResult)
            Dim serializer As New JsonSerializer
            Dim node As IParentBehaviourTreeNode(Of TBag, TValue, TResult) = Nothing

            Using reader As New StringReader(json)
                Using readerJson As New JsonTextReader(reader)
                    Try
                        Dim jObject As JObject = DirectCast(serializer.Deserialize(readerJson), JObject)
                        Dim objectToInstantiate As String = jObject(_O)(_T).Value(Of String)
                        Dim objectType As Type = System.Type.GetType(objectToInstantiate)

                        If GetType(IParentBehaviourTreeNode(Of TBag, TValue, TResult)).IsAssignableFrom(objectType) Then
                            node = DirectCast(Activator.CreateInstance(objectType), IParentBehaviourTreeNode(Of TBag, TValue, TResult))
                            node.ReadJson(jObject, actionSource)
                        End If
                    Catch ex As Exception
                        Throw New ArgumentException(String.Format(err_FromJson, node.GetType.ToString, json, ex.Message))
                    End Try
                End Using
            End Using

            Return node

        End Function

        ''' <summary>
        ''' Parses values stored in the given <see cref="JObject" /> to reconstitute this BehaviourTreeNode.
        ''' </summary>
        ''' <param name="jObject">A <see cref="JObject" /> containing this node's properties</param>
        Public MustOverride Sub ReadJson(jObject As JObject, Optional actionSource As Object = Nothing) Implements IBehaviourTreeNode(Of TBag, TValue, TResult).ReadJson

        ''' <summary>
        ''' Parses values shared by all parent nodes: Name and Children.
        ''' </summary>
        ''' <param name="jObject">The <see cref="JObject"/>, containing the values</param>
        ''' <param name="actionSource">Instance of the object, that holds the functions to call</param>
        Protected Sub ReadCommonJson(jObject As JObject, Optional actionSource As Object = Nothing)

            Try
                Dim lvl As JToken = jObject(_O)(_L)

                If Not lvl Is Nothing Then
                    Level = DirectCast(lvl.Value(Of Integer), LogEventType)
                End If
                m_Name = jObject(_O)(_N).Value(Of String)
                For Each childObject As JObject In jObject(_C).Children
                    Dim objectToInstantiate As String = childObject(_O)(_T).Value(Of String)
                    Dim objectType As Type = System.Type.GetType(objectToInstantiate)

                    If GetType(IBehaviourTreeNode(Of TBag, TValue, TResult)).IsAssignableFrom(objectType) Then
                        Dim node As IBehaviourTreeNode(Of TBag, TValue, TResult) = DirectCast(Activator.CreateInstance(objectType), IBehaviourTreeNode(Of TBag, TValue, TResult))

                        node.ReadJson(childObject, actionSource)
                        AddChild(node)
                    End If
                Next
            Catch ex As Exception
                m_Name = ex.Message
            End Try

        End Sub

        ''' <summary>
        ''' Update the behaviour tree.
        ''' </summary>
        ''' <param name="vb">A <see cref="StateManager(Of TypedValue, TResult)"/> implementation, representing state values and passed time</param>
        ''' <param name="timeStamp"> A <see cref="Single" /> value representing passed time</param>
        ''' <returns>The resulting TResult</returns>
        Public MustOverride Function Run(vb As TBag, timeStamp As Single) As TResult Implements IBehaviourTreeNode(Of TBag, TValue, TResult).Run

        ''' <summary>
        ''' Serializes this BehaviourTreeNode to a Json string representation.
        ''' </summary>
        Public Function ToJson() As String Implements IBehaviourTreeNode(Of TBag, TValue, TResult).ToJson
            Dim serializer As New JsonSerializer With {.Formatting = Formatting.None}

            Using writer As New StringWriter()
                Using writerJson As New JsonTextWriter(writer)
                    Try
                        serializer.Serialize(writerJson, WriteJson)
                        Return writer.GetStringBuilder.ToString()
                    Catch ex As Exception
                        Return ex.Message
                    End Try
                End Using
            End Using

        End Function

        ''' <summary>
        ''' Parses values stored in this BehaviourTreeNode into a <see cref="JObject"/>.
        ''' </summary>
        Public MustOverride Function WriteJson() As JObject Implements IBehaviourTreeNode(Of TBag, TValue, TResult).WriteJson

        ''' <summary>
        ''' Parses values shared by all parent nodes: Name and Children.
        ''' </summary>
        ''' <returns>A <see cref="JObject"/> containing all shared values</returns>
        Public Function WriteCommonJson() As JObject
            Dim childObjects As New List(Of JObject)

            For Each c As IBehaviourTreeNode(Of TBag, TValue, TResult) In Children
                childObjects.Add(c.WriteJson)
            Next
            Return JObject.FromObject(New With {.o = Me, .c = childObjects})

        End Function

#End Region

    End Class

End Namespace