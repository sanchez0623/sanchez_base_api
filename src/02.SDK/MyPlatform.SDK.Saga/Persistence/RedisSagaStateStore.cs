using Microsoft.Extensions.Options;
using MyPlatform.SDK.Saga.Configuration;
using MyPlatform.SDK.Saga.Models;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace MyPlatform.SDK.Saga.Persistence;

/// <summary>
/// Redis implementation of saga state store for high-performance scenarios.
/// </summary>
public class RedisSagaStateStore : ISagaStateStore
{
    private readonly IConnectionMultiplexer _redis;
    private readonly SagaOptions _options;
    private readonly string _keyPrefix;
    private readonly TimeSpan _expiration;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisSagaStateStore"/> class.
    /// </summary>
    /// <param name="redis">The Redis connection multiplexer.</param>
    /// <param name="options">The saga options.</param>
    public RedisSagaStateStore(IConnectionMultiplexer redis, IOptions<SagaOptions> options)
    {
        _redis = redis;
        _options = options.Value;
        _keyPrefix = _options.Redis?.KeyPrefix ?? "saga:";
        _expiration = TimeSpan.FromMinutes(_options.Redis?.ExpirationMinutes ?? 60);
    }

    private IDatabase Database => _redis.GetDatabase();

    private string GetKey(string sagaId) => $"{_keyPrefix}{sagaId}";

    private string GetIndexKey(string status) => $"{_keyPrefix}status:{status}";

    private string GetRetryIndexKey() => $"{_keyPrefix}retries";

    /// <inheritdoc />
    public async Task SaveAsync(SagaState state, CancellationToken cancellationToken = default)
    {
        state.UpdatedAt = DateTime.UtcNow;
        var key = GetKey(state.SagaId);
        var json = JsonConvert.SerializeObject(state);

        var db = Database;

        // Save the state
        await db.StringSetAsync(key, json, _expiration);

        // Update status index
        await db.SetAddAsync(GetIndexKey(state.Status.ToString()), state.SagaId);

        // Update retry index if needed
        if (state.NextRetryAt.HasValue)
        {
            var score = state.NextRetryAt.Value.Ticks;
            await db.SortedSetAddAsync(GetRetryIndexKey(), state.SagaId, score);
        }
        else
        {
            await db.SortedSetRemoveAsync(GetRetryIndexKey(), state.SagaId);
        }
    }

    /// <inheritdoc />
    public async Task<SagaState?> GetAsync(string sagaId, CancellationToken cancellationToken = default)
    {
        var key = GetKey(sagaId);
        var value = await Database.StringGetAsync(key);

        if (value.IsNullOrEmpty)
        {
            return null;
        }

        return JsonConvert.DeserializeObject<SagaState>(value!);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SagaState>> GetByStatusAsync(SagaStatus status, CancellationToken cancellationToken = default)
    {
        var indexKey = GetIndexKey(status.ToString());
        var members = await Database.SetMembersAsync(indexKey);

        var states = new List<SagaState>();
        foreach (var member in members)
        {
            var state = await GetAsync(member!, cancellationToken);
            if (state != null)
            {
                states.Add(state);
            }
        }

        return states;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SagaState>> GetPendingRetriesAsync(int batchSize = 100, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow.Ticks;
        var retryKey = GetRetryIndexKey();

        var members = await Database.SortedSetRangeByScoreAsync(retryKey, 0, now, take: batchSize);

        var states = new List<SagaState>();
        foreach (var member in members)
        {
            var state = await GetAsync(member!, cancellationToken);
            if (state != null)
            {
                states.Add(state);
            }
        }

        return states;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string sagaId, CancellationToken cancellationToken = default)
    {
        var state = await GetAsync(sagaId, cancellationToken);
        var key = GetKey(sagaId);
        var db = Database;

        await db.KeyDeleteAsync(key);

        // Clean up indices
        if (state != null)
        {
            await db.SetRemoveAsync(GetIndexKey(state.Status.ToString()), sagaId);
        }

        await db.SortedSetRemoveAsync(GetRetryIndexKey(), sagaId);
    }
}
