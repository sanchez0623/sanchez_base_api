namespace MyPlatform.Infrastructure.EFCore.Sharding;

/// <summary>
/// Interface for sharding route rules that determine how entities are distributed across shards.
/// </summary>
/// <typeparam name="TEntity">The entity type being sharded.</typeparam>
public interface IShardingRouteRule<TEntity> where TEntity : class
{
    /// <summary>
    /// Gets the table name suffix based on the sharding value.
    /// </summary>
    /// <param name="shardingValue">The value of the sharding column.</param>
    /// <returns>The table name suffix.</returns>
    string GetTableSuffix(object shardingValue);

    /// <summary>
    /// Gets the actual table name for the given logical table name and sharding value.
    /// </summary>
    /// <param name="logicalTableName">The logical table name.</param>
    /// <param name="shardingValue">The value of the sharding column.</param>
    /// <returns>The actual physical table name.</returns>
    string GetActualTableName(string logicalTableName, object shardingValue);

    /// <summary>
    /// Gets all possible table names for a time range (used for cross-shard queries).
    /// </summary>
    /// <param name="logicalTableName">The logical table name.</param>
    /// <param name="startTime">The start time of the range (optional).</param>
    /// <param name="endTime">The end time of the range (optional).</param>
    /// <returns>An enumerable of all possible table names.</returns>
    IEnumerable<string> GetAllTableNames(string logicalTableName, DateTime? startTime, DateTime? endTime);
}
