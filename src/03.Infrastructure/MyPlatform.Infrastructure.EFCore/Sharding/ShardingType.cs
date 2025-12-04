namespace MyPlatform.Infrastructure.EFCore.Sharding;

/// <summary>
/// Types of sharding strategies for table partitioning.
/// </summary>
public enum ShardingType
{
    /// <summary>
    /// Shard by month (e.g., table_202401, table_202402).
    /// </summary>
    ByMonth,

    /// <summary>
    /// Shard by year (e.g., table_2024, table_2025).
    /// </summary>
    ByYear,

    /// <summary>
    /// Shard by day (e.g., table_20240101, table_20240102).
    /// </summary>
    ByDay,

    /// <summary>
    /// Shard by ID modulo (e.g., table_0, table_1, table_2, table_3).
    /// </summary>
    ByMod,

    /// <summary>
    /// Shard by tenant (database per tenant).
    /// </summary>
    ByTenant,

    /// <summary>
    /// Shard by value range.
    /// </summary>
    ByRange
}
