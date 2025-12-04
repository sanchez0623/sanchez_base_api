namespace MyPlatform.Infrastructure.EFCore.ReadWriteSplit;

/// <summary>
/// Configuration options for read-write split functionality.
/// </summary>
public class ReadWriteOptions
{
    /// <summary>
    /// The configuration section name for read-write split options.
    /// </summary>
    public const string SectionName = "Database:ReadWriteSplit";

    /// <summary>
    /// Gets or sets a value indicating whether read-write split is enabled.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the master (write) database configuration.
    /// </summary>
    public MasterConfig Master { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of replica (read) databases.
    /// </summary>
    public List<ReplicaInfo> Replicas { get; set; } = new();

    /// <summary>
    /// Gets or sets the load balance strategy for selecting replicas.
    /// </summary>
    public LoadBalanceStrategy LoadBalanceStrategy { get; set; } = LoadBalanceStrategy.RoundRobin;

    /// <summary>
    /// Gets or sets a value indicating whether read operations within a transaction should use the master database.
    /// </summary>
    public bool ReadFromMasterInTransaction { get; set; } = true;
}

/// <summary>
/// Configuration for the master (write) database.
/// </summary>
public class MasterConfig
{
    /// <summary>
    /// Gets or sets the connection string for the master database.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;
}
