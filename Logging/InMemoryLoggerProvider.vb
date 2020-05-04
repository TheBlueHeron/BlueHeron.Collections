Imports System.Threading
Imports Microsoft.Extensions.Logging

Namespace Behaviour

    ''' <summary>
    ''' Provides a logger that writes messages to a an in-memory list.
    ''' </summary>
    Public NotInheritable Class InMemoryLoggerProvider
        Implements ILoggerProvider

#Region " Objects and variables "

        Private m_Disposed As Boolean
        Private m_Messages As New List(Of BehaviourLogItem)
        Private ReadOnly m_ItemsLock As ReaderWriterLockSlim ' for thread-safe collection access

#End Region

#Region " Properties "

        ''' <summary>
        ''' Collection of <see cref="BehaviourLogItem" /> messages.
        ''' </summary>
        ''' <returns><see cref="IEnumerable(Of BehaviourLogItem)" /></returns>
        Public ReadOnly Property Messages As IEnumerable(Of BehaviourLogItem)
            Get
                m_ItemsLock.EnterReadLock()
                Try
                    Return m_Messages
                Finally
                    m_ItemsLock.ExitReadLock()
                End Try
            End Get
        End Property

#End Region

#Region " Public methods and functions "

        ''' <summary>
        ''' Creates a new <see cref="InMemoryLogger" />, which will log messages using the given category.
        ''' </summary>
        ''' <param name="categoryName">The name of the category</param>
        ''' <returns>An <see cref="InMemoryLogger" /></returns>
        Public Function CreateLogger(categoryName As String) As ILogger Implements ILoggerProvider.CreateLogger

            Return New InMemoryLogger(Me, categoryName)

        End Function

#End Region

#Region " Private methods and functions "

        ''' <summary>
        ''' Creates a <see cref="BehaviourLogItem" /> and adds it to the <see cref="Messages" /> collection. Is thread-safe.
        ''' </summary>
        ''' <typeparam name="TState">The type of the object, holding an enumeration of logged parameters as <see cref="KeyValuePair(Of String, Object)" /></typeparam>
        ''' <param name="categoryName">The name of the category</param>
        ''' <param name="logLevel">The <see cref="LogLevel" /> of the message</param>
        ''' <param name="eventId">The <see cref="EventId" /> of the message</param>
        ''' <param name="state">Object of type TState, holding the parameters to log</param>
        ''' <param name="exception">Any <see cref="Exception"/> that may have occurred</param>
        ''' <param name="formatter">Function that outputs the state object as a string</param>
        Private Sub Log(Of TState)(categoryName As String, logLevel As LogLevel, eventId As EventId, state As TState, exception As Exception, formatter As Func(Of TState, Exception, String))

            m_ItemsLock.EnterWriteLock()
            m_Messages.Add(New BehaviourLogItem With {.CategoryName = categoryName, .EventId = eventId, .Exception = exception, .LogLevel = logLevel, .State = DirectCast(state, IEnumerable(Of KeyValuePair(Of String, Object)))})
            m_ItemsLock.ExitWriteLock()

        End Sub

#End Region

#Region " Construction and destruction "

        ''' <summary>
        ''' Creates a new InMemoryLoggerProvider.
        ''' </summary>
        Public Sub New()

            m_ItemsLock = New ReaderWriterLockSlim

        End Sub

        ''' <summary>
        ''' Cleans up resources.
        ''' </summary>
        Public Sub Dispose() Implements IDisposable.Dispose

            Dispose(True)

        End Sub

#End Region

#Region " IDisposable Support "

        ''' <summary>
        ''' Cleans up resources.
        ''' </summary>
        ''' <param name="disposing"></param>
        Protected Sub Dispose(disposing As Boolean)

            If Not m_Disposed Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                End If
                m_Messages = Nothing
            End If
            m_Disposed = True

        End Sub

#End Region

#Region " Classes "

        ''' <summary>
        ''' A logger that writes messages to a an in-memory list.
        ''' </summary>
        Private NotInheritable Class InMemoryLogger
            Implements ILogger

#Region " Objects and variables "

            Private ReadOnly m_Provider As InMemoryLoggerProvider
            Private ReadOnly m_CategoryName As String

#End Region

#Region " Properties "

            ''' <summary>
            ''' Collection of <see cref="BehaviourLogItem" /> messages of this <see cref="InMemoryLogger" />.
            ''' </summary>
            ''' <returns>An <see cref="IEnumerable(Of BehaviourLogItem)" /></returns>
            Public ReadOnly Property Messages As IEnumerable(Of BehaviourLogItem)
                Get
                    Return m_Provider.Messages.Where(Function(m) m.CategoryName = m_CategoryName)
                End Get
            End Property

#End Region

#Region " Public methods and functions "

            ''' <summary>
            ''' Not implemented.
            ''' </summary>
            ''' <exception cref="NotImplementedException">Not implemented</exception>
            Public Function BeginScope(Of TState)(state As TState) As IDisposable Implements ILogger.BeginScope

                Throw New NotImplementedException()

            End Function

            ''' <summary>
            ''' Returns True, as it logs all messages (filtering takes place in the behaviour tree, per node by their <see cref="IBehaviourTreeNode(Of TBag, TValue).Level" /> property.
            ''' </summary>
            ''' <returns>Boolean, True</returns>
            Public Function IsEnabled(logLevel As LogLevel) As Boolean Implements ILogger.IsEnabled

                Return True

            End Function

            ''' <summary>
            ''' Creates a <see cref="BehaviourLogItem" /> and adds it to the <see cref="Messages" /> collection of the <see cref="InMemoryLoggerProvider" />. Is thread-safe.
            ''' </summary>
            ''' <typeparam name="TState">The type of the object, holding an enumeration of logged parameters as <see cref="KeyValuePair(Of String, Object)" /></typeparam>
            ''' <param name="logLevel">The <see cref="LogLevel" /> of the message</param>
            ''' <param name="eventId">The <see cref="EventId" /> of the message</param>
            ''' <param name="state">Object of type TState, holding the parameters to log</param>
            ''' <param name="exception">Any <see cref="Exception"/> that may have occurred</param>
            ''' <param name="formatter">Function that outputs the state object as a string</param>
            Public Sub Log(Of TState)(logLevel As LogLevel, eventId As EventId, state As TState, exception As Exception, formatter As Func(Of TState, Exception, String)) Implements ILogger.Log

                m_Provider.Log(m_CategoryName, logLevel, eventId, state, exception, formatter)

            End Sub

#End Region

#Region " Construction "

            ''' <summary>
            ''' Creates a new <see cref="InMemoryLogger" />, using the given category and <see cref="InMemoryLoggerProvider" />.
            ''' </summary>
            ''' <param name="provider">The <see cref="InMemoryLoggerProvider" /> to use</param>
            ''' <param name="categoryName">The name of the category to log under</param>
            Public Sub New(provider As InMemoryLoggerProvider, categoryName As String)

                m_Provider = provider
                m_CategoryName = categoryName

            End Sub

#End Region

        End Class

#End Region

    End Class

End Namespace