using Microsoft.Extensions.Logging;
using MyPlatform.SDK.Search.Elasticsearch.Abstractions;

namespace MyPlatform.SDK.Search.Elasticsearch.Services;

/// <summary>
/// Base class for index synchronization handlers.
/// </summary>
/// <typeparam name="TDocument">The document type.</typeparam>
/// <typeparam name="TEvent">The event type.</typeparam>
public abstract class IndexSyncHandlerBase<TDocument, TEvent> : IIndexSyncHandler<TDocument, TEvent>
    where TDocument : class, ISearchDocument
    where TEvent : class
{
    private readonly ISearchService<TDocument> _searchService;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexSyncHandlerBase{TDocument, TEvent}"/> class.
    /// </summary>
    /// <param name="searchService">The search service.</param>
    /// <param name="logger">The logger.</param>
    protected IndexSyncHandlerBase(
        ISearchService<TDocument> searchService,
        ILogger logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default)
    {
        try
        {
            var documentId = GetDocumentId(@event);
            _logger.LogInformation("Processing index sync for document {DocumentId}", documentId);

            var document = await BuildDocumentAsync(@event, cancellationToken);
            await _searchService.IndexAsync(document, cancellationToken);

            _logger.LogInformation("Successfully indexed document {DocumentId}", documentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync index for event {EventType}", typeof(TEvent).Name);
            throw;
        }
    }

    /// <inheritdoc />
    public abstract Task<TDocument> BuildDocumentAsync(TEvent @event, CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public abstract string GetDocumentId(TEvent @event);
}
