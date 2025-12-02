using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace MyPlatform.SDK.Search.Elasticsearch.HealthChecks;

/// <summary>
/// Health check for Elasticsearch cluster.
/// </summary>
public class ElasticsearchHealthCheck : IHealthCheck
{
    private readonly ElasticsearchClient _client;
    private readonly ILogger<ElasticsearchHealthCheck> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElasticsearchHealthCheck"/> class.
    /// </summary>
    /// <param name="client">The Elasticsearch client.</param>
    /// <param name="logger">The logger.</param>
    public ElasticsearchHealthCheck(ElasticsearchClient client, ILogger<ElasticsearchHealthCheck> logger)
    {
        _client = client;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.Cluster.HealthAsync(cancellationToken);

            if (!response.IsValidResponse)
            {
                _logger.LogWarning("Elasticsearch health check failed: {DebugInformation}", response.DebugInformation);
                return HealthCheckResult.Unhealthy("Elasticsearch cluster is not responding", data: new Dictionary<string, object>
                {
                    ["DebugInformation"] = response.DebugInformation ?? "Unknown error"
                });
            }

            var data = new Dictionary<string, object>
            {
                ["ClusterName"] = response.ClusterName ?? "Unknown",
                ["Status"] = response.Status.ToString(),
                ["NumberOfNodes"] = response.NumberOfNodes,
                ["ActiveShards"] = response.ActiveShards,
                ["ActivePrimaryShards"] = response.ActivePrimaryShards,
                ["RelocatingShards"] = response.RelocatingShards,
                ["InitializingShards"] = response.InitializingShards,
                ["UnassignedShards"] = response.UnassignedShards
            };

            return response.Status switch
            {
                Elastic.Clients.Elasticsearch.HealthStatus.Green => HealthCheckResult.Healthy("Elasticsearch cluster is healthy", data),
                Elastic.Clients.Elasticsearch.HealthStatus.Yellow => HealthCheckResult.Degraded("Elasticsearch cluster is in yellow status", data: data),
                Elastic.Clients.Elasticsearch.HealthStatus.Red => HealthCheckResult.Unhealthy("Elasticsearch cluster is in red status", data: data),
                _ => HealthCheckResult.Unhealthy("Unknown Elasticsearch cluster status", data: data)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Elasticsearch health check failed with exception");
            return HealthCheckResult.Unhealthy("Elasticsearch health check failed", ex);
        }
    }
}
