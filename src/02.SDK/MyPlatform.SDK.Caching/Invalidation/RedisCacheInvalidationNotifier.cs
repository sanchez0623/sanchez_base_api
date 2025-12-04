using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyPlatform.SDK.Caching.Configuration;
using MyPlatform.Shared.Utils.Helpers;
using StackExchange.Redis;

namespace MyPlatform.SDK.Caching.Invalidation;

/// <summary>
/// Redis Pub/Sub implementation of cache invalidation notifier.
/// </summary>
public class RedisCacheInvalidationNotifier : ICacheInvalidationNotifier
{
    private readonly IConnectionMultiplexer _redis;
    private readonly CacheOptions _options;
    private readonly ILogger<RedisCacheInvalidationNotifier> _logger;
    private readonly string _instanceId;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisCacheInvalidationNotifier"/> class.
    /// </summary>
    /// <param name="redis">The Redis connection multiplexer.</param>
    /// <param name="options">The cache options.</param>
    /// <param name="logger">The logger.</param>
    public RedisCacheInvalidationNotifier(
        IConnectionMultiplexer redis,
        IOptions<CacheOptions> options,
        ILogger<RedisCacheInvalidationNotifier> logger)
    {
        _redis = redis;
        _options = options.Value;
        _logger = logger;
        _instanceId = string.IsNullOrEmpty(_options.InstanceId)
            ? Guid.NewGuid().ToString("N")
            : _options.InstanceId;
    }

    /// <inheritdoc />
    public string InstanceId => _instanceId;

    /// <inheritdoc />
    public async Task PublishInvalidationAsync(string key, CacheInvalidationType type = CacheInvalidationType.Remove)
    {
        await PublishInvalidationAsync(new[] { key }, type);
    }

    /// <inheritdoc />
    public async Task PublishInvalidationAsync(IEnumerable<string> keys, CacheInvalidationType type = CacheInvalidationType.Remove)
    {
        if (!_options.EnableInvalidationNotification)
        {
            return;
        }

        var keyArray = keys.ToArray();
        if (keyArray.Length == 0)
        {
            return;
        }

        var message = new CacheInvalidationMessage
        {
            Keys = keyArray,
            Type = type,
            SourceInstanceId = _instanceId,
            Timestamp = DateTimeOffset.UtcNow
        };

        await PublishMessageAsync(message);
    }

    /// <inheritdoc />
    public async Task PublishPatternInvalidationAsync(string pattern)
    {
        if (!_options.EnableInvalidationNotification)
        {
            return;
        }

        var message = new CacheInvalidationMessage
        {
            Keys = Array.Empty<string>(),
            Type = CacheInvalidationType.Pattern,
            SourceInstanceId = _instanceId,
            Timestamp = DateTimeOffset.UtcNow,
            Pattern = pattern
        };

        await PublishMessageAsync(message);
    }

    private async Task PublishMessageAsync(CacheInvalidationMessage message)
    {
        try
        {
            var json = JsonHelper.Serialize(message);
            var subscriber = _redis.GetSubscriber();
            await subscriber.PublishAsync(
                RedisChannel.Literal(_options.InvalidationChannel),
                json);

            _logger.LogDebug(
                "Published cache invalidation message for {KeyCount} keys, Type: {Type}, InstanceId: {InstanceId}",
                message.Keys.Length,
                message.Type,
                message.SourceInstanceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish cache invalidation message");
        }
    }
}
