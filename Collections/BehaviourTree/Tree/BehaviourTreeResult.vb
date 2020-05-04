
Namespace Behaviour

	''' <summary>
	''' Default <see cref="IBehaviourResult" /> implementation.
	''' </summary>
	Public Class BehaviourTreeResult
		Implements IBehaviourResult

		''' <summary>
		''' The result.
		''' </summary>
		Public Property Result As BehaviourTreeStatus Implements IBehaviourResult.Result

		''' <summary>
		''' Sets the properties of this object from its given string representation.
		''' </summary>
		''' <param name="input">String representation of this object</param>
		Public Sub FromString(input As String) Implements IBehaviourResult.FromString

			[Enum].TryParse(input, Result)

		End Sub

		''' <summary>
		''' Returns a string representation of this object.
		''' </summary>
		Public Overrides Function ToString() As String Implements IBehaviourResult.ToString

			Return Result.ToString

		End Function

	End Class

End Namespace