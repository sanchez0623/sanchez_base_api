using System.Linq.Expressions;
using MyPlatform.SDK.Search.Elasticsearch.Abstractions;

namespace MyPlatform.SDK.Search.Elasticsearch.Builders;

/// <summary>
/// Fluent query builder for constructing Elasticsearch queries.
/// </summary>
/// <typeparam name="TDocument">The type of document.</typeparam>
public class SearchQueryBuilder<TDocument> where TDocument : class, ISearchDocument
{
    private readonly SearchQuery<TDocument> _query = new();

    private SearchQueryBuilder()
    {
    }

    /// <summary>
    /// Creates a new instance of the query builder.
    /// </summary>
    /// <returns>A new query builder instance.</returns>
    public static SearchQueryBuilder<TDocument> Create() => new();

    /// <summary>
    /// Adds a term (exact match) condition.
    /// </summary>
    /// <typeparam name="TValue">The type of value.</typeparam>
    /// <param name="fieldSelector">The field selector expression.</param>
    /// <param name="value">The value to match.</param>
    /// <returns>The query builder for chaining.</returns>
    public SearchQueryBuilder<TDocument> WithTerm<TValue>(Expression<Func<TDocument, TValue>> fieldSelector, TValue value)
    {
        _query.AddCondition(new QueryCondition
        {
            Field = GetFieldName(fieldSelector),
            Type = QueryType.Term,
            Value = value
        });
        return this;
    }

    /// <summary>
    /// Adds a terms (multiple exact match) condition.
    /// </summary>
    /// <typeparam name="TValue">The type of values.</typeparam>
    /// <param name="fieldSelector">The field selector expression.</param>
    /// <param name="values">The values to match.</param>
    /// <returns>The query builder for chaining.</returns>
    public SearchQueryBuilder<TDocument> WithTerms<TValue>(Expression<Func<TDocument, TValue>> fieldSelector, IEnumerable<TValue> values)
    {
        _query.AddCondition(new QueryCondition
        {
            Field = GetFieldName(fieldSelector),
            Type = QueryType.Terms,
            Value = values.Cast<object>().ToArray()
        });
        return this;
    }

    /// <summary>
    /// Adds a match (full-text search) condition.
    /// </summary>
    /// <param name="fieldSelector">The field selector expression.</param>
    /// <param name="text">The text to search for.</param>
    /// <returns>The query builder for chaining.</returns>
    public SearchQueryBuilder<TDocument> WithMatch(Expression<Func<TDocument, string>> fieldSelector, string text)
    {
        _query.AddCondition(new QueryCondition
        {
            Field = GetFieldName(fieldSelector),
            Type = QueryType.Match,
            Value = text
        });
        return this;
    }

    /// <summary>
    /// Adds a range condition for numeric or date values.
    /// </summary>
    /// <typeparam name="TValue">The type of value.</typeparam>
    /// <param name="fieldSelector">The field selector expression.</param>
    /// <param name="min">The minimum value (inclusive).</param>
    /// <param name="max">The maximum value (inclusive).</param>
    /// <returns>The query builder for chaining.</returns>
    public SearchQueryBuilder<TDocument> WithRange<TValue>(Expression<Func<TDocument, TValue>> fieldSelector, TValue? min = default, TValue? max = default)
    {
        _query.AddCondition(new QueryCondition
        {
            Field = GetFieldName(fieldSelector),
            Type = QueryType.Range,
            MinValue = min,
            MaxValue = max
        });
        return this;
    }

    /// <summary>
    /// Adds a date range condition.
    /// </summary>
    /// <param name="fieldSelector">The field selector expression.</param>
    /// <param name="from">The start date (inclusive).</param>
    /// <param name="to">The end date (inclusive).</param>
    /// <returns>The query builder for chaining.</returns>
    public SearchQueryBuilder<TDocument> WithDateRange(Expression<Func<TDocument, DateTime>> fieldSelector, DateTime? from = null, DateTime? to = null)
    {
        _query.AddCondition(new QueryCondition
        {
            Field = GetFieldName(fieldSelector),
            Type = QueryType.Range,
            MinValue = from,
            MaxValue = to
        });
        return this;
    }

