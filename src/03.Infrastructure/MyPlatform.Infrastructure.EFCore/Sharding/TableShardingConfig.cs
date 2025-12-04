namespace MyPlatform.Infrastructure.EFCore.Sharding;

/// <summary>
/// Configuration for a sharded table.
/// </summary>
public class TableShardingConfig
{
    /// <summary>
    /// Gets or sets the column used for sharding.
    /// </summary>
    public string ShardingColumn { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of sharding strategy.
    /// </summary>
    public ShardingType ShardingType { get; set; }

    /// <summary>
    /// Gets or sets the table name format pattern.
    /// For date-based sharding: "orders_{0:yyyyMM}"
    /// For mod-based sharding: "orders_{0}"
    /// </summary>
    public string TableNameFormat { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of shards for mod-based sharding.
    /// </summary>
    public int ShardCount { get; set; } = 4;
}
