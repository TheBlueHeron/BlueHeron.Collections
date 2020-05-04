Imports Microsoft.Extensions.Logging

Namespace Behaviour

    ''' <summary>
    ''' Provides a logger that writes messages to the debug output window.
    ''' </summary>
    Public NotInheritable Class DebugLoggerProvider
        Implements ILoggerProvider

#Region " Objects and variables "

        Private m_Disposed As Boolean

#End Region

#Region " Public methods and functions "

        ''' <summary>
        ''' Creates a new <see cref="DebugLogger" />, which will log messages using the given category.
        ''' </summary>
        ''' <param name="categoryName">The name of the category</param>
        ''' <returns>An <see cref="DebugLogger" /></returns>
        Public Function CreateLogger(categoryName As String) As ILogger Implements ILoggerProvider.CreateLogger

            Return New DebugLogger(Me, categoryName)

        End Function

#End Region

#Region " Private methods and functions "

        ''' <summary>
        ''' Creates a <see cref="BehaviourLogItem" /> and forwards it to the debug output window. Is thread-safe.
        ''' </summary>
        ''' <typeparam name="TState">The type of the object, holding an enumeration of logged parameters as <see cref="KeyValuePair(Of String, Object)" /></typeparam>
        ''' <param name="categoryName">The name of the category</param>
        ''' <param name="logLevel">The <see cref="LogLevel" /> of the message</param>
        ''' <param name="eventId">The <see cref="EventId" /> of the message</param>
        ''' <param name="state">Object of type TState, holding the parameters to log</param>
        ''' <param name="exception">Any <see cref="Exception"/> that may have occurred</param>
        ''' <param name="formatter">Function that outputs the state object as a string</param>
        Private Sub Log(Of TState)(categoryName As String, logLevel As LogLevel, eventId As EventId, state As TState, exception As Exception, formatter As Func(Of TState, Exception, String))

            DebugWriteLine(New BehaviourLogItem With {.CategoryName = categoryName, .EventId = eventId, .LogLevel = logLevel}, state, exception, formatter)

        End Sub

#End Region

#Region " Construction and destruction "

        ''' <summary>
        ''' Creates a new DebugLoggerProvider.
        ''' </summary>
        Public Sub New()
        End Sub

        ''' <summary>
        ''' Cleans up resources.
        ''' </summary>
        Public Sub Dispose() Implements IDisposable.Dispose

            Dispose(True)

        End Sub

#End Region

#Region " IDisposable Support "

        ''' <inheritdoc />
        Protected Sub Dispose(disposing As Boolean)

            If Not m_Disposed Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                End If
            End If
            m_Disposed = True

        End Sub

#End Region

#Region " Classes "

        ''' <summary>
        ''' A logger that writes messages to the debug output window.
        ''' </summary>
        Private NotInheritable Class DebugLogger
            Implements ILogger

#Region " Objects and variables "

            Private ReadOnly m_Provider As DebugLoggerProvider
            Private ReadOnly m_CategoryName As String

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
            ''' Returns True, as it logs all messages (filtering takes place in the behaviour tree, per node by their <see cref="IBehaviourTreeNode(Of TBag, TValue, TResult).Level" /> property.
            ''' </summary>
            ''' <returns>Boolean, True</returns>
            Public Function IsEnabled(logLevel As LogLevel) As Boolean Implements ILogger.IsEnabled

                Return Debugger.IsAttached

            End Function

            ''' <summary>
            ''' Creates a <see cref="BehaviourLogItem" /> and forwards it to the debug output window. Is thread-safe.
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
            ''' Creates a new <see cref="DebugLogger" />, using the given category and <see cref="DebugLoggerProvider" />.
            ''' </summary>
            ''' <param name="provider">The <see cref="DebugLoggerProvider" /> to use</param>
            ''' <param name="categoryName">The name of the category to log under</param>
            Public Sub New(provider As DebugLoggerProvider, categoryName As String)

                m_Provider = provider
                m_CategoryName = categoryName

            End Sub

#End Region

        End Class

#End Region

    End Class

End Namespace