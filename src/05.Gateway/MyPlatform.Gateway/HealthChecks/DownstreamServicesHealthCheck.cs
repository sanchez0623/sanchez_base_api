using Microsoft.Extensions.Diagnostics.HealthChecks;
using Yarp.ReverseProxy.Configuration;

namespace MyPlatform.Gateway.HealthChecks;

/// <summary>
/// Health check that verifies connectivity to downstream services configured in YARP.
/// </summary>
public class DownstreamServicesHealthCheck : IHealthCheck
{
    private readonly IProxyConfigProvider _proxyConfigProvider;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DownstreamServicesHealthCheck> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DownstreamServicesHealthCheck"/> class.
    /// </summary>
    /// <param name="proxyConfigProvider">The YARP proxy configuration provider.</param>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="logger">The logger instance.</param>
    public DownstreamServicesHealthCheck(
        IProxyConfigProvider proxyConfigProvider,
        IHttpClientFactory httpClientFactory,
        ILogger<DownstreamServicesHealthCheck> logger)
    {
        _proxyConfigProvider = proxyConfigProvider;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Performs the health check by calling the /health endpoint of each downstream service.
    /// </summary>
    /// <param name="context">The health check context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the health check result.</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var config = _proxyConfigProvider.GetConfig();
        var clusters = config.Clusters;

        if (!clusters.Any())
        {
            return HealthCheckResult.Healthy("No downstream services configured");
        }

        var results = new Dictionary<string, object>();
        var unhealthyServices = new List<string>();
        var degradedServices = new List<string>();

        using var httpClient = _httpClientFactory.CreateClient();
        httpClient.Timeout = TimeSpan.FromSeconds(5);

        foreach (var cluster in clusters)
        {
            var clusterName = cluster.ClusterId;
            var destinations = cluster.Destinations;

            if (destinations == null || !destinations.Any())
            {
                results[$"{clusterName}"] = "No destinations configured";
                continue;
            }

            var healthyDestinations = 0;
            var totalDestinations = destinations.Count;

            foreach (var destination in destinations)
            {
                var destinationName = destination.Key;
                var address = destination.Value.Address;

                try
                {
                    // Validate and construct health endpoint URL
                    if (!Uri.TryCreate(address.TrimEnd('/'), UriKind.Absolute, out var baseUri))
                    {
                        results[$"{clusterName}/{destinationName}"] = $"Invalid address: {address}";
                        _logger.LogWarning("Downstream service {Cluster}/{Destination} has invalid address: {Address}", 
                            clusterName, destinationName, address);
                        continue;
                    }

                    var healthUrl = new Uri(baseUri, "/health");
                    var response = await httpClient.GetAsync(healthUrl, cancellationToken);

                    if (response.IsSuccessStatusCode)
                    {
                        healthyDestinations++;
                        results[$"{clusterName}/{destinationName}"] = "Healthy";
                        _logger.LogDebug("Downstream service {Cluster}/{Destination} is healthy", clusterName, destinationName);
                    }
                    else
                    {
                        results[$"{clusterName}/{destinationName}"] = $"Unhealthy (Status: {(int)response.StatusCode})";
                        _logger.LogWarning("Downstream service {Cluster}/{Destination} returned status {StatusCode}", 
                            clusterName, destinationName, (int)response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    results[$"{clusterName}/{destinationName}"] = $"Error: {ex.Message}";
                    _logger.LogWarning(ex, "Failed to check health of downstream service {Cluster}/{Destination}", 
                        clusterName, destinationName);
                }
            }

            // Determine cluster health status
            if (healthyDestinations == 0)
            {
                unhealthyServices.Add(clusterName);
            }
            else if (healthyDestinations < totalDestinations)
            {
                degradedServices.Add(clusterName);
            }
        }

        // Determine overall health status
        if (unhealthyServices.Any())
        {
            return HealthCheckResult.Unhealthy(
                $"Unhealthy clusters: {string.Join(", ", unhealthyServices)}",
                data: results);
        }

        if (degradedServices.Any())
        {
            return HealthCheckResult.Degraded(
                $"Degraded clusters: {string.Join(", ", degradedServices)}",
                data: results);
        }

        return HealthCheckResult.Healthy("All downstream services are healthy", data: results);
    }
}
