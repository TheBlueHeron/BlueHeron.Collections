Imports System.Data
Imports System.Data.SqlClient
Imports System.Text
Imports Microsoft.Extensions.Logging

Namespace Behaviour

    ''' <summary>
    ''' Provides a logger that writes messages to a Sql Server instance.
    ''' </summary>
    Public NotInheritable Class SqlServerLoggerProvider
        Implements ILoggerProvider

#Region " Objects and variables "

        Private Const _AND As String = " AND "
        Private Const _APOS As String = "'"
        Private Const _GTE As String = ">="
        Private Const _LTE As String = "<="
        Private Const _OR As String = " OR "
        Private Const _US As String = "_"
        Private Const _Q As String = "?"
        Private Const _PROPSPLIT As Char = "="c

        Private Const col_Level As String = "Level"
        Private Const col_NodeName As String = "NodeName"
        Private Const col_Property As String = "PropertyName"
        Private Const col_Result As String = "Result"
        Private Const col_Source As String = "Source"
        Private Const col_TimeStamp As String = "TimeStamp"
        Private Const col_Value As String = "Value"

        Private Const fmt_CategoryClause As String = "Category='{0}'"
        Private Const fmt_LevelClause As String = "{0}({1})"
        Private Const fmt_LevelSubClause As String = "{0}Level={1}"
        Private Const fmt_NodeNameClause As String = "{0}NodeName='{1}'"
        Private Const fmt_PropertyNameClause As String = "{0}PropertyName='{1}'"
        Private Const fmt_ResultClause As String = "{0}Result='{1}'"
        Private Const fmt_TimeStampClause As String = "{0}TimeStamp{1}{2}"
        Private Const fmt_SourceClause As String = "{0}Source='{1}'"
        Private Const fmt_WhereClause As String = " WHERE ({0})"

        Private Const _TBLCREATE As String = "IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[?]') AND type in (N'U')) BEGIN CREATE TABLE [dbo].[?]([ID] [bigint] IDENTITY(1,1) NOT NULL,[Category] [nvarchar](100) NOT NULL,[EventId] [int] NOT NULL,[LogLevel] [int] NOT NULL,[Exception] [nvarchar](max) NOT NULL,[Level] [smallint] NOT NULL,[NodeName] [nvarchar](100) NOT NULL,[Result] [nvarchar](100) NOT NULL,[Source] [nvarchar](100) NOT NULL,[TimeStamp] [float] NOT NULL,[PropertyName] [nvarchar](100) NOT NULL,[Value] [nvarchar](max) NOT NULL,[CreationDate] [datetime] NULL, CONSTRAINT [PK_?] PRIMARY KEY CLUSTERED ([ID] ASC) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY] END"
        Private Const _TBLCREATE_IX_Category As String = "IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[?]') AND name = N'IX_?_Category') CREATE NONCLUSTERED INDEX [IX_?_Category] ON [dbo].[?] ([Category] ASC) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]"
        Private Const _TBLCREATE_IX_EventId As String = "IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[?]') AND name = N'IX_?_EventId') CREATE NONCLUSTERED INDEX [IX_?_EventId] ON [dbo].[?]([EventId] ASC) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]"
        Private Const _TBLCREATE_IX_Level As String = "IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[?]') AND name = N'IX_?_Level') CREATE NONCLUSTERED INDEX [IX_?_Level] ON [dbo].[?]([Level] ASC) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]"
        Private Const _TBLCREATE_IX_NodeName As String = "IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[?]') AND name = N'IX_?_NodeName') CREATE NONCLUSTERED INDEX [IX_?_NodeName] ON [dbo].[?]([NodeName] ASC) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]"
        Private Const _TBLCREATE_IX_PropertyName As String = "IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[?]') AND name = N'IX_?_PropertyName') CREATE NONCLUSTERED INDEX [IX_?_PropertyName] ON [dbo].[?]([PropertyName] ASC) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]"
        Private Const _TBLCREATE_IX_Source As String = "IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[?]') AND name = N'IX_?_Source') CREATE NONCLUSTERED INDEX [IX_?_Source] ON [dbo].[?]([Source] ASC) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]"
        Private Const _TBLCREATE_IX_TimeStamp As String = "IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[?]') AND name = N'IX_?_TimeStamp') CREATE NONCLUSTERED INDEX [IX_?_TimeStamp] ON [dbo].[?]([TimeStamp] ASC) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]"
        Private Const _TBLCREATE_Trigger As String = "IF NOT EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[?_Insert]')) EXEC dbo.sp_executesql @statement = N'CREATE TRIGGER [dbo].[?_Insert] ON [dbo].[?] AFTER INSERT AS BEGIN Update ? Set CreationDate=getdate() Where ID=any (Select ID From inserted) End'"
        Private Const _TBLCREATE_Trigger_Enable As String = "ALTER TABLE [dbo].[?] ENABLE TRIGGER [?_Insert]"

        Private Const qry_GetMessages As String = "SELECT ID,Category,EventId,LogLevel,Exception,Level,NodeName,Result,Source,TimeStamp,PropertyName,Value,CreationDate FROM {0}{1}"
        Private Const qry_InsertMessage As String = "INSERT INTO {0}(Category,EventId,LogLevel,Exception,Level,NodeName,Result,Source,TimeStamp,PropertyName,Value) VALUES ('{1}',{2},{3},'{4}',{5},'{6}','{7}','{8}',{9},'{10}','{11}')"

        Private ReadOnly m_ConnectionString As String
        Private m_Disposed As Boolean
        Private m_InsertCommand As SqlCommand
        Private ReadOnly m_TableName As String

#End Region

#Region " Properties "

        ''' <summary>
        ''' Determines whether this provider will log items for its <see cref="ILogger"/>s.
        ''' </summary>
        Public Property IsEnabled As Boolean

        ''' <summary>
        ''' This pre-built <see cref="SqlCommand" /> is used to perform the insert stored procedure.
        ''' </summary>
        ''' <returns>A <see cref="SqlCommand" /></returns>
        Private ReadOnly Property InsertCommand As SqlCommand
            Get
                If m_InsertCommand Is Nothing Then
                    m_InsertCommand = New SqlCommand With {.CommandType = CommandType.Text, .Connection = New SqlConnection(m_ConnectionString)}

                    m_InsertCommand.Connection.Open()
                End If
                Return m_InsertCommand
            End Get
        End Property

