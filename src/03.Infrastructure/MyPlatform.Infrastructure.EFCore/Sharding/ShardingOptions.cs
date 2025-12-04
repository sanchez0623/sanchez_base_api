namespace MyPlatform.Infrastructure.EFCore.Sharding;

/// <summary>
/// Configuration options for database sharding.
/// </summary>
public class ShardingOptions
{
    /// <summary>
    /// The configuration section name for sharding options.
    /// </summary>
    public const string SectionName = "Database:Sharding";

    /// <summary>
    /// Gets or sets a value indicating whether sharding is enabled.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the default data source name.
    /// </summary>
    public string DefaultDataSource { get; set; } = "master";

    /// <summary>
    /// Gets or sets the default look-back period in months for cross-shard queries.
    /// Used when no start time is specified.
    /// </summary>
    public int DefaultLookBackMonths { get; set; } = 12;

    /// <summary>
    /// Gets or sets the table sharding configurations.
    /// Key is the logical table name.
    /// </summary>
    public Dictionary<string, TableShardingConfig> Tables { get; set; } = new();
}
