Imports Microsoft.Extensions.Logging

Namespace Behaviour

    ''' <summary>
    ''' Cache of <see cref="EventId" /> structs.
    ''' </summary>
    Friend Class EventIdCache

        ''' <summary>
        ''' EventId of the <see cref="LogEventType.Start"/> event.
        ''' </summary>
        Public Shared ReadOnly Started As New EventId(1, "Started")

        ''' <summary>
        ''' EventId of the <see cref="LogEventType.Finish"/> event.
        ''' </summary>
        Public Shared ReadOnly Finished As New EventId(2, "Finished")

        ''' <summary>
        ''' EventId of the <see cref="LogEventType.ValueChange"/> event.
        ''' </summary>
        Public Shared ReadOnly ValueChanged As New EventId(3, "ValueChanged")

        ''' <summary>
        ''' EventId of the <see cref="LogEventType.Faulted"/> event.
        ''' </summary>
        Public Shared ReadOnly Faulted As New EventId(999, "Faulted")

        ''' <summary>
        ''' Unknown EventId.
        ''' </summary>
        Public Shared ReadOnly Unknown As New EventId(0, "Unknown")

        ''' <summary>
        ''' Returns the <see cref="EventId" />, given its Id.
        ''' If it can not be found, <see cref="Unknown" /> is returned.
        ''' </summary>
        ''' <param name="id">Id of the <see cref="EventId" /></param>
        ''' <returns>An <see cref="EventId" /></returns>
        Public Shared Function GetById(id As Integer) As EventId

            Select Case id
                Case 1
                    Return Started
                Case 2
                    Return Finished
                Case 3
                    Return ValueChanged
                Case 999
                    Return Faulted
            End Select

            Return Unknown

        End Function

    End Class

End Namespace