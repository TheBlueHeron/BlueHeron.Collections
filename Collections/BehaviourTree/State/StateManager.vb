Imports System.Collections.Specialized
Imports System.ComponentModel
Imports System.IO
Imports System.Threading
Imports BlueHeron.Collections.Generic
Imports Microsoft.Extensions.Logging
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Namespace Behaviour

    ''' <summary>
    ''' Container for an observable timestamped collection of strongly typed values.
    ''' Notification events are fired when the collection changes and when stored values change.
    ''' </summary>
    ''' <remarks>This object is thread-safe</remarks>
    ''' <typeparam name="TValue">A <see cref="TypedValue" /> implementation</typeparam>
    ''' <typeparam name="TResult">The type of the result of node runs</typeparam>
    Public MustInherit Class StateManager(Of TValue As {TypedValue, New}, TResult As {IBehaviourResult, New})
        Implements INotifyCollectionChanged, IEnumerable(Of TValue)

#Region " Objects and variables "

        ''' <summary>
        ''' The key under which any exception is stored.
        ''' </summary>
        Public Const ErrorKey As String = "Error"

        ''' <summary>
        ''' Event is fired when the value of an item in the collection changes.
        ''' </summary>
        Public Event PropertyChanged As EventHandler(Of ValueBagPropertyChangedEventArgs)
        ''' <summary>
        ''' Event is fired when the collection of values changes.
        ''' </summary>
        Public Event CollectionChanged As NotifyCollectionChangedEventHandler Implements INotifyCollectionChanged.CollectionChanged

        Private ReadOnly m_ItemsLock As ReaderWriterLockSlim ' for thread-safe collection access
        Protected m_Logger As ILogger
        Friend m_Table As HashTable(Of String, TValue) ' inner collection

#End Region

#Region " Properties "

        ''' <summary>
        ''' Used for thread-safe event handling.
        ''' </summary>
        ''' <returns></returns>
        Public Property Context As SynchronizationContext

        ''' <summary>
        ''' Determines whether one or more properties caused an error to occur during serialization from Json.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property HasError As Boolean
            Get
                Return m_Table.ContainsKey(ErrorKey)
            End Get
        End Property

        ''' <summary>
        ''' Returns the <see cref="TypedValue"/> with the given key or name.
        ''' </summary>
        ''' <param name="key">The name or key of the <see cref="TypedValue"/></param>
        ''' <returns>A  the <see cref="TypedValue"/>, if it exists, else Null / Nothing</returns>
        Default Public ReadOnly Property Item(key As String) As TValue
            Get
                m_ItemsLock.EnterReadLock()
                Try
                    Return m_Table(key)
                Finally
                    m_ItemsLock.ExitReadLock()
                End Try
            End Get
        End Property

        ''' <summary>
        ''' The owner of this object.
        ''' </summary>
        Public Property Source As String

#End Region

