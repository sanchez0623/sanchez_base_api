namespace MyPlatform.SDK.Caching.Invalidation;

/// <summary>
/// Interface for publishing cache invalidation notifications across instances.
/// </summary>
public interface ICacheInvalidationNotifier
{
    /// <summary>
    /// Gets the unique identifier of this instance.
    /// </summary>
    string InstanceId { get; }

    /// <summary>
    /// Publishes an invalidation notification for a single cache key.
    /// </summary>
    /// <param name="key">The cache key to invalidate.</param>
    /// <param name="type">The type of invalidation operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PublishInvalidationAsync(string key, CacheInvalidationType type = CacheInvalidationType.Remove);

    /// <summary>
    /// Publishes an invalidation notification for multiple cache keys.
    /// </summary>
    /// <param name="keys">The cache keys to invalidate.</param>
    /// <param name="type">The type of invalidation operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PublishInvalidationAsync(IEnumerable<string> keys, CacheInvalidationType type = CacheInvalidationType.Remove);

    /// <summary>
    /// Publishes an invalidation notification for cache keys matching a pattern.
    /// </summary>
    /// <param name="pattern">The pattern to match cache keys.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PublishPatternInvalidationAsync(string pattern);
}
