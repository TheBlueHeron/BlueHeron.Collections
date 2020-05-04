Imports System.Runtime.CompilerServices

Namespace Behaviour

	Public Module Extensions

#Region " BehaviourTreeBuilder "

        ''' <summary>
        ''' Starts the build of a <see cref="IBehaviourTree(Of DefaultStateManager, TypedValue, BehaviourTreeResult)" />, using the default root node type.
        ''' Child nodes will inherit the <see cref="LogEventType">Logging level</see> of this node, unless explicitly set.
        ''' </summary>
        ''' <param name="builder">The <see cref="BehaviourTreeBuilder"/> to add a root node to</param>
        ''' <param name="treeName">The name of the (root node of the) behaviour tree</param>
        ''' <returns>The configured <see cref="BehaviourTreeBuilder"/></returns>
        <Extension()>
        Public Function Create(builder As BehaviourTreeBuilder, treeName As String, Optional logLevel As LogEventType = LogEventType.None) As BehaviourTreeBuilder

            builder.SetRootNode(New BehaviourTreeBuilder.DefaultBehaviourTree(treeName) With {.Level = logLevel})
            Return builder

        End Function

#End Region

#Region " LINQ "

        ''' <summary>
        ''' The ForEach Linq extension for <see cref="IEnumerable(Of T)" /> types.
        ''' </summary>
        <Extension()>
        Public Sub ForEach(Of T)(source As IEnumerable(Of T), fn As Action(Of T))

            For Each item As T In source
                fn.Invoke(item)
            Next

        End Sub

        ''' <summary>
        ''' The ForEach Linq extension for <see cref="IEnumerable(Of T)" /> types.
        ''' </summary>
        <Extension()>
        Public Sub ForEach(Of T)(source As IEnumerable(Of T), fn As Action(Of T, Integer))
            Dim index As Integer = 0

            For Each item As T In source
                fn.Invoke(item, index)
                index += 1
            Next

        End Sub

        ''' <summary>
        ''' Convert a variable length argument list of items to an enumerable.
        ''' </summary>
        Public Iterator Function FromItems(Of T)(ParamArray items As T()) As IEnumerable(Of T)

            For Each item As T In items
                Yield item
            Next

        End Function

#End Region

    End Module

End Namespace