#End Region

#Region " Public methods and functions "

        ''' <summary>
        ''' Creates a new <see cref="SqlServerLogger" />, which will log messages using the given category.
        ''' </summary>
        ''' <param name="categoryName">The name of the category</param>
        ''' <returns>A <see cref="SqlServerLogger" /></returns>
        Public Function CreateLogger(categoryName As String) As ILogger Implements ILoggerProvider.CreateLogger

            Return New SqlServerLogger(Me, categoryName.Replace(_APOS, _US))

        End Function

        ''' <summary>
        ''' Retrieves log messages, based on the given parameters.
        ''' Text filters are case-insensitive.
        ''' </summary>
        ''' <returns>An <see cref="IEnumerable(Of BehaviourLogItem)"/></returns>
        Public Function GetLog(Optional categoryName As String = "", Optional source As String = "", Optional nodeName As String = "", Optional propertyName As String = "", Optional level As LogEventType = LogEventType.All, Optional result As IBehaviourResult = Nothing, Optional fromTimeStamp As Single? = Nothing, Optional toTimeStamp As Single? = Nothing) As IEnumerable(Of BehaviourLogItem)
            Dim lst As New List(Of BehaviourLogItem)
            Dim blHasResults As Boolean
            Dim clauses As New StringBuilder
            Dim whereClause As String = String.Empty

            If Not String.IsNullOrEmpty(categoryName) Then
                clauses.AppendFormat(fmt_CategoryClause, categoryName.Replace(_APOS, _US))
            End If
            If Not String.IsNullOrEmpty(source) Then
                clauses.AppendFormat(fmt_SourceClause, If(clauses.Length = 0, String.Empty, _AND), source.Replace(_APOS, _US))
            End If
            If Not String.IsNullOrEmpty(nodeName) Then
                clauses.AppendFormat(fmt_NodeNameClause, If(clauses.Length = 0, String.Empty, _AND), nodeName.Replace(_APOS, _US))
            End If
            If Not String.IsNullOrEmpty(propertyName) Then
                clauses.AppendFormat(fmt_PropertyNameClause, If(clauses.Length = 0, String.Empty, _AND), propertyName.Replace(_APOS, _US))
            End If
            If level <> LogEventType.All Then
                Dim subClauses As New StringBuilder

                If (level And LogEventType.Faulted) = LogEventType.Faulted Then
                    subClauses.AppendFormat(fmt_LevelSubClause, String.Empty, CInt(LogEventType.Faulted))
                End If
                If (level And LogEventType.Finish) = LogEventType.Finish Then
                    subClauses.AppendFormat(fmt_LevelSubClause, If(subClauses.Length = 0, String.Empty, _OR), CInt(LogEventType.Finish))
                End If
                If (level And LogEventType.Start) = LogEventType.Start Then
                    subClauses.AppendFormat(fmt_LevelSubClause, If(subClauses.Length = 0, String.Empty, _OR), CInt(LogEventType.Start))
                End If
                If (level And LogEventType.ValueChange) = LogEventType.ValueChange Then
                    subClauses.AppendFormat(fmt_LevelSubClause, If(subClauses.Length = 0, String.Empty, _OR), CInt(LogEventType.ValueChange))
                End If
                If (level And LogEventType.None) = LogEventType.None Then
                    subClauses.AppendFormat(fmt_LevelSubClause, If(subClauses.Length = 0, String.Empty, _OR), CInt(LogEventType.None))
                End If
                clauses.AppendFormat(fmt_LevelClause, If(clauses.Length = 0, String.Empty, _AND), subClauses.ToString)
            End If
            If Not result Is Nothing Then
                clauses.AppendFormat(fmt_ResultClause, If(clauses.Length = 0, String.Empty, _AND), result.ToString)
            End If
            If Not fromTimeStamp Is Nothing Then
                clauses.AppendFormat(fmt_TimeStampClause, If(clauses.Length = 0, String.Empty, _AND), _GTE, fromTimeStamp.Value)
            End If
            If Not toTimeStamp Is Nothing Then
                clauses.AppendFormat(fmt_TimeStampClause, If(clauses.Length = 0, String.Empty, _AND), _LTE, toTimeStamp.Value)
            End If

            If clauses.Length > 0 Then
                whereClause = String.Format(fmt_WhereClause, clauses.ToString)
            End If

            Using r As SqlDataReader = ExecuteReader(String.Format(qry_GetMessages, m_TableName, whereClause), False, blHasResults, New SqlConnection(m_ConnectionString))
                If blHasResults Then
                    Do While r.Read
                        Dim errMessage As String = CStr(r(4))
                        Dim state As New List(Of KeyValuePair(Of String, Object)) From {
                            New KeyValuePair(Of String, Object)(col_Level, DirectCast(CInt(r(5)), LogEventType)),
                            New KeyValuePair(Of String, Object)(col_NodeName, CStr(r(6))),
                            New KeyValuePair(Of String, Object)(col_Result, CStr(r(7))),
                            New KeyValuePair(Of String, Object)(col_Source, CStr(r(8))),
                            New KeyValuePair(Of String, Object)(col_TimeStamp, CSng(r(9))),
                            New KeyValuePair(Of String, Object)(col_Property, CStr(r(10))),
                            New KeyValuePair(Of String, Object)(col_Value, CStr(r(11)))
                            }

                        lst.Add(New BehaviourLogItem With {
                            .ID = CInt(r(0)),
                            .CategoryName = CStr(r(1)),
                            .EventId = EventIdCache.GetById(CInt(r(2))),
                            .LogLevel = CType(CInt(r(3)), LogLevel),
                            .Exception = If(String.IsNullOrEmpty(errMessage), Nothing, New Exception(errMessage)),
                            .State = state,
                            .CreationDate = CDate(r(12))
                                })
                    Loop
                End If
            End Using

            Return lst

        End Function

