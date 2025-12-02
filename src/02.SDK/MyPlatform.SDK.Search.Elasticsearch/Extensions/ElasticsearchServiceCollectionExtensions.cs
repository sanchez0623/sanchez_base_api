using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyPlatform.SDK.Search.Elasticsearch.Abstractions;
using MyPlatform.SDK.Search.Elasticsearch.Configuration;
using MyPlatform.SDK.Search.Elasticsearch.HealthChecks;
using MyPlatform.SDK.Search.Elasticsearch.Services;

namespace MyPlatform.SDK.Search.Elasticsearch.Extensions;

/// <summary>
/// Extension methods for registering Elasticsearch services.
/// </summary>
public static class ElasticsearchServiceCollectionExtensions
{
    /// <summary>
    /// Adds Elasticsearch services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="configureOptions">Optional action to configure options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPlatformElasticsearch(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<ElasticsearchOptions>? configureOptions = null)
    {
        // Configure options
        services.Configure<ElasticsearchOptions>(configuration.GetSection(ElasticsearchOptions.SectionName));

        if (configureOptions != null)
        {
            services.PostConfigure(configureOptions);
        }

        // Register Elasticsearch client
        services.TryAddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<ElasticsearchOptions>>().Value;
            var logger = sp.GetRequiredService<ILogger<ElasticsearchClient>>();

            return CreateElasticsearchClient(options, logger);
        });

        // Register index management service
        services.TryAddSingleton<IIndexService, IndexManagementService>();

        return services;
    }

    /// <summary>
    /// Adds a search service for a specific document type.
    /// </summary>
    /// <typeparam name="TDocument">The document type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="indexName">The index name (optional, uses default if not specified).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSearchService<TDocument>(
        this IServiceCollection services,
        string? indexName = null)
        where TDocument : class, ISearchDocument
    {
        services.AddSingleton<ISearchService<TDocument>>(sp =>
        {
            var client = sp.GetRequiredService<ElasticsearchClient>();
            var options = sp.GetRequiredService<IOptions<ElasticsearchOptions>>();
            var logger = sp.GetRequiredService<ILogger<ElasticsearchService<TDocument>>>();

            return new ElasticsearchService<TDocument>(client, options, logger, indexName);
        });

        return services;
    }

    /// <summary>
    /// Adds Elasticsearch health check.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="name">The name of the health check (default: "elasticsearch").</param>
    /// <param name="tags">Health check tags.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddElasticsearchHealthCheck(
        this IServiceCollection services,
        string name = "elasticsearch",
        params string[] tags)
    {
        services.AddHealthChecks()
            .Add(new Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckRegistration(
                name,
                sp => new ElasticsearchHealthCheck(
                    sp.GetRequiredService<ElasticsearchClient>(),
                    sp.GetRequiredService<ILogger<ElasticsearchHealthCheck>>()),
                failureStatus: null,
                tags: tags));

        return services;
    }

    private static ElasticsearchClient CreateElasticsearchClient(ElasticsearchOptions options, ILogger logger)
    {
        var nodes = options.Nodes.Select(n => new Uri(n)).ToArray();

        ElasticsearchClientSettings settings;

        if (nodes.Length == 1)
        {
            settings = new ElasticsearchClientSettings(nodes[0]);
        }
        else
        {
            var pool = new StaticNodePool(nodes);
            settings = new ElasticsearchClientSettings(pool);
        }

        settings.DefaultIndex(options.DefaultIndex);
        settings.RequestTimeout(options.RequestTimeout);

        // Configure authentication
        if (!string.IsNullOrEmpty(options.ApiKey))
        {
            settings.Authentication(new ApiKey(options.ApiKey));
        }
        else if (!string.IsNullOrEmpty(options.Username) && !string.IsNullOrEmpty(options.Password))
        {
            settings.Authentication(new BasicAuthentication(options.Username, options.Password));
        }

        // Configure certificate fingerprint
        if (!string.IsNullOrEmpty(options.CertificateFingerprint))
        {
            settings.CertificateFingerprint(options.CertificateFingerprint);
        }

        // Configure debug mode
        if (options.EnableDebugMode)
        {
            settings.EnableDebugMode();
            settings.PrettyJson();
        }

        logger.LogInformation("Creating Elasticsearch client with {NodeCount} node(s)", nodes.Length);

        return new ElasticsearchClient(settings);
    }
}