    /// <summary>
    /// Adds a full-text search across multiple fields.
    /// </summary>
    /// <param name="text">The text to search for.</param>
    /// <param name="fieldSelectors">The field selectors.</param>
    /// <returns>The query builder for chaining.</returns>
    public SearchQueryBuilder<TDocument> WithFullText(string text, params Expression<Func<TDocument, object>>[] fieldSelectors)
    {
        var fields = fieldSelectors.Select(GetFieldName).ToArray();
        _query.AddCondition(new QueryCondition
        {
            Field = fields.FirstOrDefault() ?? string.Empty,
            Type = QueryType.MultiMatch,
            Value = text,
            AdditionalFields = fields.Skip(1).ToArray()
        });
        return this;
    }

    /// <summary>
    /// Adds a nested query.
    /// </summary>
    /// <param name="path">The nested path.</param>
    /// <param name="nestedBuilder">The nested query builder action.</param>
    /// <returns>The query builder for chaining.</returns>
    public SearchQueryBuilder<TDocument> WithNested(string path, Action<NestedQueryBuilder> nestedBuilder)
    {
        var builder = new NestedQueryBuilder();
        nestedBuilder(builder);

        _query.AddCondition(new QueryCondition
        {
            Type = QueryType.Nested,
            NestedPath = path,
            NestedConditions = builder.GetConditions()
        });
        return this;
    }

    /// <summary>
    /// Adds an exists condition.
    /// </summary>
    /// <param name="fieldSelector">The field selector expression.</param>
    /// <returns>The query builder for chaining.</returns>
    public SearchQueryBuilder<TDocument> WithExists<TValue>(Expression<Func<TDocument, TValue>> fieldSelector)
    {
        _query.AddCondition(new QueryCondition
        {
            Field = GetFieldName(fieldSelector),
            Type = QueryType.Exists
        });
        return this;
    }

    /// <summary>
    /// Adds a prefix condition.
    /// </summary>
    /// <param name="fieldSelector">The field selector expression.</param>
    /// <param name="prefix">The prefix to match.</param>
    /// <returns>The query builder for chaining.</returns>
    public SearchQueryBuilder<TDocument> WithPrefix(Expression<Func<TDocument, string>> fieldSelector, string prefix)
    {
        _query.AddCondition(new QueryCondition
        {
            Field = GetFieldName(fieldSelector),
            Type = QueryType.Prefix,
            Value = prefix
        });
        return this;
    }

    /// <summary>
    /// Adds a wildcard condition.
    /// </summary>
    /// <param name="fieldSelector">The field selector expression.</param>
    /// <param name="pattern">The wildcard pattern.</param>
    /// <returns>The query builder for chaining.</returns>
    public SearchQueryBuilder<TDocument> WithWildcard(Expression<Func<TDocument, string>> fieldSelector, string pattern)
    {
        _query.AddCondition(new QueryCondition
        {
            Field = GetFieldName(fieldSelector),
            Type = QueryType.Wildcard,
            Value = pattern
        });
        return this;
    }

    /// <summary>
    /// Adds ascending sort order.
    /// </summary>
    /// <typeparam name="TValue">The type of value.</typeparam>
    /// <param name="fieldSelector">The field selector expression.</param>
    /// <returns>The query builder for chaining.</returns>
    public SearchQueryBuilder<TDocument> OrderBy<TValue>(Expression<Func<TDocument, TValue>> fieldSelector)
    {
        _query.AddSortSpecification(new SortSpecification
        {
            Field = GetFieldName(fieldSelector),
            Descending = false
        });
        return this;
    }