#End Region

#Region " Private methods and functions "

        ''' <summary>
        ''' Creates the log table, with indexes and trigger.
        ''' </summary>
        Private Sub CreateSqlObjects()
            Dim cmd As SqlCommand = Nothing

            Try
                cmd = New SqlCommand(_TBLCREATE.Replace(_Q, m_TableName), New SqlConnection(m_ConnectionString))
                cmd.Connection.Open()
                cmd.ExecuteNonQuery() ' table creation
                cmd.CommandText = _TBLCREATE_IX_Category.Replace(_Q, m_TableName) ' Category index
                cmd.ExecuteNonQuery()
                cmd.CommandText = _TBLCREATE_IX_EventId.Replace(_Q, m_TableName) ' EventId index
                cmd.ExecuteNonQuery()
                cmd.CommandText = _TBLCREATE_IX_Level.Replace(_Q, m_TableName) ' Level index
                cmd.ExecuteNonQuery()
                cmd.CommandText = _TBLCREATE_IX_NodeName.Replace(_Q, m_TableName) ' NodeName index
                cmd.ExecuteNonQuery()
                cmd.CommandText = _TBLCREATE_IX_PropertyName.Replace(_Q, m_TableName) ' PropertyName index
                cmd.ExecuteNonQuery()
                cmd.CommandText = _TBLCREATE_IX_Source.Replace(_Q, m_TableName) ' Source index
                cmd.ExecuteNonQuery()
                cmd.CommandText = _TBLCREATE_IX_TimeStamp.Replace(_Q, m_TableName) ' TimeStamp index
                cmd.ExecuteNonQuery()
                cmd.CommandText = _TBLCREATE_Trigger.Replace(_Q, m_TableName) ' trigger
                cmd.ExecuteNonQuery()
                cmd.CommandText = _TBLCREATE_Trigger_Enable.Replace(_Q, m_TableName) ' trigger enable
                cmd.ExecuteNonQuery()
            Catch ex As Exception
                IsEnabled = False
            Finally
                If Not ((cmd Is Nothing) OrElse (cmd.Connection Is Nothing)) Then
                    cmd.Connection.Close()
                    cmd.Dispose()
                End If
            End Try

        End Sub

        ''' <summary>
        ''' Executes a query and returns a <see cref="SqlDataReader" />, which contains the result(s) of the query.
        ''' </summary>
        ''' <param name="query">The query to execute</param>
        ''' <param name="singleRow">Determines whether a single row will be retrieved</param>
        ''' <param name="hasResult">Signifies whether the query yielded results</param>
        ''' <param name="connection">The <see cref="SqlConnection" /> to use</param>
        ''' <returns></returns>
        Private Function ExecuteReader(query As String, singleRow As Boolean, ByRef hasResult As Boolean, connection As SqlConnection) As SqlDataReader
            Dim reader As SqlDataReader = Nothing
            Dim behavior As CommandBehavior = CommandBehavior.CloseConnection

            If singleRow Then
                behavior = behavior Or CommandBehavior.SingleRow
            End If

            Using cmd As New SqlCommand(query, connection)
                cmd.Connection.Open()
                reader = cmd.ExecuteReader(behavior)
                hasResult = reader.HasRows
            End Using

            Return reader

        End Function

        ''' <summary>
        ''' Returns a <see cref="SqlCommand" /> to insert the given <see cref="BehaviourLogItem" /> into the database.
        ''' </summary>
        ''' <param name="logItem">The <see cref="BehaviourLogItem" /> to insert</param>
        ''' <returns>A <see cref="SqlCommand" /></returns>
        Private Function GetInsertCommand(logItem As BehaviourLogItem) As SqlCommand
            Dim propName As String = String.Empty
            Dim propValue As String = String.Empty

            With logItem
                If logItem.State.Count = 7 Then
                    Dim pair As String() = CStr(logItem.State(5).Value).Split(_PROPSPLIT)
                    propName = pair(0)
                    propValue = pair(1)
                End If
                InsertCommand.CommandText = String.Format(qry_InsertMessage, m_TableName, .CategoryName, .EventId.Id, CInt(.LogLevel), .Exception, CInt(.State(0).Value), CStr(logItem.State(1).Value), DirectCast(logItem.State(2).Value, IBehaviourResult).ToString, CStr(logItem.State(3).Value), CSng(logItem.State(4).Value), propName, propValue)
            End With

            Return InsertCommand

        End Function

        ''' <summary>
        ''' Creates a <see cref="BehaviourLogItem" /> and adds it to the database. Is thread-safe.
        ''' </summary>
        ''' <typeparam name="TState">The type of the object, holding an enumeration of logged parameters as <see cref="KeyValuePair(Of String, Object)" /></typeparam>
        ''' <param name="categoryName">The name of the category</param>
        ''' <param name="logLevel">The <see cref="LogLevel" /> of the message</param>
        ''' <param name="eventId">The <see cref="EventId" /> of the message</param>
        ''' <param name="state">Object of type TState, holding the parameters to log</param>
        ''' <param name="exception">Any <see cref="Exception"/> that may have occurred</param>
        ''' <param name="formatter">Function that outputs the state object as a string</param>
        Private Sub Log(Of TState)(categoryName As String, logLevel As LogLevel, eventId As EventId, state As TState, exception As Exception, formatter As Func(Of TState, Exception, String))

            If IsEnabled Then
                GetInsertCommand(New BehaviourLogItem With {.CategoryName = categoryName, .EventId = eventId, .Exception = exception, .LogLevel = logLevel, .State = DirectCast(state, IEnumerable(Of KeyValuePair(Of String, Object)))}).ExecuteNonQuery()
            End If

        End Sub

