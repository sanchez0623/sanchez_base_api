namespace MyPlatform.SDK.Search.Elasticsearch.Abstractions;

/// <summary>
/// Base interface for search documents.
/// </summary>
public interface ISearchDocument
{
    /// <summary>
    /// Gets the unique identifier for the document.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets or sets the timestamp when the document was indexed.
    /// </summary>
    DateTime IndexedAt { get; set; }
}
