

''' <summary>
''' Commonly used string constants.
''' </summary>
Public Module Constants

#Region " Behaviour "

	Friend Const _A As String = "a" ' action
	Friend Const _C As String = "c" ' children
	Friend Const _L As String = "l" ' loglevel
	Friend Const _O As String = "o" ' object (Me)
	Friend Const _N As String = "n" ' name
	Friend Const _T As String = "t" ' type
	Friend Const _S As String = "s" ' source
	Friend Const _W As String = "w" ' weight

#Region " Format strings "

	Friend Const fmt_Name As String = "{0} {1}[{2}]{3}"

#End Region

#Region " Errors "

	Friend Const err_FromJson As String = "Error creating node of type '{0}' from JSON '{1}': {2}"
	Friend Const err_InvalidFunction As String = "Function does not exist on the given actionSource, is static or private, has an invalid number of parameters or has an invalid return type."
	Friend Const err_NoActionSource As String = "actionSource must be set."
	Friend Const err_NodeCount As String = "There must be exactly two nodes in a Switch node."
	Friend Const err_NodeWeighted As String = "Child node must implement IWeighted."
	Friend Const err_NoNodes As String = "Can't build a tree without nodes."
	Friend Const err_NoParent As String = "Can't add a child node without a parent."
	Friend Const err_RootNodeMissing As String = "Root node must be set. Use one of the Create functions."
	Friend Const err_RootNodeSet As String = "Root node is already set."
	Friend Const err_TreeEmpty As String = "Tree is empty or hasn't been built."

#End Region

#End Region

End Module