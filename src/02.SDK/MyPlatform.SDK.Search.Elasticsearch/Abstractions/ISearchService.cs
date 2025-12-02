namespace MyPlatform.SDK.Search.Elasticsearch.Abstractions;

/// <summary>
/// Interface for search services.
/// </summary>
/// <typeparam name="TDocument">The type of document to search.</typeparam>
public interface ISearchService<TDocument> where TDocument : class, ISearchDocument
{
    /// <summary>
    /// Searches for documents matching the specified query.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Search results.</returns>
    Task<SearchResult<TDocument>> SearchAsync(ISearchQuery<TDocument> query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a document by its unique identifier.
    /// </summary>
    /// <param name="id">The document identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The document if found, null otherwise.</returns>
    Task<TDocument?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Indexes a single document.
    /// </summary>
    /// <param name="document">The document to index.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task IndexAsync(TDocument document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Indexes multiple documents in bulk.
    /// </summary>
    /// <param name="documents">The documents to index.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task IndexManyAsync(IEnumerable<TDocument> documents, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a document by its unique identifier.
    /// </summary>
    /// <param name="id">The document identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes documents matching the specified query.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteByQueryAsync(ISearchQuery<TDocument> query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts documents matching the specified query.
    /// </summary>
    /// <param name="query">The search query (optional).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The count of matching documents.</returns>
    Task<long> CountAsync(ISearchQuery<TDocument>? query = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents search results.
/// </summary>
/// <typeparam name="TDocument">The type of document.</typeparam>
public class SearchResult<TDocument> where TDocument : class, ISearchDocument
{
    /// <summary>
    /// Gets or sets the list of matching documents.
    /// </summary>
    public IReadOnlyList<TDocument> Documents { get; set; } = Array.Empty<TDocument>();

    /// <summary>
    /// Gets or sets the total number of matching documents.
    /// </summary>
    public long Total { get; set; }

    /// <summary>
    /// Gets or sets the highlights for matching fields.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyList<string>>> Highlights { get; set; } 
        = new Dictionary<string, IReadOnlyDictionary<string, IReadOnlyList<string>>>();

    /// <summary>
    /// Gets or sets the time taken for the search in milliseconds.
    /// </summary>
    public long Took { get; set; }
}
