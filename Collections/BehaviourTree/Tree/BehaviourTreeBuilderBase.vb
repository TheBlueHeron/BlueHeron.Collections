
Namespace Behaviour

	''' <summary>
	''' Object that produces a behaviour tree, using a Fluent pattern.
	''' </summary>
	''' <seealso>Adapted and extended from source at: https://github.com/ashleydavis/Fluent-Behaviour-Tree </seealso>
	<DebuggerDisplay("{DisplayName}")>
	Public MustInherit Class BehaviourTreeBuilderBase(Of TBag As StateManager(Of TValue, TResult), TValue As {TypedValue, New}, TResult As {IBehaviourResult, New})

#Region " Objects and variables "

		Private m_CurNode As IParentBehaviourTreeNode(Of TBag, TValue, TResult)
		Private m_RootNode As IBehaviourTree(Of TBag, TValue, TResult)
		Private ReadOnly m_ParentNodes As Stack(Of IParentBehaviourTreeNode(Of TBag, TValue, TResult))

#End Region

#Region " Properties "

		''' <summary>
		''' Debug-friendly representation of the contents of this object.
		''' </summary>
		Public ReadOnly Property DisplayName As String
			Get
				Dim name As String = String.Empty

				If (m_CurNode Is Nothing Or m_RootNode Is Nothing) AndAlso (m_ParentNodes.Count = 0) Then
					name = err_TreeEmpty
				Else
					name = GetChildDisplayName(m_RootNode, 0) & vbCrLf
					If m_CurNode Is Nothing Then
						For Each node As IParentBehaviourTreeNode(Of TBag, TValue, TResult) In m_ParentNodes
							name &= GetParentDisplayName(node, 0)
						Next
					Else
						name &= GetParentDisplayName(m_CurNode, 0)
					End If
				End If
				Return name.Trim()
			End Get
		End Property

#End Region

#Region " Public methods and fuctions "

		''' <summary>
		''' Returns the actual node tree.
		''' </summary>
		''' <returns>A <see cref="IBehaviourTreeNode"/> implementation</returns>
		Public Overridable Function Build() As IBehaviourTree(Of TBag, TValue, TResult)

			If m_RootNode Is Nothing Then
				Throw New InvalidOperationException(err_RootNodeMissing)
			End If
			If m_CurNode Is Nothing Then
				Throw New InvalidOperationException(err_NoNodes)
			End If
			m_RootNode.AddChild(m_CurNode)

			Return m_RootNode

		End Function

		''' <summary>
		''' Creates a new <see cref="ActionNode" /> and adds it to the parent <see cref="IParentBehaviourTreeNode"/>.
		''' The result of a True / False function is mapped to <see cref="BehaviourTreeStatus.Success"/> and <see cref="BehaviourTreeStatus.Failure"/>.
		''' </summary>
		''' <param name="name">The name of the <see cref="ActionNode" /></param>
		''' <param name="action">The <see cref="Func(Of TimeStamp, Single, Boolean)"/> to perform</param>
		''' <param name="logLevel">Determines which events of this node should be logged</param>
		''' <returns>This <see cref="BehaviourTreeBuilderBase"/></returns>
		Public Overridable Function Conditional(name As String, action As Func(Of TBag, String, Single, Boolean, Boolean), Optional logLevel As LogEventType = LogEventType.None) As BehaviourTreeBuilderBase(Of TBag, TValue, TResult)

			Return [Do](name, Function(t, n, s, l) If(action(t, n, s, l), New TResult With {.Result = BehaviourTreeStatus.Success}, New TResult With {.Result = BehaviourTreeStatus.Failure}), logLevel)

		End Function

		''' <summary>
		''' Creates a new <see cref="WeightedActionNode" /> and adds it to the parent <see cref="IParentBehaviourTreeNode"/>.
		''' The result of a True / False function is mapped to <see cref="BehaviourTreeStatus.Success"/> and <see cref="BehaviourTreeStatus.Failure"/>.
		''' </summary>
		''' <param name="name">The name of the <see cref="ActionNode" /></param>
		''' <param name="weight">The relative weight of the node</param>
		''' <param name="action">The <see cref="Func(Of TimeStamp, Single, Boolean)"/> to perform</param>
		''' <param name="logLevel">Determines which events of this node should be logged</param>
		''' <returns>This <see cref="BehaviourTreeBuilderBase"/></returns>
		Public Overridable Function Conditional(name As String, weight As Integer, action As Func(Of TBag, String, Single, Boolean, Boolean), Optional logLevel As LogEventType = LogEventType.None) As BehaviourTreeBuilderBase(Of TBag, TValue, TResult)

			Return [Do](name, weight, Function(t, n, s, l) If(action(t, n, s, l), New TResult With {.Result = BehaviourTreeStatus.Success}, New TResult With {.Result = BehaviourTreeStatus.Failure}), logLevel)

		End Function

		''' <summary>
		''' Creates a new <see cref="SwitchNode" /> and adds it to the parent  <see cref="IParentBehaviourTreeNode"/>.
		''' If the action returns <see cref="BehaviourTreeStatus.Success" />, then the first child node's Run function is called, else the second child node's Run function is called.
		''' </summary>
		''' <param name="name">The name of the <see cref="ActionNode" /></param>
		''' <param name="action">The <see cref="Func(Of TimeStamp, Single, Boolean, TResult)"/> to perform</param>
		''' <param name="logLevel">Determines which events of this node should be logged</param>
		Public Overridable Function Switch(name As String, action As Func(Of TBag, String, Single, Boolean, TResult), Optional logLevel As LogEventType = LogEventType.None) As BehaviourTreeBuilderBase(Of TBag, TValue, TResult)

			Return AddParentNode(New SwitchNode(name, action) With {.Level = GetLogLevel(logLevel)})

		End Function

		''' <summary>
		''' Creates a new <see cref="WeightedSwitchNode" /> and adds it to the parent  <see cref="IParentBehaviourTreeNode"/>.
		''' If the action returns <see cref="BehaviourTreeStatus.Success" />, then the first child node's Run function is called, else the second child node's Run function is called.
		''' </summary>
		''' <param name="name">The name of the <see cref="ActionNode" /></param>
		''' <param name="weight">The relative weight of the node</param>
		''' <param name="action">The <see cref="Func(Of TimeStamp, Single, Boolean, TResult)"/> to perform</param>
		''' <param name="logLevel">Determines which events of this node should be logged</param>
		Public Overridable Function Switch(name As String, weight As Integer, action As Func(Of TBag, String, Single, Boolean, TResult), Optional logLevel As LogEventType = LogEventType.None) As BehaviourTreeBuilderBase(Of TBag, TValue, TResult)

			Return AddParentNode(New WeightedSwitchNode(name, weight, action) With {.Level = GetLogLevel(logLevel)})

		End Function

		''' <summary>
		''' Creates a new <see cref="ActionNode" /> and adds it to the parent <see cref="IParentBehaviourTreeNode"/>.
		''' </summary>
		''' <param name="name">The name of the <see cref="ActionNode" /></param>
		''' <param name="action">The <see cref="Func(Of ValueBag(Of TypedValue), Single, TResult)"/> to perform</param>
		''' <param name="logLevel">Determines which events of this node should be logged</param>
		''' <returns>This <see cref="BehaviourTreeBuilderBase"/></returns>
		Public Overridable Function [Do](name As String, action As Func(Of TBag, String, Single, Boolean, TResult), Optional logLevel As LogEventType = LogEventType.None) As BehaviourTreeBuilderBase(Of TBag, TValue, TResult)

			Return AddChildNode(New ActionNode(name, action) With {.Level = GetLogLevel(logLevel)})

		End Function

		''' <summary>
		''' Creates a new <see cref="WeightedActionNode" /> and adds it to the parent <see cref="IParentBehaviourTreeNode"/>.
		''' </summary>
		''' <param name="name">The name of the <see cref="ActionNode" /></param>
		''' <param name="weight">The relative weight of the node</param>
		''' <param name="action">The <see cref="Func(Of TBag, String, Single, Boolean, TResult)"/> to perform</param>
		''' <param name="logLevel">Determines which events of this node should be logged</param>
		''' <returns>This <see cref="BehaviourTreeBuilderBase"/></returns>
		Public Overridable Function [Do](name As String, weight As Integer, action As Func(Of TBag, String, Single, Boolean, TResult), Optional logLevel As LogEventType = LogEventType.None) As BehaviourTreeBuilderBase(Of TBag, TValue, TResult)

			Return AddChildNode(New WeightedActionNode(name, weight, action) With {.Level = GetLogLevel(logLevel)})

		End Function

		''' <summary>
		''' Ends the current sequence of adding children.
		''' The parent node that is one level higher in the hierarchy is now active again.
		''' </summary>
		''' <returns>This <see cref="BehaviourTreeBuilderBase"/></returns>
		Public Overridable Function [End]() As BehaviourTreeBuilderBase(Of TBag, TValue, TResult)

			If m_ParentNodes.Count > 0 Then
				m_CurNode = m_ParentNodes.Pop
			End If
			Return Me

		End Function

		''' <summary>
		''' Adds nodes defined in the given Json representation.
		''' </summary>
		''' <param name="actionSource">Instance of the object that holds all action functions</param>
		''' <param name="json">The Json expression that represents a parent node (and any child nodes)</param>
		''' <param name="isCompressed">The input Json is compressed (i.e. using optimal GZip compression!)</param>
		''' <returns>This <see cref="BehaviourTreeBuilderBase"/></returns>
		Public Overridable Function FromJson(json As String, Optional isCompressed As Boolean = False, Optional actionSource As Object = Nothing) As BehaviourTreeBuilderBase(Of TBag, TValue, TResult)

			m_RootNode = Nothing
			SetRootNode(BehaviourTreeBase(Of TBag, TValue, TResult).FromJson(If(isCompressed, Compressor.Decompress(json), json), actionSource))
			m_CurNode = DirectCast(m_RootNode.Children(0), IParentBehaviourTreeNode(Of TBag, TValue, TResult))
			Return Me

		End Function

		''' <summary>
		''' Creates a new <see cref="InverterNode" /> and adds it to the parent <see cref="IParentBehaviourTreeNode"/>.
		''' </summary>
		''' <param name="name">The name of the <see cref="InverterNode" /></param>
		''' <param name="logLevel">Determines which events of this node should be logged</param>
		''' <returns>This <see cref="BehaviourTreeBuilderBase"/></returns>
		Public Overridable Function Invert(name As String, Optional logLevel As LogEventType = LogEventType.None) As BehaviourTreeBuilderBase(Of TBag, TValue, TResult)

			Return AddParentNode(New InverterNode(name) With {.Level = GetLogLevel(logLevel)})

		End Function

		''' <summary>
		''' Creates a new <see cref="WeightedInverterNode" /> and adds it to the parent <see cref="IParentBehaviourTreeNode"/>.
		''' </summary>
		''' <param name="name">The name of the <see cref="WeightedInverterNode" /></param>
		''' <param name="weight">The relative weight of the node</param>
		''' <param name="logLevel">Determines which events of this node should be logged</param>
		''' <returns>This <see cref="BehaviourTreeBuilderBase"/></returns>
		Public Overridable Function Invert(name As String, weight As Integer, Optional logLevel As LogEventType = LogEventType.None) As BehaviourTreeBuilderBase(Of TBag, TValue, TResult)

			Return AddParentNode(New WeightedInverterNode(name, weight) With {.Level = GetLogLevel(logLevel)})

		End Function

		''' <summary>
		''' Creates a new <see cref="ParallelNode" /> and adds it to the parent <see cref="IParentBehaviourTreeNode"/>.
		''' </summary>
		''' <param name="name">The name of the <see cref="ParallelNode" /></param>
		''' <param name="numFailuresRequired">Number of child failures required to terminate with failure</param>
		''' <param name="numSuccessesRequired">Number of child successes required to terminate with success</param>
		''' <param name="logLevel">Determines which events of this node should be logged</param>
		''' <returns>This <see cref="BehaviourTreeBuilderBase"/></returns>
		Public Overridable Function Parallel(name As String, numFailuresRequired As Integer, numSuccessesRequired As Integer, Optional logLevel As LogEventType = LogEventType.None) As BehaviourTreeBuilderBase(Of TBag, TValue, TResult)

			Return AddParentNode(New ParallelNode(name, numFailuresRequired, numSuccessesRequired) With {.Level = GetLogLevel(logLevel)})

		End Function

		''' <summary>
		''' Creates a new <see cref="WeightedParallelNode" /> and adds it to the parent <see cref="IParentBehaviourTreeNode"/>.
		''' </summary>
		''' <param name="name">The name of the <see cref="WeightedParallelNode" /></param>
		''' <param name="weight">The relative weight of the node</param>
		''' <param name="numFailuresRequired">Number of child failures required to terminate with failure</param>
		''' <param name="numSuccessesRequired">Number of child successes required to terminate with success</param>
		''' <param name="logLevel">Determines which events of this node should be logged</param>
		''' <returns>This <see cref="BehaviourTreeBuilderBase"/></returns>
		Public Overridable Function Parallel(name As String, weight As Integer, numFailuresRequired As Integer, numSuccessesRequired As Integer, Optional logLevel As LogEventType = LogEventType.None) As BehaviourTreeBuilderBase(Of TBag, TValue, TResult)

			Return AddParentNode(New WeightedParallelNode(name, weight, numFailuresRequired, numSuccessesRequired) With {.Level = GetLogLevel(logLevel)})

		End Function

		''' <summary>
		''' Creates a new <see cref="RandomSelectorNode" /> and adds it to the parent <see cref="IParentBehaviourTreeNode"/>.
		''' </summary>
		''' <param name="name">The name of the <see cref="RandomSelectorNode" /></param>
		''' <param name="logLevel">Determines which events of this node should be logged</param>
		''' <returns>This <see cref="BehaviourTreeBuilderBase"/></returns>
		Public Overridable Function Random(name As String, Optional logLevel As LogEventType = LogEventType.None) As BehaviourTreeBuilderBase(Of TBag, TValue, TResult)

			Return AddParentNode(New RandomSelectorNode(name) With {.Level = GetLogLevel(logLevel)})

		End Function

		''' <summary>
		''' Creates a new <see cref="WeightedRandomSelectorNode" /> and adds it to the parent <see cref="IParentBehaviourTreeNode"/>.
		''' </summary>
		''' <param name="name">The name of the <see cref="WeightedRandomSelectorNode" /></param>
		''' <param name="weight">The relative weight of the node</param>
		''' <param name="logLevel">Determines which events of this node should be logged</param>
		''' <returns>This <see cref="BehaviourTreeBuilderBase"/></returns>
		Public Overridable Function Random(name As String, weight As Integer, Optional logLevel As LogEventType = LogEventType.None) As BehaviourTreeBuilderBase(Of TBag, TValue, TResult)

			Return AddParentNode(New WeightedRandomSelectorNode(name, weight) With {.Level = GetLogLevel(logLevel)})

		End Function

		''' <summary>
		''' Creates a new <see cref="SelectorNode" /> and adds it to the parent <see cref="IParentBehaviourTreeNode"/>.
		''' </summary>
		''' <param name="name">The name of the <see cref="SelectorNode" /></param>
		''' <param name="logLevel">Determines which events of this node should be logged</param>
		''' <returns>This <see cref="BehaviourTreeBuilderBase"/></returns>
		Public Overridable Function [Select](name As String, Optional logLevel As LogEventType = LogEventType.None) As BehaviourTreeBuilderBase(Of TBag, TValue, TResult)

			Return AddParentNode(New SelectorNode(name) With {.Level = GetLogLevel(logLevel)})

		End Function

		''' <summary>
		''' Creates a new <see cref="WeightedSelectorNode" /> and adds it to the parent <see cref="IParentBehaviourTreeNode"/>.
		''' </summary>
		''' <param name="name">The name of the <see cref="WeightedSelectorNode" /></param>
		''' <param name="weight">The relative weight of the node</param>
		''' <param name="logLevel">Determines which events of this node should be logged</param>
		''' <returns>This <see cref="BehaviourTreeBuilderBase"/></returns>
		Public Overridable Function [Select](name As String, weight As Integer, Optional logLevel As LogEventType = LogEventType.None) As BehaviourTreeBuilderBase(Of TBag, TValue, TResult)

			Return AddParentNode(New WeightedSelectorNode(name, weight) With {.Level = GetLogLevel(logLevel)})

		End Function

		''' <summary>
		''' Creates a new <see cref="SequenceNode" /> and adds it to the parent <see cref="IParentBehaviourTreeNode"/>.
		''' </summary>
		''' <param name="name">The name of the <see cref="SequenceNode" /></param>
		''' <param name="logLevel">Determines which events of this node should be logged</param>
		''' <returns>This <see cref="BehaviourTreeBuilderBase"/></returns>
		Public Overridable Function Sequential(name As String, Optional logLevel As LogEventType = LogEventType.None) As BehaviourTreeBuilderBase(Of TBag, TValue, TResult)

			Return AddParentNode(New SequenceNode(name) With {.Level = GetLogLevel(logLevel)})

		End Function

		''' <summary>
		''' Creates a new <see cref="WeightedSequenceNode" /> and adds it to the parent <see cref="IParentBehaviourTreeNode"/>.
		''' </summary>
		''' <param name="name">The name of the <see cref="WeightedSequenceNode" /></param>
		''' <param name="weight">The relative weight of the node</param>
		''' <param name="logLevel">Determines which events of this node should be logged</param>
		''' <returns>This <see cref="BehaviourTreeBuilderBase"/></returns>
		Public Overridable Function Sequential(name As String, weight As Integer, Optional logLevel As LogEventType = LogEventType.None) As BehaviourTreeBuilderBase(Of TBag, TValue, TResult)

			Return AddParentNode(New WeightedSequenceNode(name, weight) With {.Level = GetLogLevel(logLevel)})

		End Function

		''' <summary>
		''' Sets the root node of the tree.
		''' For internal use. Use one of the Create... extension methods instead.
		''' </summary>
		''' <param name="node">A <see cref="IBehaviourTree(Of TBag, TValue, TResult)"/> implelentation</param>
		Public Sub SetRootNode(node As IBehaviourTree(Of TBag, TValue, TResult))

			If Not m_RootNode Is Nothing Then
				Throw New InvalidOperationException()
			End If
			m_RootNode = node

		End Sub

		''' <summary>
		''' Splices a subtree into the parent tree.
		''' </summary>
		''' <param name="subTree">A <see cref="IBehaviourTreeNode"/> implementation</param>
		''' <returns>This <see cref="BehaviourTreeBuilderBase"/></returns>
		Public Function Splice(subTree As IBehaviourTreeNode(Of TBag, TValue, TResult)) As BehaviourTreeBuilderBase(Of TBag, TValue, TResult)

			If Not subTree Is Nothing Then
				Return AddChildNode(subTree)
			End If
			Return Me

		End Function

		''' <summary>
		''' Returns a Json string representation of the actual node tree.
		''' </summary>
		''' <param name="compress">If true, the output string is compressed (using optimal GZip compression)</param>
		Public Overridable Function ToJson(Optional compress As Boolean = False) As String

			If m_CurNode Is Nothing Then
				Throw New InvalidOperationException(err_NoNodes)
			End If
			Return If(compress, Compressor.Compress(m_RootNode.ToJson), m_RootNode.ToJson)

		End Function

		''' <summary>
		''' Debug-friendly representation of this object.
		''' </summary>
		Public Overrides Function ToString() As String

			Return DisplayName

		End Function

		''' <summary>
		''' Creates a new <see cref="WeightedRandomNode" /> and adds it to the parent <see cref="IParentBehaviourTreeNode"/>.
		''' </summary>
		''' <param name="name">The name of the <see cref="WeightedRandomNode" /></param>
		''' <param name="logLevel">Determines which events of this node should be logged</param>
		''' <returns>This <see cref="BehaviourTreeBuilderBase"/></returns>
		Public Overridable Function WeightedRandom(name As String, Optional logLevel As LogEventType = LogEventType.None) As BehaviourTreeBuilderBase(Of TBag, TValue, TResult)

			Return AddParentNode(New WeightedRandomNode(name) With {.Level = GetLogLevel(logLevel)})

		End Function

