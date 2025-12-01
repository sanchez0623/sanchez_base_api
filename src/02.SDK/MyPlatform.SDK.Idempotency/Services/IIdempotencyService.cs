using MyPlatform.Infrastructure.Redis.Services;

namespace MyPlatform.SDK.Idempotency.Services;

/// <summary>
/// Stored result of an idempotent operation.
/// </summary>
public class IdempotentResult
{
    /// <summary>
    /// Gets or sets the HTTP status code.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the response body.
    /// </summary>
    public string? Body { get; set; }

    /// <summary>
    /// Gets or sets the content type.
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Service for managing idempotent operations.
/// </summary>
public interface IIdempotencyService
{
    /// <summary>
    /// Tries to acquire an idempotency lock and checks for existing results.
    /// </summary>
    /// <param name="key">The idempotency key.</param>
    /// <returns>A tuple indicating if the operation should proceed and any cached result.</returns>
    Task<(bool ShouldProceed, IDistributedLock? Lock, IdempotentResult? CachedResult)> TryAcquireAsync(string key);

    /// <summary>
    /// Stores the result of an idempotent operation.
    /// </summary>
    /// <param name="key">The idempotency key.</param>
    /// <param name="result">The result to store.</param>
    /// <param name="expiry">Optional expiration time.</param>
    Task StoreResultAsync(string key, IdempotentResult result, TimeSpan? expiry = null);

    /// <summary>
    /// Gets a cached result for an idempotency key.
    /// </summary>
    /// <param name="key">The idempotency key.</param>
    /// <returns>The cached result if found; otherwise, null.</returns>
    Task<IdempotentResult?> GetResultAsync(string key);

    /// <summary>
    /// Checks if an idempotency key exists.
    /// </summary>
    /// <param name="key">The idempotency key.</param>
    /// <returns>True if the key exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(string key);
}

/// <summary>
/// Redis implementation of idempotency service.
/// </summary>
public class RedisIdempotencyService : IIdempotencyService
{
    private readonly IRedisCacheService _cacheService;
    private readonly IDistributedLockService _lockService;
    private readonly TimeSpan _defaultExpiry;
    private readonly TimeSpan _lockExpiry;
    private readonly TimeSpan _lockWaitTime;
    private const string KeyPrefix = "idempotency:";
    private const string ResultPrefix = "result:";

    public RedisIdempotencyService(
        IRedisCacheService cacheService,
        IDistributedLockService lockService,
        TimeSpan defaultExpiry,
        TimeSpan lockExpiry,
        TimeSpan lockWaitTime)
    {
        _cacheService = cacheService;
        _lockService = lockService;
        _defaultExpiry = defaultExpiry;
        _lockExpiry = lockExpiry;
        _lockWaitTime = lockWaitTime;
    }

    /// <inheritdoc />
    public async Task<(bool ShouldProceed, IDistributedLock? Lock, IdempotentResult? CachedResult)> TryAcquireAsync(string key)
    {
        var fullKey = $"{KeyPrefix}{key}";

        // Check for cached result first
        var cachedResult = await GetResultAsync(key);
        if (cachedResult is not null)
        {
            return (false, null, cachedResult);
        }

        // Try to acquire lock
        var @lock = await _lockService.AcquireLockAsync(fullKey, _lockExpiry, _lockWaitTime);
        if (@lock is null)
        {
            // Could not acquire lock, check again for cached result
            cachedResult = await GetResultAsync(key);
            return (false, null, cachedResult);
        }

        // Check again after acquiring lock
        cachedResult = await GetResultAsync(key);
        if (cachedResult is not null)
        {
            await @lock.ReleaseAsync();
            return (false, null, cachedResult);
        }

        return (true, @lock, null);
    }

    /// <inheritdoc />
    public async Task StoreResultAsync(string key, IdempotentResult result, TimeSpan? expiry = null)
    {
        var resultKey = $"{KeyPrefix}{ResultPrefix}{key}";
        await _cacheService.SetAsync(resultKey, result, expiry ?? _defaultExpiry);
    }

    /// <inheritdoc />
    public async Task<IdempotentResult?> GetResultAsync(string key)
    {
        var resultKey = $"{KeyPrefix}{ResultPrefix}{key}";
        return await _cacheService.GetAsync<IdempotentResult>(resultKey);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string key)
    {
        var resultKey = $"{KeyPrefix}{ResultPrefix}{key}";
        return await _cacheService.ExistsAsync(resultKey);
    }
}