#End Region

#Region " Construction and destruction "

        ''' <summary>
        ''' Creates a new SqlServerLoggerProvider.
        ''' </summary>
        ''' <param name="connectionString">The connection to use to connect to the database</param>
        ''' <param name="tableName">The name of the table to insert log messages into</param>
        Public Sub New(connectionString As String, tableName As String, Optional createIfNotExists As Boolean = False)

            m_ConnectionString = connectionString
            m_TableName = tableName.Replace(_APOS, _US)
            If createIfNotExists Then
                CreateSqlObjects()
            End If
            IsEnabled = True

        End Sub

        ''' <summary>
        ''' Cleans up resources.
        ''' </summary>
        Public Sub Dispose() Implements IDisposable.Dispose

            Dispose(True)

        End Sub

#End Region

#Region " IDisposable Support "

        ''' <inheritdoc />
        Protected Sub Dispose(disposing As Boolean)

            If Not m_Disposed Then
                If disposing Then
                    If Not m_InsertCommand Is Nothing Then
                        m_InsertCommand.Connection.Close()
                        m_InsertCommand.Dispose()
                    End If
                End If
            End If
            m_Disposed = True

        End Sub

#End Region

#Region " Classes "

        ''' <summary>
        ''' A logger that writes messages to a a sql server instance.
        ''' </summary>
        Private NotInheritable Class SqlServerLogger
            Implements ILogger