#End Region

#Region " Private methods and functions "

		''' <summary>
		''' Adds the given <see cref="IBehaviourTreeNode"/> to the current <see cref="IParentBehaviourTreeNode"/>.
		''' </summary>
		''' <param name="node">The <see cref="IBehaviourTreeNode"/> to add</param>
		''' <returns>This <see cref="BehaviourTreeBuilderBase"/></returns>
		Protected Overridable Function AddChildNode(node As IBehaviourTreeNode(Of TBag, TValue, TResult)) As BehaviourTreeBuilderBase(Of TBag, TValue, TResult)

			If m_ParentNodes.Count = 0 Then
				Throw New InvalidOperationException(err_NoParent)
			End If
			m_ParentNodes.Peek.AddChild(node)
			Return Me

		End Function

		''' <summary>
		''' Adds the given <see cref="IParentBehaviourTreeNode"/> to the current <see cref="IParentBehaviourTreeNode"/>.
		''' </summary>
		''' <param name="node">The <see cref="IParentBehaviourTreeNode"/> to add</param>
		''' <returns>This <see cref="BehaviourTreeBuilderBase"/></returns>
		Protected Overridable Function AddParentNode(node As IParentBehaviourTreeNode(Of TBag, TValue, TResult)) As BehaviourTreeBuilderBase(Of TBag, TValue, TResult)

			If m_ParentNodes.Count > 0 Then
				m_ParentNodes.Peek.AddChild(node)
			End If
			m_ParentNodes.Push(node)
			Return Me

		End Function

		''' <summary>
		''' Returns the node's name formatted for its depth.
		''' </summary>
		''' <param name="node">The <see cref="IBehaviourTreeNode(Of TBag, TValue, TResult)"/></param>
		''' <param name="depth">Depth of the node (0 is top-level)</param>
		Private Function GetChildDisplayName(node As IBehaviourTreeNode(Of TBag, TValue, TResult), depth As Integer) As String

			Return String.Format(fmt_Name, depth, If(depth = 0, String.Empty, GetSpacer(depth)), node.Name, vbCrLf)

		End Function

		''' <summary>
		''' Returns the <see cref="LogEventType" /> to set on the node (only inherit from root node if the level is set to <see cref="LogEventType.None"/>).
		''' </summary>
		''' <param name="logLevel">The <see cref="LogEventType" /> of the node</param>
		''' <returns>The appropriate <see cref="LogEventType" /></returns>
		Private Function GetLogLevel(logLevel As LogEventType) As LogEventType

			If m_RootNode Is Nothing Then
				Throw New InvalidOperationException(err_RootNodeMissing)
			End If
			Return If(logLevel = LogEventType.None, m_RootNode.Level, logLevel)

		End Function

		''' <summary>
		''' Returns the node's name formatted for its depth.
		''' </summary>
		''' <param name="node">The <see cref="IParentBehaviourTreeNode(Of TBag, TValue, TResult)"/></param>
		''' <param name="depth">Depth of the node (0 is top-level)</param>
		Private Function GetParentDisplayName(node As IParentBehaviourTreeNode(Of TBag, TValue, TResult), depth As Integer) As String
			Dim name As String = GetChildDisplayName(node, depth)

			For Each childNode As IBehaviourTreeNode(Of TBag, TValue, TResult) In node.Children
				If TypeOf (childNode) Is IParentBehaviourTreeNode(Of TBag, TValue, TResult) Then
					name &= GetParentDisplayName(DirectCast(childNode, IParentBehaviourTreeNode(Of TBag, TValue, TResult)), depth + 1)
				Else
					name &= GetChildDisplayName(childNode, depth + 1)
				End If
			Next

			Return name

		End Function

		''' <summary>
		''' Helper function to format display names for depth.
		''' </summary>
		''' <param name="depth">Depth of the node (0 is top-level)</param>
		Private Function GetSpacer(depth As Integer) As String
			Dim spacer As String = String.Empty

			For i As Integer = 1 To depth
				spacer &= vbTab
			Next

			Return spacer

		End Function

#End Region

#Region " Construction "

		''' <summary>
		''' Creates a new <see cref="BehaviourTreeBuilderBase" />, that starts from scratch.
		''' </summary>
		<DebuggerStepThrough()>
		Public Sub New()

			m_ParentNodes = New Stack(Of IParentBehaviourTreeNode(Of TBag, TValue, TResult))

		End Sub

		''' <summary>
		''' Creates a new <see cref="BehaviourTreeBuilderBase"/>, starting from the given <see cref="IParentBehaviourTreeNode"/>.
		''' </summary>
		''' <param name="parent">The parent <see cref="IParentBehaviourTreeNode"/> implementation</param>
		<DebuggerStepThrough()>
		Public Sub New(parent As IParentBehaviourTreeNode(Of TBag, TValue, TResult))

			Me.New()
			m_ParentNodes.Push(parent)

		End Sub

#End Region

	End Class

End Namespace