Imports Microsoft.Extensions.Logging

Namespace Behaviour

    ''' <summary>
    ''' Container for a log message from the behaviour tree.
    ''' </summary>
    Public Structure BehaviourLogItem

        ''' <summary>
        ''' The name of the category of the <see cref="ILogger" /> that generated the message.
        ''' </summary>
        Public CategoryName As String

        ''' <summary>
        ''' Date and time this item was saved to a data store.
        ''' </summary>
        Public CreationDate As Date

        ''' <summary>
        ''' The <see cref="EventId" />
        ''' </summary>
        Public EventId As EventId

        ''' <summary>
        ''' Any <see cref="Exception" />, that may have occurred.
        ''' </summary>
        Public Exception As Exception

        ''' <summary>
        ''' Unique ID of this item, áfter it has been saved to a data store.
        ''' </summary>
        Public ID As Double

        ''' <summary>
        ''' The <see cref="LogLevel" /> of the message.
        ''' </summary>
        Public LogLevel As LogLevel

        ''' <summary>
        ''' Collection of named values to log, like source, action name, action result and StateManager property changes.
        ''' </summary>
        Public State As IEnumerable(Of KeyValuePair(Of String, Object))

    End Structure

End Namespace