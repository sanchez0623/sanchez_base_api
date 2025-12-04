using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MyPlatform.Infrastructure.Redis.Services;
using MyPlatform.SDK.Caching.Configuration;
using MyPlatform.SDK.Caching.Invalidation;

namespace MyPlatform.SDK.Caching.Services;

/// <summary>
/// Interface for multi-level cache service.
/// </summary>
public interface IMultiLevelCache
{
    /// <summary>
    /// Gets a value from the cache.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <returns>The cached value if found; otherwise, default.</returns>
    Task<T?> GetAsync<T>(string key);

    /// <summary>
    /// Sets a value in the cache.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="localExpiry">Optional local cache expiration.</param>
    /// <param name="distributedExpiry">Optional distributed cache expiration.</param>
    Task SetAsync<T>(string key, T value, TimeSpan? localExpiry = null, TimeSpan? distributedExpiry = null);

    /// <summary>
    /// Gets or sets a value in the cache.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="factory">Factory to create the value if not found.</param>
    /// <param name="localExpiry">Optional local cache expiration.</param>
    /// <param name="distributedExpiry">Optional distributed cache expiration.</param>
    /// <returns>The cached or newly created value.</returns>
    Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? localExpiry = null, TimeSpan? distributedExpiry = null);

    /// <summary>
    /// Removes a value from both cache levels.
    /// </summary>
    /// <param name="key">The cache key.</param>
    Task RemoveAsync(string key);

    /// <summary>
    /// Invalidates the local cache for a key.
    /// </summary>
    /// <param name="key">The cache key.</param>
    void InvalidateLocal(string key);
}

/// <summary>
/// Multi-level cache implementation with local memory and distributed Redis cache.
/// </summary>
public class MultiLevelCache : IMultiLevelCache
{
    private readonly IMemoryCache _memoryCache;
    private readonly IRedisCacheService _redisCache;
    private readonly CacheOptions _options;
    private readonly ICacheInvalidationNotifier? _invalidationNotifier;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiLevelCache"/> class.
    /// </summary>
    /// <param name="memoryCache">The memory cache.</param>
    /// <param name="redisCache">The Redis cache service.</param>
    /// <param name="options">The cache options.</param>
    /// <param name="invalidationNotifier">The optional cache invalidation notifier.</param>
    public MultiLevelCache(
        IMemoryCache memoryCache,
        IRedisCacheService redisCache,
        IOptions<CacheOptions> options,
        ICacheInvalidationNotifier? invalidationNotifier = null)
    {
        _memoryCache = memoryCache;
        _redisCache = redisCache;
        _options = options.Value;
        _invalidationNotifier = invalidationNotifier;
    }

    private string GetKey(string key) => $"{_options.KeyPrefix}{key}";

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key)
    {
        var fullKey = GetKey(key);

        // Try local cache first
        if (_options.EnableLocalCache && _memoryCache.TryGetValue(fullKey, out T? localValue))
        {
            return localValue;
        }

        // Try distributed cache
        if (_options.EnableDistributedCache)
        {
            var distributedValue = await _redisCache.GetAsync<T>(fullKey);
            if (distributedValue is not null)
            {
                // Populate local cache
                if (_options.EnableLocalCache)
                {
                    var localExpiry = TimeSpan.FromSeconds(_options.LocalCacheExpirationSeconds);
                    _memoryCache.Set(fullKey, distributedValue, localExpiry);
                }

                return distributedValue;
            }
        }

        return default;
    }

    /// <inheritdoc />
    public async Task SetAsync<T>(string key, T value, TimeSpan? localExpiry = null, TimeSpan? distributedExpiry = null)
    {
        var fullKey = GetKey(key);
        localExpiry ??= TimeSpan.FromSeconds(_options.LocalCacheExpirationSeconds);
        distributedExpiry ??= TimeSpan.FromSeconds(_options.DistributedCacheExpirationSeconds);

        // Set in local cache
        if (_options.EnableLocalCache)
        {
            _memoryCache.Set(fullKey, value, localExpiry.Value);
        }

        // Set in distributed cache
        if (_options.EnableDistributedCache)
        {
            await _redisCache.SetAsync(fullKey, value, distributedExpiry);
        }

        // Publish invalidation notification to other instances
        if (_invalidationNotifier != null)
        {
            await _invalidationNotifier.PublishInvalidationAsync(fullKey, CacheInvalidationType.Update);
        }
    }

    /// <inheritdoc />
    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? localExpiry = null, TimeSpan? distributedExpiry = null)
    {
        var cached = await GetAsync<T>(key);
        if (cached is not null)
        {
            return cached;
        }

        var value = await factory();
        if (value is not null)
        {
            await SetAsync(key, value, localExpiry, distributedExpiry);
        }

        return value;
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key)
    {
        var fullKey = GetKey(key);

        if (_options.EnableLocalCache)
        {
            _memoryCache.Remove(fullKey);
        }

        if (_options.EnableDistributedCache)
        {
            await _redisCache.RemoveAsync(fullKey);
        }

        // Publish invalidation notification to other instances
        if (_invalidationNotifier != null)
        {
            await _invalidationNotifier.PublishInvalidationAsync(fullKey, CacheInvalidationType.Remove);
        }
    }

    /// <inheritdoc />
    public void InvalidateLocal(string key)
    {
        var fullKey = GetKey(key);
        _memoryCache.Remove(fullKey);
    }
}
