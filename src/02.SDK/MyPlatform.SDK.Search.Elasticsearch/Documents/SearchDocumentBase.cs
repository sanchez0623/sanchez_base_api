using MyPlatform.SDK.Search.Elasticsearch.Abstractions;

namespace MyPlatform.SDK.Search.Elasticsearch.Documents;

/// <summary>
/// Base class for search documents.
/// </summary>
public abstract class SearchDocumentBase : ISearchDocument
{
    /// <summary>
    /// Gets or sets the unique identifier for the document.
    /// </summary>
    public abstract string Id { get; }

    /// <summary>
    /// Gets or sets the timestamp when the document was indexed.
    /// </summary>
    public DateTime IndexedAt { get; set; } = DateTime.UtcNow;
}
