
Namespace Generic

	''' <summary>
	''' Object that stores a binary tree in an array, allowing for fast and memory-efficient sorting of items.
	''' A custom Key selector function and item comparer can be used to enable many different ways to evaluate the stored items.
	''' </summary>
	''' <typeparam name="TSource">The type of the object that must be stored.</typeparam>
	''' <typeparam name="TKey">The value by which to sort the stored items of type TSource</typeparam>
	''' <remarks><seealso>http://en.wikipedia.org/wiki/Binary_heap</seealso></remarks>
	Public Class BinaryHeap(Of TSource, TKey)

#Region " Objects and variables "

		Private m_a As TSource()
		Private m_Size As Integer
		Private m_KeySelector As Func(Of TSource, TKey)
		Private m_Comparer As IComparer(Of TKey)
		Private m_HeapTypePredicate As Func(Of Boolean, Boolean)

#End Region

#Region " Properties "

		Public ReadOnly Property Array As TSource()
			Get
				Return m_a
			End Get
		End Property

		Public ReadOnly Property Capacity As Integer
			Get
				Return m_a.Length
			End Get
		End Property

		Public ReadOnly Property Size As Integer
			Get
				Return m_Size
			End Get
		End Property

#End Region

#Region " Public methods and functions "

		Public Function Delete() As TSource
			Dim ret As TSource = Peek()

			m_Size -= 1
			If m_Size > 1 Then
				m_a(0) = m_a(m_Size)
				ShiftDown(m_a, 0, m_Size - 1, m_KeySelector, m_Comparer, m_HeapTypePredicate)
			End If

			Return ret

		End Function

		Public Shared Sub Heapify(a As TSource(), size As Integer, keySelector As Func(Of TSource, TKey), predicate As Func(Of Boolean, Boolean))

			Heapify(a, size, keySelector, Comparer(Of TKey).Default, predicate)

		End Sub

		Public Shared Sub Heapify(a As TSource(), size As Integer, keySelector As Func(Of TSource, TKey), comparer As IComparer(Of TKey), predicate As Func(Of Boolean, Boolean))
			Dim start As Integer = CInt((size - 2) / 2)	'Start with the last parent node

			Do While start >= 0
				ShiftDown(a, start, size - 1, keySelector, comparer, predicate)
				start -= 1
			Loop

		End Sub

		Public Shared Sub HeapSort(a As TSource(), size As Integer, keySelector As Func(Of TSource, TKey), ascending As Boolean)

			HeapSort(a, size, keySelector, Comparer(Of TKey).Default, ascending)

		End Sub

		Public Shared Sub HeapSort(a As TSource(), size As Integer, keySelector As Func(Of TSource, TKey), comparer As IComparer(Of TKey), ascending As Boolean)
			Dim predicate As Func(Of Boolean, Boolean)

			If ascending Then
				predicate = Function(b) Not b
			Else
				predicate = Function(b) b
			End If

			Heapify(a, size, keySelector, comparer, predicate)
			SortHeapified(a, size, keySelector, comparer, predicate)

		End Sub

		Public Sub Insert(newItem As TSource)

			If m_Size = m_a.Length Then
				Throw New InvalidOperationException("Heap is full.")
			End If

			m_a(m_Size) = newItem

			If m_Size > 0 Then
				ShiftUp(m_a, 0, m_Size, m_KeySelector, m_Comparer, m_HeapTypePredicate)
			End If

			m_Size += 1

		End Sub

		Public Function Peek() As TSource

			If m_Size = 0 Then
				Throw New InvalidOperationException("Heap is empty.")
			End If

			Return m_a(0)

		End Function

		''' <summary>
		''' Sort a heapified array.
		''' </summary>
		''' <param name="a"></param>
		''' <param name="size"></param>
		''' <param name="predicate"></param>
		Public Shared Sub SortHeapified(a As TSource(), size As Integer, keySelector As Func(Of TSource, TKey), predicate As Func(Of Boolean, Boolean))

			SortHeapified(a, size, keySelector, Comparer(Of TKey).Default, predicate)

		End Sub

		''' <summary>
		''' Sort a heapified array.
		''' </summary>
		''' <param name="a"></param>
		''' <param name="size"></param>
		''' <param name="comparer"></param>
		''' <param name="predicate"></param>
		Public Shared Sub SortHeapified(a As TSource(), size As Integer, keySelector As Func(Of TSource, TKey), comparer As IComparer(Of TKey), predicate As Func(Of Boolean, Boolean))
			Dim ending As Integer = size - 1

			Do While (ending > 0)
				Swap(a, ending, 0)
				ending -= 1
				ShiftDown(a, 0, ending, keySelector, comparer, predicate)
			Loop

		End Sub