#Region " Public methods and functions "

        ''' <summary>
        ''' Adds the given <see cref="TypedValue" /> to the collection.
        ''' </summary>
        ''' <param name="value">A <see cref="TypedValue"/> implementation of type T</param>
        Public Sub Add(value As TValue)
            Dim old As IList(Of TValue) = GetValues()

            m_ItemsLock.EnterWriteLock()
            m_Table.Add(value.Key, value)
            m_ItemsLock.ExitWriteLock()

            OnCollectionChanged(NotifyCollectionChangedAction.Add, m_Table.Values.ToList, old)

        End Sub

        ''' <summary>
        ''' Clears the collection.
        ''' </summary>
        Public Sub Clear()
            Dim old As IList(Of TValue) = GetValues()

            m_ItemsLock.EnterWriteLock()
            m_Table.Clear()
            m_ItemsLock.ExitWriteLock()
            OnCollectionChanged(NotifyCollectionChangedAction.Reset, New List(Of TValue), old)

        End Sub

        ''' <summary>
        ''' Returns a boolean, determining whether a <see cref="TypedValue" /> with the given key exists in the collection.
        ''' </summary>
        ''' <param name="key">The key to look for</param>
        ''' <returns>Boolean, True if the <see cref="TypedValue"/> exists</returns>
        Public Function ContainsKey(key As String) As Boolean

            m_ItemsLock.EnterReadLock()
            Return m_Table.ContainsKey(key)
            m_ItemsLock.ExitReadLock()

        End Function

        ''' <summary>
        ''' Creates a StateManager of type TTarget from the given JSON.
        ''' </summary>
        ''' <param name="json">The input JSON</param>
        ''' <param name="isCompressed">The input is compressed (i.e. using optimal GZip compression!)</param>
        ''' <param name="context">The <see cref="SynchronizationContext"/> to be used by the StateManager</param>
        ''' <returns>An intance of type TTarget</returns>
        Public Shared Function FromJson(Of TTarget As {StateManager(Of TValue, TResult), New})(json As String, Optional isCompressed As Boolean = False, Optional context As SynchronizationContext = Nothing, Optional logger As ILogger = Nothing) As TTarget
            Dim bag As New TTarget
            Dim serializer As New JsonSerializer

            If Not context Is Nothing Then
                bag.Context = context
            End If
            If Not logger Is Nothing Then
                bag.m_Logger = logger
            End If

            Using reader As New StringReader(If(isCompressed, Compressor.Decompress(json), json))
                Using readerJson As New JsonTextReader(reader)
                    Try
                        bag.ReadJson(bag, DirectCast(serializer.Deserialize(readerJson), JObject))
                    Catch ex As Exception
                        bag.Add(New TValue With {.m_Key = ErrorKey, .m_Value = ex})
                    End Try
                End Using
            End Using

            Return bag

        End Function

        ''' <summary>
        ''' Returns the value with the given key.
        ''' If the value cannot be cast directly to the given type, Null / Nothing is returned.
        ''' </summary>
        ''' <typeparam name="T">The type of the value</typeparam>
        ''' <param name="key">The key of the <see cref="TypedValue"/></param>
        <DebuggerStepThrough()>
        Public Overridable Function [Get](Of T)(key As String) As T
            Dim val As TValue = m_Table(key)

            If TypeOf (val.Value) Is T Then
                Return DirectCast(val.Value, T)
            End If

            Return Nothing

        End Function

        ''' <summary>
        ''' Returns the value with the given key.
        ''' </summary>
        ''' <param name="key">The key of the <see cref="TypedValue"/></param>
        <DebuggerStepThrough()>
        Public Overridable Function [Get](key As String) As Object

            Return m_Table(key).Value

        End Function

        ''' <summary>
        ''' Returns the <see cref="IEnumerator(Of TValue)" />.
        ''' </summary>
        Public Function GetEnumerator() As IEnumerator(Of TValue) Implements IEnumerable(Of TValue).GetEnumerator

            Return GetValues.GetEnumerator()

        End Function

        ''' <summary>
        ''' Returns the inner collection's values as <see cref="IList(Of TValue)" />.
        ''' </summary>
        Public Function GetValues() As IList(Of TValue)

            m_ItemsLock.EnterReadLock()
            Try
                Return m_Table.Values.ToList
            Finally
                m_ItemsLock.ExitReadLock()
            End Try

        End Function

        ''' <summary>
        ''' Logs a faulted step in the behaviour tree.
        ''' </summary>
        Public Sub LogFaulted(nodename As String, result As IBehaviourResult, source As String, timeStamp As Single, Optional ex As Exception = Nothing)

            BehaviourLog.LogBehaviourFaulted(m_Logger, LogEventType.Faulted, nodename, result, source, timeStamp, ex)

        End Sub

        ''' <summary>
        ''' Logs a finished step in the behaviour tree.
        ''' </summary>
        Public Sub LogFinished(nodename As String, result As IBehaviourResult, source As String, timeStamp As Single)

            BehaviourLog.LogBehaviourFinish(m_Logger, LogEventType.Finish, nodename, result, source, timeStamp)

        End Sub

        ''' <summary>
        ''' Logs a started step in the behaviour tree.
        ''' </summary>
        Public Sub LogStarted(nodename As String, result As IBehaviourResult, source As String, timeStamp As Single)

            BehaviourLog.LogBehaviourStart(m_Logger, LogEventType.Start, nodename, result, source, timeStamp)

        End Sub

        ''' <summary>
        ''' Logs a value change in the behaviour tree.
        ''' </summary>
        Public Sub LogValueChange(nodename As String, result As IBehaviourResult, source As String, timeStamp As Single, propertyName As String, newValue As Object)

            BehaviourLog.LogBehaviourValueChange(m_Logger, LogEventType.ValueChange, nodename, result, source, timeStamp, propertyName, newValue)

        End Sub

        ''' <summary>
        ''' Parses properties and values defined in the given <see cref="JObject"/> into the StateManager of type TTarget.
        ''' </summary>
        ''' <typeparam name="TTarget">A <see cref="StateManager(Of TValue, TResult)" /> implementation</typeparam>
        ''' <param name="bag">A StateManager of type TTarget</param>
        ''' <param name="jObject">The <see cref="JObject" /> holding the properties and values</param>
        Public MustOverride Sub ReadJson(Of TTarget As {StateManager(Of TValue, TResult), New})(bag As TTarget, jObject As JObject)

        ''' <summary>
        ''' Removes the <see cref="TypedValue"/> with the given key.
        ''' </summary>
        ''' <exception cref="NullReferenceException">No item with the given key exists in the collection</exception>
        ''' <param name="key">The name or key of the <see cref="TypedValue"/></param>
        Public Sub Remove(key As String)
            Dim old As IList(Of TValue) = GetValues()

            m_ItemsLock.EnterWriteLock()
            Try
                If m_Table.ContainsKey(key) Then
                    m_Table.Remove(key)
                Else
                    Throw New NullReferenceException
                End If
            Finally
                m_ItemsLock.ExitWriteLock()
            End Try
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, m_Table.Values.ToList, old)

        End Sub

        ''' <summary>
        ''' Sets the value.
        ''' </summary>
        ''' <param name="nodeName">The name of the node, that changed this property</param>
        ''' <param name="key">The key of the <see cref="TypedValue"/></param>
        ''' <param name="value">The value</param>
        ''' <param name="ts">Value, representing passed time or a frame number</param>
        <DebuggerStepThrough()>
        Public Overridable Sub [Set](nodeName As String, key As String, value As Object, ts As Single)
            Dim val As TValue = m_Table(key)

            If (val.Value Is Nothing) OrElse (Not val.Value.Equals(val)) Then
                val.m_Value = value
                OnPropertyChanged(nodeName, val, ts)
            End If

        End Sub

        ''' <summary>
        ''' Serializes this StateManager in to a JSON string representation.
        ''' </summary>
        ''' <param name="compress">Whether to compress the output Json (using optimal GZip compression)</param>
        Public MustOverride Function ToJson(Optional compress As Boolean = False) As String

#End Region

#Region " Private methods and functions "

        ''' <summary>
        ''' Returns the <see cref="IEnumerator"/>.
        ''' </summary>
        Private Function GetEnumeratorInternal() As IEnumerator Implements IEnumerable.GetEnumerator

            Return GetEnumerator()

        End Function

        ''' <summary>
        ''' Fires the <see cref="CollectionChanged" /> event.
        ''' </summary>
        ''' <param name="action">The <see cref="NotifyCollectionChangedAction"/> that led to the event</param>
        ''' <param name="newItems">The resulting collection</param>
        ''' <param name="oldItems">The collection before the change occurred</param>
        <DebuggerStepThrough()>
        Protected Overridable Sub OnCollectionChanged(action As NotifyCollectionChangedAction, newItems As IList(Of TValue), oldItems As IList(Of TValue))

            If Context Is Nothing Then
                RaiseEvent CollectionChanged(Me, New NotifyCollectionChangedEventArgs(action, newItems, oldItems))
            Else
                Context.Post(Sub()
                                 RaiseEvent CollectionChanged(Me, New NotifyCollectionChangedEventArgs(action, newItems, oldItems))
                             End Sub, Nothing)
            End If

        End Sub

        ''' <summary>
        ''' Fires the <see cref="PropertyChanged" /> event.
        ''' </summary>
        ''' <param name="nodeName">The name of the node that changed the <see cref="TypedValue"/></param>
        ''' <param name="val">The <see cref="TypedValue"/> of type TValue that changed</param>
        ''' <param name="ts">Value, representing passed time or a frame number</param>
        <DebuggerStepThrough()>
        Protected Overridable Sub OnPropertyChanged(nodeName As String, val As TValue, ts As Single)
            Dim e As New ValueBagPropertyChangedEventArgs(nodeName, val.Copy, ts)

            If Context Is Nothing Then
                RaiseEvent PropertyChanged(Me, e)
            Else
                Context.Post(Sub()
                                 RaiseEvent PropertyChanged(Me, e)
                             End Sub, Nothing)
            End If

        End Sub

