using Microsoft.Extensions.Options;
using MyPlatform.Infrastructure.Redis.Configuration;
using StackExchange.Redis;

namespace MyPlatform.Infrastructure.Redis.Services;

/// <summary>
/// Redis implementation of distributed lock service.
/// </summary>
public class RedisDistributedLockService : IDistributedLockService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly string _instanceName;

    public RedisDistributedLockService(IConnectionMultiplexer redis, IOptions<RedisOptions> options)
    {
        _redis = redis;
        _instanceName = options.Value.InstanceName;
    }

    /// <inheritdoc />
    public async Task<IDistributedLock?> AcquireLockAsync(
        string key,
        TimeSpan expiry,
        TimeSpan? waitTime = null,
        TimeSpan? retryInterval = null)
    {
        var lockKey = $"{_instanceName}lock:{key}";
        var lockValue = Guid.NewGuid().ToString();
        var database = _redis.GetDatabase();

        waitTime ??= TimeSpan.Zero;
        retryInterval ??= TimeSpan.FromMilliseconds(100);

        var startTime = DateTime.UtcNow;

        do
        {
            if (await database.StringSetAsync(lockKey, lockValue, expiry, When.NotExists))
            {
                return new RedisDistributedLock(database, lockKey, lockValue);
            }

            if (waitTime.Value > TimeSpan.Zero)
            {
                await Task.Delay(retryInterval.Value);
            }
        }
        while (DateTime.UtcNow - startTime < waitTime.Value);

        return null;
    }

    /// <inheritdoc />
    public async Task<bool> IsLockedAsync(string key)
    {
        var lockKey = $"{_instanceName}lock:{key}";
        var database = _redis.GetDatabase();
        return await database.KeyExistsAsync(lockKey);
    }
}

/// <summary>
/// Redis distributed lock implementation.
/// </summary>
internal class RedisDistributedLock : IDistributedLock
{
    private readonly IDatabase _database;
    private readonly string _lockValue;
    private bool _isReleased;

    public string Key { get; }
    public bool IsAcquired => !_isReleased;

    public RedisDistributedLock(IDatabase database, string key, string lockValue)
    {
        _database = database;
        Key = key;
        _lockValue = lockValue;
    }

    public async Task<bool> ExtendAsync(TimeSpan expiry)
    {
        if (_isReleased) return false;

        var script = @"
            if redis.call('get', KEYS[1]) == ARGV[1] then
                return redis.call('pexpire', KEYS[1], ARGV[2])
            else
                return 0
            end";

        var result = await _database.ScriptEvaluateAsync(script,
            new RedisKey[] { Key },
            new RedisValue[] { _lockValue, (long)expiry.TotalMilliseconds });

        return (long)result! == 1;
    }

    public async Task ReleaseAsync()
    {
        if (_isReleased) return;

        var script = @"
            if redis.call('get', KEYS[1]) == ARGV[1] then
                return redis.call('del', KEYS[1])
            else
                return 0
            end";

        await _database.ScriptEvaluateAsync(script,
            new RedisKey[] { Key },
            new RedisValue[] { _lockValue });

        _isReleased = true;
    }

    public async ValueTask DisposeAsync()
    {
        await ReleaseAsync();
    }

    public void Dispose()
    {
        ReleaseAsync().GetAwaiter().GetResult();
    }
}
