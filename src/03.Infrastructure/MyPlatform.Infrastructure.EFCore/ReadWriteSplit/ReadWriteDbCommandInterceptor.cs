using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace MyPlatform.Infrastructure.EFCore.ReadWriteSplit;

/// <summary>
/// Database command interceptor that routes SQL commands to the appropriate database
/// based on the operation type (read vs write).
/// </summary>
/// <remarks>
/// - SELECT statements are routed to replica databases
/// - INSERT, UPDATE, DELETE statements are routed to the master database
/// - Operations within a transaction are routed to the master database
/// </remarks>
public class ReadWriteDbCommandInterceptor : DbCommandInterceptor
{
    private readonly IConnectionStringResolver _resolver;
    private static readonly AsyncLocal<bool> _forceMaster = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadWriteDbCommandInterceptor"/> class.
    /// </summary>
    /// <param name="resolver">The connection string resolver.</param>
    public ReadWriteDbCommandInterceptor(IConnectionStringResolver resolver)
    {
        _resolver = resolver;
    }

    /// <summary>
    /// Gets or sets a value indicating whether to force master database usage for the current async context.
    /// </summary>
    public static bool ForceMaster
    {
        get => _forceMaster.Value;
        set => _forceMaster.Value = value;
    }

    /// <inheritdoc />
    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        RouteCommand(command, eventData);
        return base.ReaderExecuting(command, eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        RouteCommand(command, eventData);
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    /// <inheritdoc />
    public override InterceptionResult<int> NonQueryExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result)
    {
        // Non-query commands (INSERT, UPDATE, DELETE) always go to master
        SetMasterConnection(command);
        return base.NonQueryExecuting(command, eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        // Non-query commands (INSERT, UPDATE, DELETE) always go to master
        SetMasterConnection(command);
        return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
    }

    /// <inheritdoc />
    public override InterceptionResult<object> ScalarExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result)
    {
        RouteCommand(command, eventData);
        return base.ScalarExecuting(command, eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
    {
        RouteCommand(command, eventData);
        return base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
    }

    /// <summary>
    /// Routes the command to the appropriate database based on the SQL command type.
    /// </summary>
    /// <param name="command">The database command.</param>
    /// <param name="eventData">The command event data.</param>
    private void RouteCommand(DbCommand command, CommandEventData eventData)
    {
        // Force master if explicitly requested
        if (ForceMaster)
        {
            SetMasterConnection(command);
            return;
        }

        // If in a transaction, use master
        if (command.Transaction is not null)
        {
            SetMasterConnection(command);
            return;
        }

        // Check if this is a read operation
        if (IsReadOperation(command.CommandText))
        {
            SetReplicaConnection(command);
        }
        else
        {
            SetMasterConnection(command);
        }
    }

    /// <summary>
    /// Determines if the SQL command is a read operation.
    /// </summary>
    /// <param name="commandText">The SQL command text.</param>
    /// <returns>True if the command is a read operation; otherwise, false.</returns>
    private static bool IsReadOperation(string commandText)
    {
        if (string.IsNullOrWhiteSpace(commandText))
        {
            return false;
        }

        var trimmedCommand = commandText.TrimStart().ToUpperInvariant();
        return trimmedCommand.StartsWith("SELECT", StringComparison.Ordinal);
    }

    /// <summary>
    /// Sets the connection to use the master database.
    /// </summary>
    /// <param name="command">The database command.</param>
    /// <remarks>
    /// If the connection is already open, the connection string cannot be changed.
    /// In this case, the command will continue to use the existing connection.
    /// For proper read-write split in all scenarios, ensure connections are obtained
    /// from the correct connection pool before being opened.
    /// </remarks>
    private void SetMasterConnection(DbCommand command)
    {
        var masterConnectionString = _resolver.GetWriteConnectionString();
        if (!string.IsNullOrEmpty(masterConnectionString) &&
            command.Connection is not null &&
            command.Connection.ConnectionString != masterConnectionString)
        {
            if (command.Connection.State == System.Data.ConnectionState.Open)
            {
                // Connection is already open, cannot change connection string.
                // The command will execute on the current connection.
                // This is expected behavior when using EF Core's connection management.
                return;
            }
            command.Connection.ConnectionString = masterConnectionString;
        }
    }

    /// <summary>
    /// Sets the connection to use a replica database.
    /// </summary>
    /// <param name="command">The database command.</param>
    /// <remarks>
    /// If the connection is already open, the connection string cannot be changed.
    /// In this case, the command will continue to use the existing connection.
    /// </remarks>
    private void SetReplicaConnection(DbCommand command)
    {
        if (_resolver.ShouldUseMaster())
        {
            SetMasterConnection(command);
            return;
        }

        var replicaConnectionString = _resolver.GetReadConnectionString();
        if (!string.IsNullOrEmpty(replicaConnectionString) &&
            command.Connection is not null &&
            command.Connection.ConnectionString != replicaConnectionString)
        {
            if (command.Connection.State == System.Data.ConnectionState.Open)
            {
                // Connection is already open, cannot change connection string.
                // The command will execute on the current connection.
                return;
            }
            command.Connection.ConnectionString = replicaConnectionString;
        }
    }
}
