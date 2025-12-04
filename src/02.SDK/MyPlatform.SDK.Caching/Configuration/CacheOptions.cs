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

    /// <summary>
    /// Gets or sets a value indicating whether cache invalidation notification is enabled.
    /// When enabled, cache updates/removals will be published via Redis Pub/Sub
    /// to notify other instances to invalidate their local caches.
    /// </summary>
    public bool EnableInvalidationNotification { get; set; } = false;

    /// <summary>
    /// Gets or sets the Redis Pub/Sub channel name for cache invalidation messages.
    /// </summary>
    public string InvalidationChannel { get; set; } = "cache:invalidate";

    /// <summary>
    /// Gets or sets the unique identifier for this instance.
    /// If empty, a GUID will be generated automatically.
    /// Used to identify the source of invalidation messages and ignore self-sent messages.
    /// </summary>
    public string InstanceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the batch processing interval in milliseconds.
    /// Invalidation messages will be collected and processed in batches at this interval.
    /// </summary>
    public int BatchIntervalMs { get; set; } = 100;

    /// <summary>
    /// Gets or sets the maximum number of messages to process in a single batch.
    /// </summary>
    public int MaxBatchSize { get; set; } = 1000;
}
