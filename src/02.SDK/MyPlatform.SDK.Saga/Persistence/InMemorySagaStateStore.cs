using System.Collections.Concurrent;
using MyPlatform.SDK.Saga.Models;

namespace MyPlatform.SDK.Saga.Persistence;

/// <summary>
/// In-memory implementation of saga state store for development and testing.
/// </summary>
public class InMemorySagaStateStore : ISagaStateStore
{
    private readonly ConcurrentDictionary<string, SagaState> _states = new();

    /// <inheritdoc />
    public Task SaveAsync(SagaState state, CancellationToken cancellationToken = default)
    {
        state.UpdatedAt = DateTime.UtcNow;
        _states.AddOrUpdate(state.SagaId, state, (_, _) => state);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<SagaState?> GetAsync(string sagaId, CancellationToken cancellationToken = default)
    {
        _states.TryGetValue(sagaId, out var state);
        return Task.FromResult(state);
    }

    /// <inheritdoc />
    public Task<IEnumerable<SagaState>> GetByStatusAsync(SagaStatus status, CancellationToken cancellationToken = default)
    {
        var states = _states.Values.Where(s => s.Status == status);
        return Task.FromResult(states);
    }

    /// <inheritdoc />
    public Task<IEnumerable<SagaState>> GetPendingRetriesAsync(int batchSize = 100, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var states = _states.Values
            .Where(s => s.NextRetryAt.HasValue && s.NextRetryAt <= now)
            .Take(batchSize);
        return Task.FromResult(states);
    }

    /// <inheritdoc />
    public Task DeleteAsync(string sagaId, CancellationToken cancellationToken = default)
    {
        _states.TryRemove(sagaId, out _);
        return Task.CompletedTask;
    }
}
