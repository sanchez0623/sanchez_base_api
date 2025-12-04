using System.Threading.Channels;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyPlatform.SDK.Caching.Configuration;
using MyPlatform.Shared.Utils.Helpers;
using StackExchange.Redis;

namespace MyPlatform.SDK.Caching.Invalidation;

/// <summary>
/// Background service that subscribes to cache invalidation messages and processes them in batches.
/// </summary>
public class CacheInvalidationSubscriber : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IMemoryCache _memoryCache;
    private readonly CacheOptions _options;
    private readonly ICacheInvalidationNotifier _notifier;
    private readonly ILogger<CacheInvalidationSubscriber> _logger;
    private readonly Channel<CacheInvalidationMessage> _messageChannel;

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheInvalidationSubscriber"/> class.
    /// </summary>
    /// <param name="redis">The Redis connection multiplexer.</param>
    /// <param name="memoryCache">The memory cache.</param>
    /// <param name="options">The cache options.</param>
    /// <param name="notifier">The cache invalidation notifier.</param>
    /// <param name="logger">The logger.</param>
    public CacheInvalidationSubscriber(
        IConnectionMultiplexer redis,
        IMemoryCache memoryCache,
        IOptions<CacheOptions> options,
        ICacheInvalidationNotifier notifier,
        ILogger<CacheInvalidationSubscriber> logger)
    {
        _redis = redis;
        _memoryCache = memoryCache;
        _options = options.Value;
        _notifier = notifier;
        _logger = logger;

        _messageChannel = Channel.CreateBounded<CacheInvalidationMessage>(
            new BoundedChannelOptions(_options.MaxBatchSize * 2)
            {
                FullMode = BoundedChannelFullMode.DropOldest
            });
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.EnableInvalidationNotification)
        {
            _logger.LogInformation("Cache invalidation notification is disabled");
            return;
        }

        _logger.LogInformation(
            "Starting cache invalidation subscriber on channel '{Channel}' with InstanceId '{InstanceId}'",
            _options.InvalidationChannel,
            _notifier.InstanceId);

        // Start the batch processor task
        var batchProcessorTask = ProcessBatchesAsync(stoppingToken);

        // Subscribe to Redis channel
        var subscriber = _redis.GetSubscriber();
        await subscriber.SubscribeAsync(
            RedisChannel.Literal(_options.InvalidationChannel),
            async (_, message) => await HandleMessageAsync(message));

        // Wait for cancellation
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Cache invalidation subscriber is stopping");
        }

        // Unsubscribe on shutdown
        await subscriber.UnsubscribeAsync(RedisChannel.Literal(_options.InvalidationChannel));

        // Complete the channel and wait for batch processor to finish
        _messageChannel.Writer.Complete();
        await batchProcessorTask;
    }

    private async ValueTask HandleMessageAsync(RedisValue message)
    {
        try
        {
            if (message.IsNullOrEmpty)
            {
                return;
            }

            var json = (string?)message;
            if (string.IsNullOrEmpty(json))
            {
                return;
            }

            var invalidationMessage = JsonHelper.Deserialize<CacheInvalidationMessage>(json);
            if (invalidationMessage == null)
            {
                return;
            }

            // Ignore messages from this instance
            if (invalidationMessage.SourceInstanceId == _notifier.InstanceId)
            {
                _logger.LogDebug("Ignoring cache invalidation message from self");
                return;
            }

            await _messageChannel.Writer.WriteAsync(invalidationMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle cache invalidation message");
        }
    }

    private async Task ProcessBatchesAsync(CancellationToken stoppingToken)
    {
        var batch = new List<CacheInvalidationMessage>();
        var processedKeys = new HashSet<string>();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                batch.Clear();
                processedKeys.Clear();

                // Collect messages for batch processing
                using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(_options.BatchIntervalMs));
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, stoppingToken);

                try
                {
                    while (batch.Count < _options.MaxBatchSize)
                    {
                        var message = await _messageChannel.Reader.ReadAsync(linkedCts.Token);
                        batch.Add(message);
                    }
                }
                catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested)
                {
                    // Timeout reached, process collected messages
                }
                catch (ChannelClosedException)
                {
                    // Channel closed, process remaining messages and exit
                    break;
                }

                if (batch.Count == 0)
                {
                    continue;
                }

                // Process batch with deduplication
                foreach (var message in batch)
                {
                    if (message.Type == CacheInvalidationType.Pattern && !string.IsNullOrEmpty(message.Pattern))
                    {
                        // Pattern invalidation - MemoryCache doesn't support pattern-based removal
                        // This is a known limitation; users should use key-based invalidation instead
                        _logger.LogWarning(
                            "Received pattern invalidation for pattern '{Pattern}', but MemoryCache doesn't support pattern removal. " +
                            "Consider using explicit key-based invalidation instead.",
                            message.Pattern);
                        continue;
                    }

                    foreach (var key in message.Keys)
                    {
                        if (processedKeys.Add(key))
                        {
                            _memoryCache.Remove(key);
                        }
                    }
                }

                if (processedKeys.Count > 0)
                {
                    _logger.LogDebug(
                        "Processed batch of {BatchCount} messages, invalidated {KeyCount} unique keys",
                        batch.Count,
                        processedKeys.Count);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing cache invalidation batch");
            }
        }

        // Process any remaining messages in the channel
        while (_messageChannel.Reader.TryRead(out var message))
        {
            foreach (var key in message.Keys)
            {
                if (processedKeys.Add(key))
                {
                    _memoryCache.Remove(key);
                }
            }
        }

        if (processedKeys.Count > 0)
        {
            _logger.LogDebug("Processed {KeyCount} remaining keys during shutdown", processedKeys.Count);
        }
    }
}