    /// <summary>
    /// Adds descending sort order.
    /// </summary>
    /// <typeparam name="TValue">The type of value.</typeparam>
    /// <param name="fieldSelector">The field selector expression.</param>
    /// <returns>The query builder for chaining.</returns>
    public SearchQueryBuilder<TDocument> OrderByDescending<TValue>(Expression<Func<TDocument, TValue>> fieldSelector)
    {
        _query.AddSortSpecification(new SortSpecification
        {
            Field = GetFieldName(fieldSelector),
            Descending = true
        });
        return this;
    }

    /// <summary>
    /// Configures pagination.
    /// </summary>
    /// <param name="pageIndex">The page index (1-based).</param>
    /// <param name="pageSize">The page size.</param>
    /// <returns>The query builder for chaining.</returns>
    public SearchQueryBuilder<TDocument> WithPaging(int pageIndex = 1, int pageSize = 20)
    {
        _query.PageIndex = Math.Max(1, pageIndex);
        _query.PageSize = Math.Max(1, Math.Min(10000, pageSize));
        return this;
    }

    /// <summary>
    /// Adds highlight configuration for specified fields.
    /// </summary>
    /// <param name="fieldSelectors">The field selectors to highlight.</param>
    /// <returns>The query builder for chaining.</returns>
    public SearchQueryBuilder<TDocument> WithHighlight(params Expression<Func<TDocument, object>>[] fieldSelectors)
    {
        var fields = fieldSelectors.Select(GetFieldName);
        _query.AddHighlightFields(fields);
        return this;
    }

    /// <summary>
    /// Builds the search query.
    /// </summary>
    /// <returns>The built search query.</returns>
    public ISearchQuery<TDocument> Build() => _query;

    private static string GetFieldName<TValue>(Expression<Func<TDocument, TValue>> fieldSelector)
    {
        return fieldSelector.Body switch
        {
            MemberExpression memberExpression => ToCamelCase(memberExpression.Member.Name),
            UnaryExpression { Operand: MemberExpression unaryMemberExpression } => ToCamelCase(unaryMemberExpression.Member.Name),
            _ => throw new ArgumentException("Invalid field selector expression", nameof(fieldSelector))
        };
    }

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        return char.ToLowerInvariant(name[0]) + name[1..];
    }
}

/// <summary>
/// Builder for nested queries.
/// </summary>
public class NestedQueryBuilder
{
    private readonly List<QueryCondition> _conditions = new();

    /// <summary>
    /// Adds a term condition to the nested query.
    /// </summary>
    /// <typeparam name="TValue">The type of value.</typeparam>
    /// <param name="field">The field name.</param>
    /// <param name="value">The value to match.</param>
    /// <returns>The nested query builder for chaining.</returns>
    public NestedQueryBuilder WithTerm<TValue>(string field, TValue value)
    {
        _conditions.Add(new QueryCondition
        {
            Field = field,
            Type = QueryType.Term,
            Value = value
        });
        return this;
    }

    /// <summary>
    /// Adds a match condition to the nested query.
    /// </summary>
    /// <param name="field">The field name.</param>
    /// <param name="text">The text to search for.</param>
    /// <returns>The nested query builder for chaining.</returns>
    public NestedQueryBuilder WithMatch(string field, string text)
    {
        _conditions.Add(new QueryCondition
        {
            Field = field,
            Type = QueryType.Match,
            Value = text
        });
        return this;
    }

    /// <summary>
    /// Adds a range condition to the nested query.
    /// </summary>
    /// <typeparam name="TValue">The type of value.</typeparam>
    /// <param name="field">The field name.</param>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <returns>The nested query builder for chaining.</returns>
    public NestedQueryBuilder WithRange<TValue>(string field, TValue? min = default, TValue? max = default)
    {
        _conditions.Add(new QueryCondition
        {
            Field = field,
            Type = QueryType.Range,
            MinValue = min,
            MaxValue = max
        });
        return this;
    }

    /// <summary>
    /// Gets the conditions built by this builder.
    /// </summary>
    /// <returns>The list of conditions.</returns>
    internal IReadOnlyList<QueryCondition> GetConditions() => _conditions;
}
