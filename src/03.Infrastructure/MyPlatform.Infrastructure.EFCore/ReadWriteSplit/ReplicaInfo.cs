namespace MyPlatform.Infrastructure.EFCore.ReadWriteSplit;

/// <summary>
/// Configuration information for a replica (read) database.
/// </summary>
public class ReplicaInfo
{
    /// <summary>
    /// Gets or sets the name identifier for this replica.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the connection string for this replica.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the weight for weighted load balancing. Higher weight means more traffic.
    /// </summary>
    public int Weight { get; set; } = 1;

    /// <summary>
    /// Gets or sets a value indicating whether this replica is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;
}
