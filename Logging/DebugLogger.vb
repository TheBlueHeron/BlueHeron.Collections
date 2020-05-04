
#Const DEBUG = True

Namespace Behaviour

	Partial Public Class DebugLoggerProvider

		Private Const fmt_Message As String = "{0} | {1} | {2}"

		''' <summary>
		''' Separated to be able to define a DEBUG constant for the debug write function that writes a line in the debug output window and must do so even when running a release build in the programming environment.
		''' </summary>
		''' <typeparam name="TState">The type of the state object</typeparam>
		''' <param name="logItem">The <see cref="BehaviourLogItem"/></param>
		''' <param name="state">State object of type TState</param>
		''' <param name="ex">Any exception that may have occurred</param>
		''' <param name="formatter">Function that formats the state object</param>
		Private Sub DebugWriteLine(Of TState)(logItem As BehaviourLogItem, state As TState, ex As Exception, formatter As Func(Of TState, Exception, String))

			Debug.WriteLine(String.Format(fmt_Message, logItem.CategoryName, logItem.LogLevel, formatter(state, ex)))

		End Sub

	End Class

End Namespace