#End Region

#Region " Construction "

        ''' <summary>
        ''' Creates a new, empty StateManager.
        ''' </summary>
        ''' <param name="src">The owner of this object</param>
        ''' <param name="logger">A <see cref="ILogger(Of BehaviourLogItem)"/> instance</param>
        <DebuggerStepThrough()>
        Friend Sub New(src As String, logger As ILogger)

            m_ItemsLock = New ReaderWriterLockSlim
            m_Logger = logger
            Source = If(String.IsNullOrEmpty(src), Guid.NewGuid.ToString, src)
            m_Table = New HashTable(Of String, TValue)

        End Sub

        ''' <summary>
        ''' Creates a new, empty StateManager, using the given <see cref="SynchronizationContext"/>.
        ''' </summary>
        ''' <param name="src">The owner of this object</param>
        ''' <param name="logger">A <see cref="ILogger(Of BehaviourLogItem)"/> instance</param>
        ''' <param name="context">The <see cref="SynchronizationContext"/> to use</param>
        <DebuggerStepThrough()>
        Friend Sub New(src As String, logger As ILogger, context As SynchronizationContext)

            Me.New(src, logger)
            Me.Context = context

        End Sub

#End Region

    End Class

    ''' <summary>
    ''' Default <see cref="StateManager(Of TypedValue, TResult)" /> implementation.
    ''' </summary>
    Public Class DefaultStateManager
        Inherits StateManager(Of TypedValue, BehaviourTreeResult)

