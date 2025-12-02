namespace MyPlatform.SDK.Search.Elasticsearch.Abstractions;

/// <summary>
/// Interface for handling index synchronization events.
/// </summary>
/// <typeparam name="TDocument">The document type.</typeparam>
/// <typeparam name="TEvent">The event type.</typeparam>
public interface IIndexSyncHandler<TDocument, TEvent>
    where TDocument : class, ISearchDocument
    where TEvent : class
{
    /// <summary>
    /// Handles an event by building and indexing the corresponding document.
    /// </summary>
    /// <param name="event">The event to handle.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds a document from an event.
    /// </summary>
    /// <param name="event">The event to build from.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The built document.</returns>
    Task<TDocument> BuildDocumentAsync(TEvent @event, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the document ID from an event.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <returns>The document ID.</returns>
    string GetDocumentId(TEvent @event);
}
