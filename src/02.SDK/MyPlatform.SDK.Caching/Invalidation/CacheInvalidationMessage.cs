namespace MyPlatform.SDK.Caching.Invalidation;

/// <summary>
/// Represents a cache invalidation message sent via Redis Pub/Sub.
/// </summary>
public class CacheInvalidationMessage
{
    /// <summary>
    /// Gets or sets the cache keys to invalidate.
    /// </summary>
    public string[] Keys { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the type of invalidation operation.
    /// </summary>
    public CacheInvalidationType Type { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the instance that sent this message.
    /// Used to ignore messages sent by the same instance.
    /// </summary>
    public string SourceInstanceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the message was created.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the pattern for pattern-based invalidation.
    /// Only used when Type is Pattern.
    /// </summary>
    public string? Pattern { get; set; }
}
