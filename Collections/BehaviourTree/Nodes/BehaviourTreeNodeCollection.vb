Imports System.Collections.ObjectModel

Namespace Behaviour

    ''' <summary>
    ''' An immutable collection of BehaviourTreeNodes, extended to enable finding nodes.
    ''' </summary>
    Public Class BehaviourTreeNodeCollection(Of TBag As StateManager(Of TValue, TResult), TValue As {TypedValue, New}, TResult As {IBehaviourResult, New})
        Inherits ReadOnlyCollection(Of IBehaviourTreeNode(Of TBag, TValue, TResult))

#Region " Public methods and functions "

        ''' <summary>
        ''' Returns the first node with the given name.
        ''' </summary>
        ''' <param name="nodeName">The name of the node</param>
        ''' <returns>A <see cref="IBehaviourTreeNode" /> if it exists, else Null / Nothing</returns>
        Public Function Find(nodeName As String) As IBehaviourTreeNode(Of TBag, TValue, TResult)
            Dim rst As IBehaviourTreeNode(Of TBag, TValue, TResult) = FirstOrDefault(Function(t) t.Name = nodeName)

            If rst Is Nothing Then
                For Each n As IParentBehaviourTreeNode(Of TBag, TValue, TResult) In OfType(Of IParentBehaviourTreeNode(Of TBag, TValue, TResult))
                    rst = n.Children.Find(nodeName)
                    If Not rst Is Nothing Then
                        Exit For
                    End If
                Next
            End If

            Return rst

        End Function

#End Region

#Region " Construction "

        ''' <summary>
        ''' C5reates a new BehaviourTreeNodeCollection.
        ''' </summary>
        ''' <param name="list">A <see cref="IList(Of IBehaviourTreeNode(Of TBag, TValue, TResult))" /></param>
        Public Sub New(list As IList(Of IBehaviourTreeNode(Of TBag, TValue, TResult)))

            MyBase.New(list)

        End Sub

#End Region

    End Class

End Namespace