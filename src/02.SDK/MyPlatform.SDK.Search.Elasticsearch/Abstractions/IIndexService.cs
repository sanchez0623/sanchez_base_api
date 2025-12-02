namespace MyPlatform.SDK.Search.Elasticsearch.Abstractions;

/// <summary>
/// Interface for index management operations.
/// </summary>
public interface IIndexService
{
    /// <summary>
    /// Creates an index with the specified settings.
    /// </summary>
    /// <typeparam name="TDocument">The document type for the index.</typeparam>
    /// <param name="indexName">The name of the index to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task CreateIndexAsync<TDocument>(string indexName, CancellationToken cancellationToken = default)
        where TDocument : class, ISearchDocument;

    /// <summary>
    /// Checks if an index exists.
    /// </summary>
    /// <param name="indexName">The name of the index.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the index exists, false otherwise.</returns>
    Task<bool> IndexExistsAsync(string indexName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an index.
    /// </summary>
    /// <param name="indexName">The name of the index to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteIndexAsync(string indexName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an alias for an index.
    /// </summary>
    /// <param name="indexName">The name of the index.</param>
    /// <param name="aliasName">The alias name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task CreateAliasAsync(string indexName, string aliasName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes an index to make recently indexed documents available for search.
    /// </summary>
    /// <param name="indexName">The name of the index.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RefreshIndexAsync(string indexName, CancellationToken cancellationToken = default);
}