#Region " Objects and variables "

            Private ReadOnly m_Provider As SqlServerLoggerProvider
            Private ReadOnly m_CategoryName As String

#End Region

#Region " Public methods and functions "

            ''' <summary>
            ''' Not implemented.
            ''' </summary>
            ''' <exception cref="NotImplementedException">Not implemented</exception>
            Public Function BeginScope(Of TState)(state As TState) As IDisposable Implements ILogger.BeginScope

                Throw New NotImplementedException()

            End Function

            ''' <summary>
            ''' Retrieves log messages, based on the given parameters
            ''' </summary>
            ''' <returns>An <see cref="IEnumerable(Of BehaviourLogItem)"/></returns>
            Public Function GetLog(Optional source As String = "", Optional nodeName As String = "", Optional propertyName As String = "", Optional level As LogEventType = LogEventType.All, Optional result As IBehaviourResult = Nothing, Optional fromTimeStamp As Single? = Nothing, Optional toTimeStamp As Single? = Nothing) As IEnumerable(Of BehaviourLogItem)

                Return m_Provider.GetLog(m_CategoryName, source, nodeName, propertyName, level, result, fromTimeStamp, toTimeStamp)

            End Function

            ''' <summary>
            ''' Returns True, as it logs all messages (filtering takes place in the behaviour tree, per node by their <see cref="IBehaviourTreeNode(Of TBag, TValue, TResult).Level" /> property.
            ''' </summary>
            ''' <returns>Boolean, True</returns>
            Public Function IsEnabled(logLevel As LogLevel) As Boolean Implements ILogger.IsEnabled

                Return True

            End Function

            ''' <summary>
            ''' Creates a <see cref="BehaviourLogItem" /> and adds it to the database of the <see cref="InMemoryLoggerProvider" />. Is thread-safe.
            ''' </summary>
            ''' <typeparam name="TState">The type of the object, holding an enumeration of logged parameters as <see cref="KeyValuePair(Of String, Object)" /></typeparam>
            ''' <param name="logLevel">The <see cref="LogLevel" /> of the message</param>
            ''' <param name="eventId">The <see cref="EventId" /> of the message</param>
            ''' <param name="state">Object of type TState, holding the parameters to log</param>
            ''' <param name="exception">Any <see cref="Exception"/> that may have occurred</param>
            ''' <param name="formatter">Function that outputs the state object as a string</param>
            Public Sub Log(Of TState)(logLevel As LogLevel, eventId As EventId, state As TState, exception As Exception, formatter As Func(Of TState, Exception, String)) Implements ILogger.Log

                m_Provider.Log(m_CategoryName, logLevel, eventId, state, exception, formatter)

            End Sub

#End Region

#Region " Construction "

            ''' <summary>
            ''' Creates a new <see cref="SqlServerLogger" />, using the given category and <see cref="SqlServerLoggerProvider" />.
            ''' </summary>
            ''' <param name="provider">The <see cref="SqlServerLoggerProvider" /> to use</param>
            ''' <param name="categoryName">The name of the category to log under</param>
            Public Sub New(provider As SqlServerLoggerProvider, categoryName As String)

                m_Provider = provider
                m_CategoryName = categoryName

            End Sub

#End Region

        End Class

#End Region

    End Class

End Namespace