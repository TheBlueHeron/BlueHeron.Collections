Imports System.Text

Namespace Generic

	''' <summary>
	''' 
	''' </summary>
	Public Class LinkedHashSet(Of T)

#Region " Objects and variables "

		Private Const _SPC As String = " "

		Private m_Set As HashSet(Of T)
		Private m_RemainSet As LinkedHashSet(Of T)
		Private m_Size As Integer = -1

#End Region

#Region " Properties "

		''' <summary>
		''' Returns the element at the given index.
		''' </summary>
		''' <param name="index">Integer</param>
		''' <returns>Element of type T</returns>
		Default ReadOnly Property Item(index As Integer) As T
			Get
				Return ElementAt(index)
			End Get
		End Property

		''' <summary>
		''' Returns the total number of elements in the set.
		''' </summary>
		''' <returns>Integer</returns>
		Public ReadOnly Property Size As Integer
			Get
				If m_Size < 0 Then
					If m_RemainSet Is Nothing Then
						m_Size = m_Set.Count
					Else
						m_Size = m_Set.Count + m_RemainSet.Size
					End If
				End If
				Return m_Size
			End Get
		End Property

#End Region

#Region " Public methods and functions "

		''' <summary>
		''' Returns a new set with the given features added to the front of the list.
		''' </summary>
		''' <param name="features"></param>
		''' <returns></returns>
		Public Function Append(features As HashSet(Of T)) As LinkedHashSet(Of T)

			Return New LinkedHashSet(Of T)(features, Me)

		End Function

		''' <summary>
		''' Returns the element at the given index.
		''' </summary>
		''' <param name="index">Integer</param>
		''' <returns>Element of type T</returns>
		Public Function ElementAt(index As Integer) As T

			If (index > Size) OrElse (index < 0) Then
				Throw New IndexOutOfRangeException
			End If
			If index < m_Set.Count - 1 Then
				Return m_Set(index)
			Else
				Return m_RemainSet.ElementAt(index - m_Set.Count)
			End If

		End Function

		''' <summary>
		''' Debug-friendly representation of this object.
		''' </summary>
		''' <returns>String</returns>
		Public Overrides Function ToString() As String
			Dim sb As New StringBuilder

			For i As Integer = 0 To Size - 1
				If i <> 0 Then
					sb.Append(_SPC)
				End If
				sb.Append(ElementAt(i).ToString)
			Next

			Return sb.ToString

		End Function

#End Region

#Region " Construction "

		Public Sub New(st As HashSet(Of T))

			m_Set = st

		End Sub

		Public Sub New(st As HashSet(Of T), remainSet As LinkedHashSet(Of T))

			m_Set = st
			m_RemainSet = remainSet

		End Sub

#End Region

	End Class

End Namespace