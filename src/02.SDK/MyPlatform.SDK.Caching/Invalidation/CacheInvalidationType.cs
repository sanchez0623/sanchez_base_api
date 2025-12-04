namespace MyPlatform.SDK.Caching.Invalidation;

/// <summary>
/// Represents the type of cache invalidation operation.
/// </summary>
public enum CacheInvalidationType
{
    /// <summary>
    /// Cache entry was removed.
    /// </summary>
    Remove,

    /// <summary>
    /// Cache entry was updated.
    /// </summary>
    Update,

    /// <summary>
    /// Cache entry expired.
    /// </summary>
    Expire,

    /// <summary>
    /// Cache entries matching a pattern should be invalidated.
    /// </summary>
    Pattern
}
