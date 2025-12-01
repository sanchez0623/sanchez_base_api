using System.Collections.Concurrent;

namespace MyPlatform.SDK.EventBus.Outbox;

/// <summary>
/// In-memory implementation of outbox store for development and testing.
/// </summary>
public class InMemoryOutboxStore : IOutboxStore
{
    private readonly ConcurrentDictionary<Guid, OutboxMessage> _messages = new();

    /// <inheritdoc />
    public Task SaveAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        _messages.TryAdd(message.Id, message);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IEnumerable<OutboxMessage>> GetUnprocessedAsync(int batchSize = 100, CancellationToken cancellationToken = default)
    {
        var messages = _messages.Values
            .Where(m => !m.IsProcessed)
            .OrderBy(m => m.CreatedAt)
            .Take(batchSize);

        return Task.FromResult(messages);
    }

    /// <inheritdoc />
    public Task MarkAsProcessedAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        if (_messages.TryGetValue(messageId, out var message))
        {
            message.IsProcessed = true;
            message.ProcessedAt = DateTime.UtcNow;
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task MarkAsFailedAsync(Guid messageId, string error, CancellationToken cancellationToken = default)
    {
        if (_messages.TryGetValue(messageId, out var message))
        {
            message.Error = error;
            message.RetryCount++;
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<int> DeleteProcessedAsync(DateTime olderThan, CancellationToken cancellationToken = default)
    {
        var toDelete = _messages.Values
            .Where(m => m.IsProcessed && m.ProcessedAt < olderThan)
            .Select(m => m.Id)
            .ToList();

        foreach (var id in toDelete)
        {
            _messages.TryRemove(id, out _);
        }

        return Task.FromResult(toDelete.Count);
    }
}
