namespace MyPlatform.SDK.Search.Elasticsearch.Configuration;

/// <summary>
/// Elasticsearch configuration options.
/// </summary>
public class ElasticsearchOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Elasticsearch";

    /// <summary>
    /// Gets or sets the Elasticsearch node URLs.
    /// </summary>
    public string[] Nodes { get; set; } = { "http://localhost:9200" };

    /// <summary>
    /// Gets or sets the username for authentication.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the password for authentication.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets the API key for authentication.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Gets or sets the certificate fingerprint for SSL verification.
    /// </summary>
    public string? CertificateFingerprint { get; set; }

    /// <summary>
    /// Gets or sets the default index name.
    /// </summary>
    public string DefaultIndex { get; set; } = "default";

    /// <summary>
    /// Gets or sets the number of shards for new indices.
    /// </summary>
    public int NumberOfShards { get; set; } = 3;

    /// <summary>
    /// Gets or sets the number of replicas for new indices.
    /// </summary>
    public int NumberOfReplicas { get; set; } = 1;

    /// <summary>
    /// Gets or sets a value indicating whether debug mode is enabled.
    /// </summary>
    public bool EnableDebugMode { get; set; } = false;

    /// <summary>
    /// Gets or sets the request timeout.
    /// </summary>
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);
}
