
Namespace Behaviour

	''' <summary>
	''' Event arguments for the <see cref="Statemanager(Of T).PropertyChanged" /> event.
	''' </summary>
	Public Class ValueBagPropertyChangedEventArgs
		Inherits EventArgs

#Region " Properties "

		''' <summary>
		''' The name of the node, that changed this property.
		''' </summary>
		Public ReadOnly Property NodeName As String

		''' <summary>
		''' The property that changed.
		''' </summary>
		Public ReadOnly Property [Property] As TypedValue

		''' <summary>
		''' Value, representing passed time.
		''' </summary>
		Public ReadOnly Property TimeStamp As Single

#End Region

#Region " Construction "

		''' <summary>
		''' Creates a new ValueBagPropertyChangedEventArgs object.
		''' </summary>
		''' <param name="name">The name of the node, that changed this property</param>
		''' <param name="prop">The property that changed</param>
		''' <param name="ts">Value, representing passed time or frame number</param>
		Public Sub New(name As String, prop As TypedValue, ts As Single)

			NodeName = name
			[Property] = prop
			TimeStamp = ts

		End Sub

#End Region

	End Class

End Namespace