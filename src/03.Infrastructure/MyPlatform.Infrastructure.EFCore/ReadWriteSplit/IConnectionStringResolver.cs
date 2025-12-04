namespace MyPlatform.Infrastructure.EFCore.ReadWriteSplit;

/// <summary>
/// Interface for resolving connection strings for read-write split scenarios.
/// </summary>
public interface IConnectionStringResolver
{
    /// <summary>
    /// Gets the connection string for write operations (master database).
    /// </summary>
    /// <returns>The master database connection string.</returns>
    string GetWriteConnectionString();

    /// <summary>
    /// Gets the connection string for read operations (replica database with load balancing).
    /// </summary>
    /// <returns>The replica database connection string.</returns>
    string GetReadConnectionString();

    /// <summary>
    /// Determines whether the master database should be used for the current operation.
    /// </summary>
    /// <returns>True if master should be used; otherwise, false.</returns>
    bool ShouldUseMaster();
}
