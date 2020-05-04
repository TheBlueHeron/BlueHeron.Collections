Imports System.Text

Namespace Generic

	''' <summary>
	''' 
	''' </summary>
	Public Class PairedSequence(Of TSource, TTarget)

#Region " Objects and variables "

		Private m_Src As HashSet(Of TSource)
		Private m_Tgt As HashSet(Of TTarget)
		Private m_Weight As Double

#End Region

#Region " Properties "

		Public ReadOnly Property Sources As HashSet(Of TSource)
			Get
				Return m_Src
			End Get
		End Property

		Public ReadOnly Property Targets As HashSet(Of TTarget)
			Get
				Return m_Tgt
			End Get
		End Property

		Public ReadOnly Property Weight As Double
			Get
				Return m_Weight
			End Get
		End Property

#End Region

#Region " Public methods and functions "

		''' <summary>
		''' Returns a debug-friendly representation of this object.
		''' </summary>
		''' <returns>String</returns>
		Public Overrides Function ToString() As String
			Dim sb As New StringBuilder

			With sb
				.Append(m_Weight)
				If m_Src.Count > 0 Then
					.Append(ControlChars.NewLine)
					.Append(m_Src(0).ToString)
					If m_Src.Count > 1 Then
						For i As Integer = 1 To m_Src.Count - 1
							.Append(" ")
							.Append(m_Src(i).ToString)
						Next
					End If
				End If
				If m_Tgt.Count > 0 Then
					.Append(ControlChars.NewLine)
					.Append(m_Tgt(0).ToString)
					If m_Tgt.Count > 1 Then
						For i As Integer = 1 To m_Tgt.Count - 1
							.Append(" ")
							.Append(m_Tgt(i).ToString)
						Next
					End If
				End If
			End With

			Return sb.ToString

		End Function

#End Region

#Region " Construction "

		''' <summary>
		''' Creates a new SequencePair.
		''' </summary>
		''' <param name="src"></param>
		''' <param name="tgt"></param>
		''' <param name="weight"></param>
		Public Sub New(src As HashSet(Of TSource), tgt As HashSet(Of TTarget), weight As Double)

			m_Src = src
			m_Tgt = tgt
			m_Weight = weight

		End Sub

#End Region

	End Class

End Namespace