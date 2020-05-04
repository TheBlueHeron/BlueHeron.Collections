Imports System.ComponentModel
Imports System.IO
Imports System.Linq.Expressions
Imports System.Reflection
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Namespace Behaviour

    ''' <summary>
    ''' Base class for <see cref="IBehaviourTreeNode"/> implementations.
    ''' </summary>
    Public MustInherit Class LeafNodeBase(Of TBag As StateManager(Of TValue, TResult), TValue As {TypedValue, New}, TResult As {IBehaviourResult, New})
        Implements IBehaviourTreeNode(Of TBag, TValue, TResult)

#Region " Objects and variables "

        Protected m_Name As String
        Private m_Type As String

#End Region

#Region " Properties "

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
                If String.IsNullOrEmpty(m_Type) Then
                    m_Type = Me.GetType.AssemblyQualifiedName
                End If
                Return m_Type
            End Get
        End Property

        ''' <summary>
        ''' The minimum <see cref="LogEventType" /> that will be logged.
        ''' </summary>
        <JsonProperty(PropertyName:="l", DefaultValueHandling:=DefaultValueHandling.IgnoreAndPopulate, NullValueHandling:=NullValueHandling.Ignore), DefaultValue(GetType(LogEventType), "None")>
        Public Property Level As LogEventType = LogEventType.None Implements IBehaviourTreeNode(Of TBag, TValue, TResult).Level

#End Region

#Region " Public methods and functions"

        ''' <summary>
        ''' Creates a <see cref="Func(Of TBag, Single, TResult)" /> from the data in the given <see cref="JObject"/>.
        ''' </summary>
        ''' <param name="obj">The <see cref="JObject"/> that holds the deserialized properties and values</param>
        ''' <param name="actionSource">Instance of the object that holds all action functions</param>
        ''' <param name="errorMessage">Reference parameter for an error message, if any occurred</param>
        ''' <returns>A <see cref="Func(Of TBag, Single, TResult)" /> instance</returns>
        ''' <seealso>https://stackoverflow.com/questions/2933221/can-you-get-a-funct-or-similar-from-a-methodinfo-object</seealso>
        Friend Shared Function CreateAction(obj As JObject, actionSource As Object, ByRef errorMessage As String) As Func(Of TBag, String, Single, Boolean, TResult)

            If actionSource Is Nothing Then
                Throw New NullReferenceException(err_NoActionSource)
            End If
            Try
                Dim method As MethodInfo = actionSource.GetType().GetMethod(obj(_A).Value(Of String), BindingFlags.Instance Or BindingFlags.Public)

                If (method Is Nothing) OrElse (method.GetParameters.Count <> 4) OrElse (Not method.ReturnType.Equals(GetType(TResult))) Then
                    Throw New Exception(err_InvalidFunction)
                End If

                Dim inputValueBag As ParameterExpression = Expression.Parameter(GetType(StateManager(Of TValue, TResult))) ' input parameter for the StateManager
                Dim inputName As ParameterExpression = Expression.Parameter(GetType(String)) ' input parameter for the actionName
                Dim inputTimeStamp As ParameterExpression = Expression.Parameter(GetType(Single)) ' input parameter for the timestamp
                Dim inputLogEvent As ParameterExpression = Expression.Parameter(GetType(Boolean)) ' input parameter for the logEvent
                Dim target As Expression = Expression.Constant(actionSource) ' instance of object, containing the function
                Dim methodCall As MethodCallExpression = Expression.Call(target, method, inputValueBag, inputName, inputTimeStamp, inputLogEvent) ' expression of method call

                Return Expression.Lambda(Of Func(Of TBag, String, Single, Boolean, TResult))(methodCall, inputValueBag, inputName, inputTimeStamp, inputLogEvent).Compile() ' store lambda expression with method call as body
            Catch ex As Exception
                errorMessage = ex.Message
            End Try

            Return Nothing

        End Function

        ''' <summary>
        ''' Creates a BehaviourTreeNode from Json.
        ''' </summary>
        ''' <param name="json">Input Json string</param>
        ''' <param name="actionSource">Instance of the object, that holds the functions to call</param>
        ''' <returns>An instance of type TNode</returns>
        Public Shared Function FromJson(json As String, Optional actionSource As Object = Nothing) As IBehaviourTreeNode(Of TBag, TValue, TResult)
            Dim serializer As New JsonSerializer
            Dim node As IBehaviourTreeNode(Of TBag, TValue, TResult) = Nothing

            Using reader As New StringReader(json)
                Using readerJson As New JsonTextReader(reader)
                    Try
                        Dim jObject As JObject = DirectCast(serializer.Deserialize(readerJson), JObject)
                        Dim objectToInstantiate As String = jObject(_O)(_T).Value(Of String)
                        Dim objectType As Type = System.Type.GetType(objectToInstantiate)

                        If GetType(IBehaviourTreeNode(Of TBag, TValue, TResult)).IsAssignableFrom(objectType) Then
                            node = DirectCast(Activator.CreateInstance(objectType), IBehaviourTreeNode(Of TBag, TValue, TResult))
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
        ''' <param name="actionSource">Instance of the object, that holds the functions to call</param>
        Public MustOverride Sub ReadJson(jObject As JObject, Optional actionSource As Object = Nothing) Implements IBehaviourTreeNode(Of TBag, TValue, TResult).ReadJson

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

#End Region

    End Class

End Namespace