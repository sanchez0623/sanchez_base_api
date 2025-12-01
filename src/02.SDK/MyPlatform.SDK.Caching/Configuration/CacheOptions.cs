namespace MyPlatform.SDK.Caching.Configuration;

/// <summary>
/// Multi-level cache configuration options.
/// </summary>
public class CacheOptions
{
    /// <summary>
    /// Gets or sets the default local cache expiration in seconds.
    /// </summary>
    public int LocalCacheExpirationSeconds { get; set; } = 60;

    /// <summary>
    /// Gets or sets the default distributed cache expiration in seconds.
    /// </summary>
    public int DistributedCacheExpirationSeconds { get; set; } = 300;

    /// <summary>
    /// Gets or sets a value indicating whether local cache is enabled.
    /// </summary>
    public bool EnableLocalCache { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether distributed cache is enabled.
    /// </summary>
    public bool EnableDistributedCache { get; set; } = true;

    /// <summary>
    /// Gets or sets the local cache size limit.
    /// </summary>
    public long LocalCacheSizeLimit { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the key prefix for cache entries.
    /// </summary>
    public string KeyPrefix { get; set; } = "cache:";
}
