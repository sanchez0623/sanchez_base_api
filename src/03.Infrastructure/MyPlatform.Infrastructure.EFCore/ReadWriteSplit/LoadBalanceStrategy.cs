namespace MyPlatform.Infrastructure.EFCore.ReadWriteSplit;

/// <summary>
/// Load balance strategy for replica database selection.
/// </summary>
public enum LoadBalanceStrategy
{
    /// <summary>
    /// Round-robin selection of replicas.
    /// </summary>
    RoundRobin,

    /// <summary>
    /// Random selection of replicas.
    /// </summary>
    Random,

    /// <summary>
    /// Weighted round-robin selection based on replica weights.
    /// </summary>
    WeightedRoundRobin,

    /// <summary>
    /// Selection based on least connections (reserved for future implementation).
    /// </summary>
    LeastConnections
}
