using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyPlatform.SDK.Search.Elasticsearch.Abstractions;
using MyPlatform.SDK.Search.Elasticsearch.Configuration;

namespace MyPlatform.SDK.Search.Elasticsearch.Services;

/// <summary>
/// Elasticsearch search service implementation.
/// </summary>
/// <typeparam name="TDocument">The type of document.</typeparam>
public class ElasticsearchService<TDocument> : ISearchService<TDocument>
    where TDocument : class, ISearchDocument
{
    private readonly ElasticsearchClient _client;
    private readonly ILogger<ElasticsearchService<TDocument>> _logger;
    private readonly string _indexName;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElasticsearchService{TDocument}"/> class.
    /// </summary>
    /// <param name="client">The Elasticsearch client.</param>
    /// <param name="options">The Elasticsearch options.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="indexName">The index name (optional, overrides default).</param>
    public ElasticsearchService(
        ElasticsearchClient client,
        IOptions<ElasticsearchOptions> options,
        ILogger<ElasticsearchService<TDocument>> logger,
        string? indexName = null)
    {
        _client = client;
        _logger = logger;
        _indexName = indexName ?? options.Value.DefaultIndex;
    }

    /// <inheritdoc />
    public async Task<SearchResult<TDocument>> SearchAsync(ISearchQuery<TDocument> query, CancellationToken cancellationToken = default)
    {
        var searchDescriptor = new SearchRequestDescriptor<TDocument>()
            .Index(_indexName)
            .From((query.PageIndex - 1) * query.PageSize)
            .Size(query.PageSize);

        // Build query
        var boolQuery = BuildBoolQuery(query.Conditions);
        if (boolQuery != null)
        {
            searchDescriptor.Query(q => q.Bool(boolQuery));
        }

        // Build sorting
        if (query.SortSpecifications.Count > 0)
        {
            searchDescriptor.Sort(sortDescriptor =>
            {
                foreach (var sort in query.SortSpecifications)
                {
                    sortDescriptor.Field(new Field(sort.Field), s => s.Order(sort.Descending ? SortOrder.Desc : SortOrder.Asc));
                }
            });
        }

        // Note: Highlight implementation skipped due to Elastic.Clients.Elasticsearch 8.x API complexity.
        // Highlight fields are collected in the query object but not applied to the search request.
        // Users needing highlighting can extend this class or use the ElasticsearchClient directly.

        var response = await _client.SearchAsync(searchDescriptor, cancellationToken);

        if (!response.IsValidResponse)
        {
            _logger.LogError("Search failed: {DebugInformation}", response.DebugInformation);
            throw new InvalidOperationException($"Search failed: {response.DebugInformation}");
        }

        var highlights = new Dictionary<string, IReadOnlyDictionary<string, IReadOnlyList<string>>>();
        if (response.Hits != null)
        {
            foreach (var hit in response.Hits)
            {
                if (hit.Highlight != null && hit.Id != null)
                {
                    var hitHighlights = hit.Highlight.ToDictionary(
                        kvp => kvp.Key,
                        kvp => (IReadOnlyList<string>)kvp.Value.ToList());
                    highlights[hit.Id] = hitHighlights;
                }
            }
        }

        return new SearchResult<TDocument>
        {
            Documents = response.Documents.ToList(),
            Total = response.Total,
            Took = response.Took,
            Highlights = highlights
        };
    }

    /// <inheritdoc />
    public async Task<TDocument?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var response = await _client.GetAsync<TDocument>(id, g => g.Index(_indexName), cancellationToken);

        if (!response.IsValidResponse)
        {
            if (response.ApiCallDetails?.HttpStatusCode == 404)
            {
                return null;
            }

            _logger.LogError("Get by ID failed: {DebugInformation}", response.DebugInformation);
            throw new InvalidOperationException($"Get by ID failed: {response.DebugInformation}");
        }

        return response.Source;
    }

    /// <inheritdoc />
    public async Task IndexAsync(TDocument document, CancellationToken cancellationToken = default)
    {
        document.IndexedAt = DateTime.UtcNow;

        var response = await _client.IndexAsync(document, i => i.Index(_indexName).Id(document.Id), cancellationToken);

        if (!response.IsValidResponse)
        {
            _logger.LogError("Index failed: {DebugInformation}", response.DebugInformation);
            throw new InvalidOperationException($"Index failed: {response.DebugInformation}");
        }
    }

    /// <inheritdoc />
    public async Task IndexManyAsync(IEnumerable<TDocument> documents, CancellationToken cancellationToken = default)
    {
        var docs = documents.ToList();
        if (docs.Count == 0)
        {
            return;
        }

        foreach (var doc in docs)
        {
            doc.IndexedAt = DateTime.UtcNow;
        }

        var response = await _client.BulkAsync(b => b
            .Index(_indexName)
            .IndexMany(docs, (d, doc) => d.Id(doc.Id)), cancellationToken);

        if (!response.IsValidResponse)
        {
            _logger.LogError("Bulk index failed: {DebugInformation}", response.DebugInformation);
            throw new InvalidOperationException($"Bulk index failed: {response.DebugInformation}");
        }

        if (response.Errors)
        {
            var errors = response.ItemsWithErrors.Select(item => item.Error?.Reason).ToList();
            _logger.LogError("Bulk index had errors: {Errors}", string.Join(", ", errors));
            throw new InvalidOperationException($"Bulk index had errors: {string.Join(", ", errors)}");
        }
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var response = await _client.DeleteAsync<TDocument>(id, d => d.Index(_indexName), cancellationToken);

        if (!response.IsValidResponse && response.ApiCallDetails?.HttpStatusCode != 404)
        {
            _logger.LogError("Delete failed: {DebugInformation}", response.DebugInformation);
            throw new InvalidOperationException($"Delete failed: {response.DebugInformation}");
        }
    }

    /// <inheritdoc />
    public async Task DeleteByQueryAsync(ISearchQuery<TDocument> query, CancellationToken cancellationToken = default)
    {
        var boolQuery = BuildBoolQuery(query.Conditions);
        if (boolQuery == null)
        {
            return;
        }

        var response = await _client.DeleteByQueryAsync<TDocument>(d => d
            .Indices(_indexName)
            .Query(q => q.Bool(boolQuery)), cancellationToken);

        if (!response.IsValidResponse)
        {
            _logger.LogError("Delete by query failed: {DebugInformation}", response.DebugInformation);
            throw new InvalidOperationException($"Delete by query failed: {response.DebugInformation}");
        }
    }

    /// <inheritdoc />
    public async Task<long> CountAsync(ISearchQuery<TDocument>? query = null, CancellationToken cancellationToken = default)
    {
        CountResponse response;

        if (query == null || query.Conditions.Count == 0)
        {
            response = await _client.CountAsync<TDocument>(c => c.Indices(_indexName), cancellationToken);
        }
        else
        {
            var boolQuery = BuildBoolQuery(query.Conditions);
            response = await _client.CountAsync<TDocument>(c => c
                .Indices(_indexName)
                .Query(q => q.Bool(boolQuery!)), cancellationToken);
        }

        if (!response.IsValidResponse)
        {
            _logger.LogError("Count failed: {DebugInformation}", response.DebugInformation);
            throw new InvalidOperationException($"Count failed: {response.DebugInformation}");
        }

        return response.Count;
    }

    private static Action<BoolQueryDescriptor<TDocument>>? BuildBoolQuery(IReadOnlyList<QueryCondition> conditions)
    {
        if (conditions.Count == 0)
        {
            return null;
        }

        return b =>
        {
            foreach (var condition in conditions)
            {
                b.Must(BuildQuery(condition));
            }
        };
    }

    private static Action<QueryDescriptor<TDocument>> BuildQuery(QueryCondition condition)
    {
        return condition.Type switch
        {
            QueryType.Term => q => q.Term(t => t.Field(new Field(condition.Field)).Value(FieldValue.String(condition.Value?.ToString() ?? string.Empty))),
            QueryType.Terms => q => q.Terms(t => t.Field(new Field(condition.Field)).Term(new TermsQueryField(((object[])condition.Value!).Select(v => FieldValue.String(v?.ToString() ?? string.Empty)).ToArray()))),
            QueryType.Match => q => q.Match(m => m.Field(new Field(condition.Field)).Query(condition.Value?.ToString() ?? string.Empty)),
            QueryType.MultiMatch => BuildMultiMatchQuery(condition),
            QueryType.Range => BuildRangeQuery(condition),
            QueryType.Prefix => q => q.Prefix(p => p.Field(new Field(condition.Field)).Value(condition.Value?.ToString() ?? string.Empty)),
            QueryType.Wildcard => q => q.Wildcard(w => w.Field(new Field(condition.Field)).Value(condition.Value?.ToString() ?? string.Empty)),
            QueryType.Exists => q => q.Exists(e => e.Field(new Field(condition.Field))),
            QueryType.Nested => BuildNestedQuery(condition),
            _ => throw new NotSupportedException($"Query type {condition.Type} is not supported")
        };
    }

    private static Action<QueryDescriptor<TDocument>> BuildMultiMatchQuery(QueryCondition condition)
    {
        var fields = new List<string> { condition.Field };
        fields.AddRange(condition.AdditionalFields);

        return q => q.MultiMatch(m => m.Fields(Fields.FromStrings(fields.ToArray())).Query(condition.Value?.ToString() ?? string.Empty));
    }

    private static Action<QueryDescriptor<TDocument>> BuildRangeQuery(QueryCondition condition)
    {
        // Check if it's a date range query
        if (condition.MinValue is DateTime || condition.MaxValue is DateTime)
        {
            return q => q.Range(r => r
                .DateRange(dr =>
                {
                    dr.Field(new Field(condition.Field));
                    if (condition.MinValue is DateTime minDate)
                    {
                        dr.Gte(minDate);
                    }
                    if (condition.MaxValue is DateTime maxDate)
                    {
                        dr.Lte(maxDate);
                    }
                }));
        }

        // Handle numeric range query
        return q => q.Range(r => r
            .NumberRange(nr =>
            {
                nr.Field(new Field(condition.Field));
                if (condition.MinValue != null)
                {
                    nr.Gte(Convert.ToDouble(condition.MinValue));
                }
                if (condition.MaxValue != null)
                {
                    nr.Lte(Convert.ToDouble(condition.MaxValue));
                }
            }));
    }

    private static Action<QueryDescriptor<TDocument>> BuildNestedQuery(QueryCondition condition)
    {
        return q => q.Nested(n =>
        {
            n.Path(condition.NestedPath!);
            if (condition.NestedConditions.Count > 0)
            {
                n.Query(nq =>
                {
                    nq.Bool(b =>
                    {
                        foreach (var nestedCondition in condition.NestedConditions)
                        {
                            b.Must(BuildQuery(nestedCondition));
                        }
                    });
                });
            }
        });
    }
}
