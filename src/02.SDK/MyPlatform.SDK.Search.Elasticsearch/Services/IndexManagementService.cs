using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyPlatform.SDK.Search.Elasticsearch.Abstractions;
using MyPlatform.SDK.Search.Elasticsearch.Configuration;

namespace MyPlatform.SDK.Search.Elasticsearch.Services;

/// <summary>
/// Index management service implementation.
/// </summary>
public class IndexManagementService : IIndexService
{
    private readonly ElasticsearchClient _client;
    private readonly ElasticsearchOptions _options;
    private readonly ILogger<IndexManagementService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexManagementService"/> class.
    /// </summary>
    /// <param name="client">The Elasticsearch client.</param>
    /// <param name="options">The Elasticsearch options.</param>
    /// <param name="logger">The logger.</param>
    public IndexManagementService(
        ElasticsearchClient client,
        IOptions<ElasticsearchOptions> options,
        ILogger<IndexManagementService> logger)
    {
        _client = client;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task CreateIndexAsync<TDocument>(string indexName, CancellationToken cancellationToken = default)
        where TDocument : class, ISearchDocument
    {
        var exists = await IndexExistsAsync(indexName, cancellationToken);
        if (exists)
        {
            _logger.LogInformation("Index {IndexName} already exists", indexName);
            return;
        }

        var response = await _client.Indices.CreateAsync<TDocument>(indexName, c => c
            .Settings(s => s
                .NumberOfShards(_options.NumberOfShards)
                .NumberOfReplicas(_options.NumberOfReplicas)), cancellationToken);

        if (!response.IsValidResponse)
        {
            _logger.LogError("Failed to create index {IndexName}: {DebugInformation}", indexName, response.DebugInformation);
            throw new InvalidOperationException($"Failed to create index {indexName}: {response.DebugInformation}");
        }

        _logger.LogInformation("Created index {IndexName}", indexName);
    }

    /// <inheritdoc />
    public async Task<bool> IndexExistsAsync(string indexName, CancellationToken cancellationToken = default)
    {
        var response = await _client.Indices.ExistsAsync(indexName, cancellationToken);
        return response.Exists;
    }

    /// <inheritdoc />
    public async Task DeleteIndexAsync(string indexName, CancellationToken cancellationToken = default)
    {
        var exists = await IndexExistsAsync(indexName, cancellationToken);
        if (!exists)
        {
            _logger.LogInformation("Index {IndexName} does not exist", indexName);
            return;
        }

        var response = await _client.Indices.DeleteAsync(indexName, cancellationToken);

        if (!response.IsValidResponse)
        {
            _logger.LogError("Failed to delete index {IndexName}: {DebugInformation}", indexName, response.DebugInformation);
            throw new InvalidOperationException($"Failed to delete index {indexName}: {response.DebugInformation}");
        }

        _logger.LogInformation("Deleted index {IndexName}", indexName);
    }

    /// <inheritdoc />
    public async Task CreateAliasAsync(string indexName, string aliasName, CancellationToken cancellationToken = default)
    {
        var response = await _client.Indices.PutAliasAsync(indexName, aliasName, cancellationToken);

        if (!response.IsValidResponse)
        {
            _logger.LogError("Failed to create alias {AliasName} for index {IndexName}: {DebugInformation}",
                aliasName, indexName, response.DebugInformation);
            throw new InvalidOperationException($"Failed to create alias {aliasName}: {response.DebugInformation}");
        }

        _logger.LogInformation("Created alias {AliasName} for index {IndexName}", aliasName, indexName);
    }

    /// <inheritdoc />
    public async Task RefreshIndexAsync(string indexName, CancellationToken cancellationToken = default)
    {
        var response = await _client.Indices.RefreshAsync(indexName, cancellationToken);

        if (!response.IsValidResponse)
        {
            _logger.LogError("Failed to refresh index {IndexName}: {DebugInformation}", indexName, response.DebugInformation);
            throw new InvalidOperationException($"Failed to refresh index {indexName}: {response.DebugInformation}");
        }

        _logger.LogDebug("Refreshed index {IndexName}", indexName);
    }
}
