using System.Linq.Expressions;

namespace MyPlatform.SDK.Search.Elasticsearch.Abstractions;

/// <summary>
/// Interface for building search queries.
/// </summary>
/// <typeparam name="TDocument">The type of document.</typeparam>
public interface ISearchQuery<TDocument> where TDocument : class, ISearchDocument
{
    /// <summary>
    /// Gets the list of query conditions.
    /// </summary>
    IReadOnlyList<QueryCondition> Conditions { get; }

    /// <summary>
    /// Gets the sort specifications.
    /// </summary>
    IReadOnlyList<SortSpecification> SortSpecifications { get; }

    /// <summary>
    /// Gets the page index (1-based).
    /// </summary>
    int PageIndex { get; }

    /// <summary>
    /// Gets the page size.
    /// </summary>
    int PageSize { get; }

    /// <summary>
    /// Gets the fields to highlight.
    /// </summary>
    IReadOnlyList<string> HighlightFields { get; }
}

/// <summary>
/// Represents a query condition.
/// </summary>
public class QueryCondition
{
    /// <summary>
    /// Gets or sets the field name.
    /// </summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the query type.
    /// </summary>
    public QueryType Type { get; set; }

    /// <summary>
    /// Gets or sets the query value.
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// Gets or sets the minimum value for range queries.
    /// </summary>
    public object? MinValue { get; set; }

    /// <summary>
    /// Gets or sets the maximum value for range queries.
    /// </summary>
    public object? MaxValue { get; set; }

    /// <summary>
    /// Gets or sets additional fields for multi-field queries.
    /// </summary>
    public IReadOnlyList<string> AdditionalFields { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets nested conditions for nested queries.
    /// </summary>
    public IReadOnlyList<QueryCondition> NestedConditions { get; set; } = Array.Empty<QueryCondition>();

    /// <summary>
    /// Gets or sets the nested path for nested queries.
    /// </summary>
    public string? NestedPath { get; set; }
}

/// <summary>
/// Query types supported by the search query builder.
/// </summary>
public enum QueryType
{
    /// <summary>
    /// Exact term match.
    /// </summary>
    Term,

    /// <summary>
    /// Multiple exact term matches.
    /// </summary>
    Terms,

    /// <summary>
    /// Full-text match.
    /// </summary>
    Match,

    /// <summary>
    /// Multi-field full-text match.
    /// </summary>
    MultiMatch,

    /// <summary>
    /// Range query for numeric or date values.
    /// </summary>
    Range,

    /// <summary>
    /// Prefix query.
    /// </summary>
    Prefix,

    /// <summary>
    /// Wildcard query.
    /// </summary>
    Wildcard,

    /// <summary>
    /// Nested query.
    /// </summary>
    Nested,

    /// <summary>
    /// Exists query.
    /// </summary>
    Exists
}

/// <summary>
/// Represents a sort specification.
/// </summary>
public class SortSpecification
{
    /// <summary>
    /// Gets or sets the field to sort by.
    /// </summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether to sort in descending order.
    /// </summary>
    public bool Descending { get; set; }
}
