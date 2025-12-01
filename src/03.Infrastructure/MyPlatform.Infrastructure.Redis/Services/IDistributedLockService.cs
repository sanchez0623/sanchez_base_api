namespace MyPlatform.Infrastructure.Redis.Services;

/// <summary>
/// Service for distributed locking using Redis.
/// </summary>
public interface IDistributedLockService
{
    /// <summary>
    /// Acquires a distributed lock.
    /// </summary>
    /// <param name="key">The lock key.</param>
    /// <param name="expiry">The lock expiration time.</param>
    /// <param name="waitTime">Maximum time to wait for the lock.</param>
    /// <param name="retryInterval">Interval between retry attempts.</param>
    /// <returns>A disposable lock handle if acquired; otherwise, null.</returns>
    Task<IDistributedLock?> AcquireLockAsync(
        string key,
        TimeSpan expiry,
        TimeSpan? waitTime = null,
        TimeSpan? retryInterval = null);

    /// <summary>
    /// Checks if a lock exists.
    /// </summary>
    /// <param name="key">The lock key.</param>
    /// <returns>True if the lock exists; otherwise, false.</returns>
    Task<bool> IsLockedAsync(string key);
}

/// <summary>
/// Represents an acquired distributed lock.
/// </summary>
public interface IDistributedLock : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// Gets the lock key.
    /// </summary>
    string Key { get; }

    /// <summary>
    /// Gets a value indicating whether the lock is held.
    /// </summary>
    bool IsAcquired { get; }

    /// <summary>
    /// Extends the lock expiration time.
    /// </summary>
    /// <param name="expiry">The new expiration time.</param>
    /// <returns>True if the extension was successful; otherwise, false.</returns>
    Task<bool> ExtendAsync(TimeSpan expiry);

    /// <summary>
    /// Releases the lock.
    /// </summary>
    Task ReleaseAsync();
}
