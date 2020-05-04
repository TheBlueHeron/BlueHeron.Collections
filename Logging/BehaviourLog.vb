Imports Microsoft.Extensions.Logging

Namespace Behaviour

    ''' <summary>
    ''' Cache of logging actions.
    ''' </summary>
    ''' <seealso> https://www.stevejgordon.co.uk/high-performance-logging-in-net-core </seealso>
    Friend Class BehaviourLog

#Region " Objects and variables "

        Private Const fmt_Message As String = "{LogEventType} | {NodeName} | {Result} | {Source} | {TimeStamp}"
        Private Const fmt_MessageWithValue As String = fmt_Message & " | {Value}"
        Private Const fmt_Value As String = "{0}={1}"

#End Region

#Region " Properties "

        ''' <summary>
        ''' Defines a <see cref="LoggerMessage" /> for start events.
        ''' </summary>
        Private Shared ReadOnly m_LogStartMessage As Action(Of ILogger, LogEventType, String, IBehaviourResult, String, Single, Exception) = LoggerMessage.Define(Of LogEventType, String, IBehaviourResult, String, Single)(LogLevel.Information, EventIdCache.Started, fmt_Message)

        ''' <summary>
        ''' Defines a <see cref="LoggerMessage" /> for finish events.
        ''' </summary>
        Private Shared ReadOnly m_LogFinishMessage As Action(Of ILogger, LogEventType, String, IBehaviourResult, String, Single, Exception) = LoggerMessage.Define(Of LogEventType, String, IBehaviourResult, String, Single)(LogLevel.Information, EventIdCache.Finished, fmt_Message)

        ''' <summary>
        ''' Defines a <see cref="LoggerMessage" /> for fault or error events.
        ''' </summary>
        Private Shared ReadOnly m_LogFaultMessage As Action(Of ILogger, LogEventType, String, IBehaviourResult, String, Single, Exception) = LoggerMessage.Define(Of LogEventType, String, IBehaviourResult, String, Single)(LogLevel.Error, EventIdCache.Faulted, fmt_Message)

        ''' <summary>
        ''' Defines a <see cref="LoggerMessage" /> for value change events.
        ''' </summary>
        Private Shared ReadOnly m_LogValueMessage As Action(Of ILogger, LogEventType, String, IBehaviourResult, String, Single, String, Exception) = LoggerMessage.Define(Of LogEventType, String, IBehaviourResult, String, Single, String)(LogLevel.Trace, EventIdCache.ValueChanged, fmt_MessageWithValue)

#End Region

#Region " Methods "

        ''' <summary>
        ''' Adds a start event to the log.
        ''' </summary>
        ''' <param name="logger">The <see cref="ILogger" /> to use</param>
        ''' <param name="level">The <see cref="LogEventType"/></param>
        ''' <param name="nodename">The name of the node in which this event occurred</param>
        ''' <param name="result">The <see cref="IBehaviourResult"/> of this operation</param>
        ''' <param name="source">The source object of the behaviour tree</param>
        ''' <param name="timeStamp">A single value, representing passed time or a frame number</param>
        Public Shared Sub LogBehaviourStart(logger As ILogger, level As LogEventType, nodename As String, result As IBehaviourResult, source As String, timeStamp As Single)

            m_LogStartMessage(logger, level, nodename, result, source, timeStamp, Nothing)

        End Sub

        ''' <summary>
        ''' Adds a finish event to the log.
        ''' </summary>
        ''' <param name="logger">The <see cref="ILogger" /> to use</param>
        ''' <param name="level">The <see cref="LogEventType"/></param>
        ''' <param name="nodename">The name of the node in which this event occurred</param>
        ''' <param name="result">The <see cref="IBehaviourResult"/> of this operation</param>
        ''' <param name="source">The source object of the behaviour tree</param>
        ''' <param name="timeStamp">A single value, representing passed time or a frame number</param>
        Public Shared Sub LogBehaviourFinish(logger As ILogger, level As LogEventType, nodename As String, result As IBehaviourResult, source As String, timeStamp As Single)

            m_LogFinishMessage(logger, level, nodename, result, source, timeStamp, Nothing)

        End Sub

        ''' <summary>
        ''' Adds a fault event to the log.
        ''' </summary>
        ''' <param name="logger">The <see cref="ILogger" /> to use</param>
        ''' <param name="level">The <see cref="LogEventType"/></param>
        ''' <param name="nodename">The name of the node in which this event occurred</param>
        ''' <param name="result">The <see cref="IBehaviourResult"/> of this operation</param>
        ''' <param name="source">The source object of the behaviour tree</param>
        ''' <param name="timeStamp">A single value, representing passed time or a frame number</param>
        ''' <param name="ex">The exception that occurred</param>
        Public Shared Sub LogBehaviourFaulted(logger As ILogger, level As LogEventType, nodename As String, result As IBehaviourResult, source As String, timeStamp As Single, ex As Exception)

            m_LogFaultMessage(logger, level, nodename, result, source, timeStamp, ex)

        End Sub

        ''' <summary>
        ''' Adds a value change event to the log.
        ''' </summary>
        ''' <param name="logger">The <see cref="ILogger" /> to use</param>
        ''' <param name="level">The <see cref="LogEventType"/></param>
        ''' <param name="nodename">The name of the node in which this event occurred</param>
        ''' <param name="result">The <see cref="IBehaviourResult"/> of this operation</param>
        ''' <param name="source">The source object of the behaviour tree</param>
        ''' <param name="timeStamp">A single value, representing passed time or a frame number</param>
        ''' <param name="propertyName">Name of the property that was changed</param>
        ''' <param name="value">The new value of the property</param>
        Public Shared Sub LogBehaviourValueChange(logger As ILogger, level As LogEventType, nodename As String, result As IBehaviourResult, source As String, timeStamp As Single, propertyName As String, value As Object)

            m_LogValueMessage(logger, level, nodename, result, source, timeStamp, String.Format(fmt_Value, propertyName, value), Nothing)

        End Sub

#End Region

    End Class

End Namespace