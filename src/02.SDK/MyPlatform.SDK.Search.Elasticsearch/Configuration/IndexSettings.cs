namespace MyPlatform.SDK.Search.Elasticsearch.Configuration;

/// <summary>
/// Index-specific settings.
/// </summary>
public class IndexSettings
{
    /// <summary>
    /// Gets or sets the index name.
    /// </summary>
    public string IndexName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of shards (overrides default).
    /// </summary>
    public int? NumberOfShards { get; set; }

    /// <summary>
    /// Gets or sets the number of replicas (overrides default).
    /// </summary>
    public int? NumberOfReplicas { get; set; }

    /// <summary>
    /// Gets or sets the refresh interval (e.g., "1s", "30s").
    /// </summary>
    public string RefreshInterval { get; set; } = "1s";

    /// <summary>
    /// Gets or sets the maximum result window for pagination.
    /// </summary>
    public int MaxResultWindow { get; set; } = 10000;
}