#Region " Objects and variables "

        Private Const _DEFLOGGER As String = "Debug"

#End Region

#Region " Public methods and functions "

        ''' <summary>
        ''' Creates a DefaultStateManager from the given JSON.
        ''' </summary>
        ''' <param name="json">The input JSON</param>
        ''' <param name="isCompressed">The input is compressed (i.e. using optimal GZip compression!)</param>
        ''' <param name="context">The <see cref="SynchronizationContext"/> to be used by the StateManager</param>
        ''' <returns>An intance of a DefaultStateManager</returns>
        Public Overloads Shared Function FromJson(json As String, Optional isCompressed As Boolean = False, Optional context As SynchronizationContext = Nothing) As DefaultStateManager

            Return FromJson(Of DefaultStateManager)(json, isCompressed, context)

        End Function

        ''' <summary>
        ''' Parses properties and values defined in the given <see cref="JObject"/> into the DefaultStateManager.
        ''' </summary>
        ''' <typeparam name="TTarget">A DefaultStateManager</typeparam>
        ''' <param name="bag">A StateManager of type TTarget</param>
        ''' <param name="jObject">The <see cref="JObject" /> holding the properties and values</param>
        Public Overrides Sub ReadJson(Of TTarget As {StateManager(Of TypedValue, BehaviourTreeResult), New})(bag As TTarget, jObject As JObject)

            Source = jObject(_S).Value(Of String)
            For Each v As JToken In jObject.Last.Values
                bag.Add(New TypedValue With {.m_Key = v.First.First.Value(Of String), .m_Value = v.First.Next.Last.ToObject(Of Object)})
            Next

        End Sub

        ''' <summary>
        ''' Serializes this StateManager in to a JSON string representation.
        ''' </summary>
        ''' <param name="compress">Whether to compress the output Json (using optimal GZip compression)</param>
        Public Overrides Function ToJson(Optional compress As Boolean = False) As String
            Dim serializer As New JsonSerializer With {.Formatting = Formatting.None}
            Dim obj As New With {.s = Source, .v = Me}

            Using writer As New StringWriter()
                Using writerJson As New JsonTextWriter(writer)
                    Try
                        serializer.Serialize(writerJson, obj)
                        Return writer.GetStringBuilder.ToString()
                    Catch ex As Exception
                        Return ex.Message
                    End Try
                End Using
            End Using

        End Function

#End Region

#Region " Construction "

        ''' <summary>
        ''' Creates a new, empty DefaultStateManager, using a debug logger.
        ''' </summary>
        Public Sub New()

            MyBase.New(String.Empty, New LoggerFactory({New DebugLoggerProvider}).CreateLogger(_DEFLOGGER))

        End Sub

        ''' <summary>
        ''' Creates a new, empty DefaultStateManager, using a debug logger.
        ''' </summary>
        ''' <param name="src">The owner of this object</param>
        Public Sub New(src As String)

            MyBase.New(src, New LoggerFactory({New DebugLoggerProvider}).CreateLogger(_DEFLOGGER))

        End Sub

        ''' <summary>
        ''' Creates a new, empty DefaultStateManager, using the given <see cref="SynchronizationContext"/> and a debug logger.
        ''' </summary>
        ''' <param name="context">The <see cref="SynchronizationContext"/> to use</param>
        Public Sub New(context As SynchronizationContext)

            MyBase.New(String.Empty, New LoggerFactory({New DebugLoggerProvider}).CreateLogger(_DEFLOGGER), context)

        End Sub

        ''' <summary>
        ''' Creates a new, empty DefaultStateManager, using the given <see cref="SynchronizationContext"/> and a debug logger.
        ''' </summary>
        ''' <param name="src">The owner of this object</param>
        ''' <param name="context">The <see cref="SynchronizationContext"/> to use</param>
        Public Sub New(src As String, context As SynchronizationContext)

            MyBase.New(src, New LoggerFactory({New DebugLoggerProvider}).CreateLogger(_DEFLOGGER), context)

        End Sub

        ''' <summary>
        ''' Creates a new, empty DefaultStateManager.
        ''' </summary>
        ''' <param name="src">The owner of this object</param>
        ''' <param name="logger">An <see cref="ILogger"/> instance</param>
        Public Sub New(src As String, logger As ILogger)

            MyBase.New(src, logger)

        End Sub

        ''' <summary>
        ''' Creates a new, empty DefaultStateManager, using the given <see cref="SynchronizationContext"/>.
        ''' </summary>
        ''' <param name="src">The owner of this object</param>
        ''' <param name="logger">A <see cref="ILogger"/> instance</param>
        ''' <param name="context">The <see cref="SynchronizationContext"/> to use</param>
        Public Sub New(src As String, logger As ILogger, context As SynchronizationContext)

            MyBase.New(src, logger, context)

        End Sub