#End Region

#Region " Private methods and functions "

		Private Sub SetHeapType(heapType As HeapType)

			If heapType = heapType.MaxHeap Then
				m_HeapTypePredicate = Function(b) Not b
			Else
				m_HeapTypePredicate = Function(b) b
			End If

		End Sub

		Private Shared Sub ShiftDown(a As TSource(), start As Integer, ending As Integer, keySelector As Func(Of TSource, TKey), comparer As IComparer(Of TKey), predicate As Func(Of Boolean, Boolean))
			Dim root As Integer = start

			Do While root * 2 + 1 <= ending	' while the root has at least one child
				Dim child As Integer = root * 2 + 1	' left child
				Dim swapped As Integer = root

				If predicate(comparer.Compare(keySelector(a(swapped)), keySelector(a(child))) > 0) Then
					swapped = child
				End If
				If (child + 1 <= ending) AndAlso predicate(comparer.Compare(keySelector(a(swapped)), keySelector(a(child + 1))) > 0) Then ' right child
					swapped = child + 1
				End If
				If swapped <> root Then
					Swap(a, root, swapped)
					root = swapped ' repeat to continue shifting down the child now
				Else
					Return
				End If
			Loop

		End Sub

		Private Shared Sub ShiftUp(a As TSource(), start As Integer, ending As Integer, keySelector As Func(Of TSource, TKey), comparer As IComparer(Of TKey), predicate As Func(Of Boolean, Boolean))
			Dim child As Integer = ending

			Do While child > start
				Dim parent As Integer = CInt((child - 1) / 2)

				If predicate(comparer.Compare(keySelector(a(parent)), keySelector(a(child))) > 0) Then
					Swap(a, parent, child)
					child = parent
				Else
					Return
				End If
			Loop

		End Sub

		Private Shared Sub Swap(a As TSource(), index1 As Integer, index2 As Integer)
			Dim swapped As TSource = a(index1)

			a(index1) = a(index2)
			a(index2) = swapped

		End Sub

#End Region

#Region " Construction "

		''' <summary>
		''' Construct a binary heap by 'heapifying' an array.
		''' </summary>
		''' <param name="a">The array to heapify.</param>
		''' <param name="heapType">Indicates whether it is a max-heap or min-heap.</param>
		Public Sub New(a As TSource(), heapType As HeapType, keySelector As Func(Of TSource, TKey))

			Me.New(a, heapType, keySelector, Comparer(Of TKey).Default)

		End Sub

		''' <summary>
		''' Construct a binary heap by 'heapifying' an array.
		''' </summary>
		''' <param name="a">The array to heapify.</param>
		''' <param name="heapType">Indicates whether it is a max-heap or min-heap.</param>
		''' <param name="comparer">An ICompare</param>
		Public Sub New(a As TSource(), heapType As HeapType, keySelector As Func(Of TSource, TKey), comparer As IComparer(Of TKey))

			m_a = a
			m_Size = a.Length
			m_KeySelector = keySelector
			m_Comparer = comparer
			SetHeapType(heapType)

			Heapify(a, m_Size, keySelector, m_Comparer, m_HeapTypePredicate)

		End Sub

		''' <summary>
		''' Construct an empty heap.
		''' </summary>
		''' <param name="capacity">Capacity of the Heap</param>
		''' <param name="heapType">Indicates whether it is a max-heap or min-heap.</param>
		Public Sub New(capacity As Integer, heapType As HeapType, keySelector As Func(Of TSource, TKey))

			Me.New(capacity, heapType, keySelector, Comparer(Of TKey).Default)

		End Sub

		''' <summary>
		''' Construct an empty heap.
		''' </summary>
		''' <param name="capacity">Capacity of the Heap</param>
		''' <param name="heapType">Indicates whether it is a max-heap or min-heap.</param>
		''' <param name="comparer">An IComparer</param>
		Public Sub New(capacity As Integer, heapType As HeapType, keySelector As Func(Of TSource, TKey), comparer As IComparer(Of TKey))

			ReDim m_a(capacity - 1)
			m_Size = 0
			m_KeySelector = keySelector
			m_Comparer = comparer
			SetHeapType(heapType)

		End Sub

#End Region

	End Class

End Namespace