using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace MyPlatform.SDK.Idempotency.Services;

/// <summary>
/// 基于 Redis 的事件消费者幂等性检查器
/// 使用 Lua 脚本保证原子性，支持 10W+ QPS
/// </summary>
public class RedisEventIdempotencyChecker : IEventIdempotencyChecker
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisEventIdempotencyChecker> _logger;

    private const string KeyPrefix = "event:idempotent:";
    private static readonly TimeSpan s_defaultExpiry = TimeSpan.FromDays(7);

    // 状态常量（与 EventProcessingStatus 对应）
    private const string StatusProcessing = "processing";
    private const string StatusCompleted = "completed";
    private const string StatusFailed = "failed";

    public RedisEventIdempotencyChecker(
        IConnectionMultiplexer redis,
        ILogger<RedisEventIdempotencyChecker> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> TryAcquireAsync(
        string eventId,
        string consumerGroup,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var key = BuildKey(eventId, consumerGroup);
        var ttl = expiry ?? s_defaultExpiry;

        try
        {
            // 使用 Lua 脚本确保原子性：
            // 1. 如果 key 不存在，设置为 processing 并返回成功
            // 2. 如果 key 存在且为 failed，允许重试，设置为 processing 并返回成功
            // 3. 其他情况返回失败
            var script = @"
                local current = redis.call('GET', KEYS[1])
                if current == false then
                    redis.call('SET', KEYS[1], ARGV[1], 'EX', ARGV[2])
                    return 1
                elseif current == 'failed' then
                    redis.call('SET', KEYS[1], ARGV[1], 'EX', ARGV[2])
                    return 1
                else
                    return 0
                end
            ";

            var result = await db.ScriptEvaluateAsync(
                script,
                new RedisKey[] { key },
                new RedisValue[] { StatusProcessing, (int)ttl.TotalSeconds });

            var acquired = (int)result == 1;

            if (acquired)
            {
                _logger.LogDebug(
                    "Acquired processing lock for event {EventId} in consumer group {ConsumerGroup}",
                    eventId, consumerGroup);
            }
            else
            {
                _logger.LogDebug(
                    "Event {EventId} already being processed in consumer group {ConsumerGroup}",
                    eventId, consumerGroup);
            }

            return acquired;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to acquire lock for event {EventId} in consumer group {ConsumerGroup}",
                eventId, consumerGroup);

            // Redis 故障时，降级为允许处理（避免消息堆积）
            // 依赖业务层的幂等性保证
            return true;
        }
    }

    /// <inheritdoc />
    public async Task MarkCompletedAsync(
        string eventId,
        string consumerGroup,
        CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var key = BuildKey(eventId, consumerGroup);

        try
        {
            // 更新状态为已完成，保持原有 TTL
            await db.StringSetAsync(key, StatusCompleted, keepTtl: true);

            _logger.LogDebug(
                "Marked event {EventId} as completed in consumer group {ConsumerGroup}",
                eventId, consumerGroup);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to mark event {EventId} as completed, but business logic succeeded",
                eventId);
            // 不抛异常，业务已成功执行
        }
    }

    /// <inheritdoc />
    public async Task MarkFailedAsync(
        string eventId,
        string consumerGroup,
        CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var key = BuildKey(eventId, consumerGroup);

        try
        {
            // 标记为失败，允许后续重试
            await db.StringSetAsync(key, StatusFailed, keepTtl: true);

            _logger.LogDebug(
                "Marked event {EventId} as failed in consumer group {ConsumerGroup}, will allow retry",
                eventId, consumerGroup);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to mark event {EventId} as failed",
                eventId);
        }
    }

    /// <inheritdoc />
    public async Task<bool> IsProcessedAsync(
        string eventId,
        string consumerGroup,
        CancellationToken cancellationToken = default)
    {
        var status = await GetStatusAsync(eventId, consumerGroup, cancellationToken);
        return status == EventProcessingStatus.Completed;
    }

    /// <inheritdoc />
    public async Task<EventProcessingStatus?> GetStatusAsync(
        string eventId,
        string consumerGroup,
        CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var key = BuildKey(eventId, consumerGroup);

        try
        {
            var value = await db.StringGetAsync(key);

            if (value.IsNullOrEmpty)
            {
                return null;
            }

            return value.ToString() switch
            {
                StatusProcessing => EventProcessingStatus.Processing,
                StatusCompleted => EventProcessingStatus.Completed,
                StatusFailed => EventProcessingStatus.Failed,
                _ => null
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to get status for event {EventId} in consumer group {ConsumerGroup}",
                eventId, consumerGroup);
            return null;
        }
    }

    private static string BuildKey(string eventId, string consumerGroup)
    {
        return $"{KeyPrefix}{consumerGroup}:{eventId}";
    }
}
