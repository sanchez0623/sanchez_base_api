using MyPlatform.SDK.Search.Elasticsearch.Abstractions;

namespace MyPlatform.SDK.Search.Elasticsearch.Builders;

/// <summary>
/// Represents a search query.
/// </summary>
/// <typeparam name="TDocument">The type of document.</typeparam>
public class SearchQuery<TDocument> : ISearchQuery<TDocument> where TDocument : class, ISearchDocument
{
    private readonly List<QueryCondition> _conditions = new();
    private readonly List<SortSpecification> _sortSpecifications = new();
    private readonly List<string> _highlightFields = new();

    /// <summary>
    /// Gets the list of query conditions.
    /// </summary>
    public IReadOnlyList<QueryCondition> Conditions => _conditions;

    /// <summary>
    /// Gets the sort specifications.
    /// </summary>
    public IReadOnlyList<SortSpecification> SortSpecifications => _sortSpecifications;

    /// <summary>
    /// Gets the page index (1-based).
    /// </summary>
    public int PageIndex { get; internal set; } = 1;

    /// <summary>
    /// Gets the page size.
    /// </summary>
    public int PageSize { get; internal set; } = 20;

    /// <summary>
    /// Gets the fields to highlight.
    /// </summary>
    public IReadOnlyList<string> HighlightFields => _highlightFields;

    /// <summary>
    /// Adds a query condition.
    /// </summary>
    /// <param name="condition">The condition to add.</param>
    internal void AddCondition(QueryCondition condition) => _conditions.Add(condition);

    /// <summary>
    /// Adds a sort specification.
    /// </summary>
    /// <param name="specification">The sort specification to add.</param>
    internal void AddSortSpecification(SortSpecification specification) => _sortSpecifications.Add(specification);

    /// <summary>
    /// Adds fields to highlight.
    /// </summary>
    /// <param name="fields">The fields to highlight.</param>
    internal void AddHighlightFields(IEnumerable<string> fields) => _highlightFields.AddRange(fields);
}
