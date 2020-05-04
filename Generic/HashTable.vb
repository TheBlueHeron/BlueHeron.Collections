
Namespace Generic

	''' <summary>
	''' Strongly-typed <see cref="Hashtable">Hashtable</see>.
	''' </summary>
	<Serializable>
	Public Class HashTable(Of TKey, TValue)
		Inherits Hashtable

#Region " Properties "

		Default Public Shadows Property Item(key As TKey) As TValue
			Get
				Return DirectCast(MyBase.Item(key), TValue)
			End Get
			Set(value As TValue)
				MyBase.Item(key) = value
			End Set
		End Property

		Public Shadows ReadOnly Property Keys As IEnumerable(Of TKey)
			Get
				Return MyBase.Keys.OfType(Of TKey)()
			End Get
		End Property

		Public Shadows ReadOnly Property Values As IEnumerable(Of TValue)
			Get
				Return MyBase.Values.OfType(Of TValue)()
			End Get
		End Property

#End Region

#Region " Public methods and functions "

		Public Shadows Sub Add(key As TKey, value As TValue)

			MyBase.Add(key, value)

		End Sub

		Public Shadows Function Contains(key As TKey) As Boolean

			Return MyBase.Contains(key)

		End Function

		Public Shadows Function ContainsKey(key As TKey) As Boolean

			Return MyBase.ContainsKey(key)

		End Function

		Public Shadows Function ContainsValue(value As TValue) As Boolean

			Return MyBase.ContainsValue(value)

		End Function

		Public Shadows Sub Remove(key As TKey)

			MyBase.Remove(key)

		End Sub

#End Region

#Region " Construction "

		Public Sub New()

			MyBase.New

		End Sub

		Protected Sub New(serializationInfo As Runtime.Serialization.SerializationInfo, streamingContext As Runtime.Serialization.StreamingContext)

			MyBase.New(serializationInfo, streamingContext)

		End Sub

#End Region

	End Class

End Namespace