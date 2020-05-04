Imports System.Runtime.CompilerServices
Imports Microsoft.Extensions.Logging

Namespace Behaviour

    ''' <summary>
    ''' Extension functions to sweeten the process of adding different logging functionalities to existing frameworks.
    ''' </summary>
    Module LoggerFactoryExtensions

        ''' <summary>
        ''' Adds a new <see cref="DebugLoggerProvider" /> to the given factory.
        ''' </summary>
        ''' <param name="factory">A <see cref="DebugLoggerProvider" /></param>
        ''' <returns>The given <see cref="ILoggerFactory" /></returns>
        <Extension>
        Public Function AddDebug(factory As ILoggerFactory) As ILoggerFactory

            factory.AddProvider(New DebugLoggerProvider)
            Return factory

        End Function

        ''' <summary>
        ''' Adds a new <see cref="InMemoryLoggerProvider" /> to the given factory.
        ''' </summary>
        ''' <param name="factory">A <see cref="InMemoryLoggerProvider" /></param>
        ''' <returns>The given <see cref="ILoggerFactory" /></returns>
        <Extension>
        Public Function AddInMemory(factory As ILoggerFactory) As ILoggerFactory

            factory.AddProvider(New InMemoryLoggerProvider)
            Return factory

        End Function

        ''' <summary>
        ''' Adds a new <see cref="SqlServerLoggerProvider" /> to the given factory.
        ''' </summary>
        ''' <param name="factory">A <see cref="SqlServerLoggerProvider" /></param>
        ''' <param name="connectionString">The connection to use to connect to the database</param>
        ''' <param name="tableName">The name of the table to insert log messages into</param>
        ''' <returns>The given <see cref="ILoggerFactory" /></returns>
        <Extension>
        Public Function AddSqlServer(factory As ILoggerFactory, connectionString As String, tableName As String) As ILoggerFactory

            factory.AddProvider(New SqlServerLoggerProvider(connectionString, tableName))
            Return factory

        End Function

    End Module

End Namespace