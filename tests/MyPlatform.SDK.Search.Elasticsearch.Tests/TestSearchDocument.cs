using MyPlatform.SDK.Search.Elasticsearch.Documents;

namespace MyPlatform.SDK.Search.Elasticsearch.Tests;

/// <summary>
/// Test search document for unit tests.
/// </summary>
public class TestSearchDocument : SearchDocumentBase
{
    public string DocumentId { get; set; } = string.Empty;

    public override string Id => DocumentId;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<long> CategoryIds { get; set; } = new();
}
