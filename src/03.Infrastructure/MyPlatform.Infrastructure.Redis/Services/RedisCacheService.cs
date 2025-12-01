using MyPlatform.Shared.Utils.Helpers;
using StackExchange.Redis;

namespace MyPlatform.Infrastructure.Redis.Services;

/// <summary>
/// Service for Redis cache operations.
/// </summary>
public interface IRedisCacheService
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
    /// <param name="expiry">Optional expiration time.</param>
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);

    /// <summary>
    /// Gets or sets a value in the cache.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="factory">Factory to create the value if not found.</param>
    /// <param name="expiry">Optional expiration time.</param>
    /// <returns>The cached or newly created value.</returns>
    Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null);

    /// <summary>
    /// Removes a value from the cache.
    /// </summary>
    /// <param name="key">The cache key.</param>
    Task RemoveAsync(string key);

    /// <summary>
    /// Removes all values matching a pattern.
    /// </summary>
    /// <param name="pattern">The key pattern.</param>
    Task RemoveByPatternAsync(string pattern);

    /// <summary>
    /// Checks if a key exists in the cache.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <returns>True if the key exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(string key);

    /// <summary>
    /// Increments a value in the cache.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to increment by.</param>
    /// <returns>The new value.</returns>
    Task<long> IncrementAsync(string key, long value = 1);

    /// <summary>
    /// Sets a hash field value.
    /// </summary>
    /// <param name="key">The hash key.</param>
    /// <param name="field">The field name.</param>
    /// <param name="value">The value to set.</param>
    Task HashSetAsync<T>(string key, string field, T value);

    /// <summary>
    /// Gets a hash field value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The hash key.</param>
    /// <param name="field">The field name.</param>
    /// <returns>The field value if found; otherwise, default.</returns>
    Task<T?> HashGetAsync<T>(string key, string field);

    /// <summary>
    /// Gets all hash values.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <param name="key">The hash key.</param>
    /// <returns>A dictionary of field names and values.</returns>
    Task<Dictionary<string, T>> HashGetAllAsync<T>(string key);
}

/// <summary>
/// Redis implementation of the cache service.
/// </summary>
public class RedisCacheService : IRedisCacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly string _instanceName;

    public RedisCacheService(IConnectionMultiplexer redis, string instanceName = "")
    {
        _redis = redis;
        _instanceName = instanceName;
    }

    private IDatabase Database => _redis.GetDatabase();

    private string GetKey(string key) => string.IsNullOrEmpty(_instanceName) ? key : $"{_instanceName}{key}";

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await Database.StringGetAsync(GetKey(key));
        if (value.IsNullOrEmpty)
        {
            return default;
        }

        return JsonHelper.Deserialize<T>(value!);
    }

    /// <inheritdoc />
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var json = JsonHelper.Serialize(value);
        await Database.StringSetAsync(GetKey(key), json, expiry);
    }

    /// <inheritdoc />
    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null)
    {
        var cached = await GetAsync<T>(key);
        if (cached is not null)
        {
            return cached;
        }

        var value = await factory();
        if (value is not null)
        {
            await SetAsync(key, value, expiry);
        }

        return value;
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key)
    {
        await Database.KeyDeleteAsync(GetKey(key));
    }

    /// <inheritdoc />
    public async Task RemoveByPatternAsync(string pattern)
    {
        var server = _redis.GetServer(_redis.GetEndPoints().First());
        var keys = server.Keys(pattern: GetKey(pattern)).ToArray();
        if (keys.Length > 0)
        {
            await Database.KeyDeleteAsync(keys);
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string key)
    {
        return await Database.KeyExistsAsync(GetKey(key));
    }

    /// <inheritdoc />
    public async Task<long> IncrementAsync(string key, long value = 1)
    {
        return await Database.StringIncrementAsync(GetKey(key), value);
    }

    /// <inheritdoc />
    public async Task HashSetAsync<T>(string key, string field, T value)
    {
        var json = JsonHelper.Serialize(value);
        await Database.HashSetAsync(GetKey(key), field, json);
    }

    /// <inheritdoc />
    public async Task<T?> HashGetAsync<T>(string key, string field)
    {
        var value = await Database.HashGetAsync(GetKey(key), field);
        if (value.IsNullOrEmpty)
        {
            return default;
        }

        return JsonHelper.Deserialize<T>(value!);
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, T>> HashGetAllAsync<T>(string key)
    {
        var entries = await Database.HashGetAllAsync(GetKey(key));
        var result = new Dictionary<string, T>();

        foreach (var entry in entries)
        {
            var value = JsonHelper.Deserialize<T>(entry.Value!);
            if (value is not null)
            {
                result[entry.Name!] = value;
            }
        }

        return result;
    }
}