#End Region

    End Class

    ''' <summary>
    ''' A value of a certain type, with a key by which to identify it.
    ''' Implements <see cref="INotifyPropertyChanged"/> to enable montoring.
    ''' </summary>
    <DebuggerDisplay("{Key}: {Value}")>
    Public Class TypedValue

#Region " Objects and variables "

        Private Const fmt_ToString As String = "{0}: {1}"

        Friend m_Key As String
        Friend m_Value As Object

#End Region

#Region " Properties "

        ''' <summary>
        ''' The name or key of this value.
        ''' </summary>
        <JsonProperty(PropertyName:="k")>
        Public ReadOnly Property Key As String
            Get
                Return m_Key
            End Get
        End Property

        ''' <summary>
        ''' The value of the object.
        ''' </summary>
        <JsonProperty(PropertyName:="v")>
        Public ReadOnly Property Value As Object
            Get
                Return m_Value
            End Get
        End Property

#End Region

#Region " Public methods and functions "

        ''' <summary>
        ''' Returns a deep copy of this object.
        ''' </summary>
        ''' <returns>A new <see cref="TypedValue"/> </returns>
        <DebuggerStepThrough()>
        Public Overridable Function Copy() As TypedValue

            Return New TypedValue With {.m_Key = m_Key, .m_Value = m_Value}

        End Function

        ''' <summary>
        ''' Debug-friendly representation of this object.
        ''' </summary>
        <DebuggerStepThrough()>
        Public Overrides Function ToString() As String

            Return String.Format(fmt_ToString, m_Key, m_Value)

        End Function

#End Region

#Region " Construction "

        ''' <summary>
        ''' Needed for serialization.
        ''' </summary>
        <DebuggerStepThrough()>
        Public Sub New()
        End Sub

        ''' <summary>
        ''' Creates a new TypedValue object.
        ''' </summary>
        ''' <param name="name">Name of the Value</param>
        ''' <param name="value">The actual value</param>
        ''' <exception cref="NullReferenceException">No name or value was supplied (i.e. Null / Nothing)</exception>
        <DebuggerStepThrough()>
        Public Sub New(name As String, value As Object)

            m_Key = name
            m_Value = value

        End Sub

#End Region

    End Class

End Namespace