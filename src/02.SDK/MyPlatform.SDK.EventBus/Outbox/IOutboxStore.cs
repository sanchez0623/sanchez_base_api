namespace MyPlatform.SDK.EventBus.Outbox;

/// <summary>
/// Interface for outbox message storage.
/// </summary>
public interface IOutboxStore
{
    /// <summary>
    /// Saves an outbox message.
    /// </summary>
    /// <param name="message">The message to save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SaveAsync(OutboxMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets unprocessed messages.
    /// </summary>
    /// <param name="batchSize">The maximum number of messages to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of unprocessed messages.</returns>
    Task<IEnumerable<OutboxMessage>> GetUnprocessedAsync(int batchSize = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a message as processed.
    /// </summary>
    /// <param name="messageId">The message identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task MarkAsProcessedAsync(Guid messageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a message as failed.
    /// </summary>
    /// <param name="messageId">The message identifier.</param>
    /// <param name="error">The error message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task MarkAsFailedAsync(Guid messageId, string error, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes processed messages older than the specified date.
    /// </summary>
    /// <param name="olderThan">The cutoff date.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of deleted messages.</returns>
    Task<int> DeleteProcessedAsync(DateTime olderThan, CancellationToken cancellationToken = default);
}
