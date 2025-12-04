using MyPlatform.SDK.MultiTenancy.Models;

namespace MyPlatform.SDK.MultiTenancy.Configuration;

/// <summary>
/// Configuration options for multi-tenancy.
/// </summary>
public class MultiTenancyOptions
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "MultiTenancy";

    /// <summary>
    /// Gets or sets a value indicating whether multi-tenancy is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the shared database connection string.
    /// Used when tenant IsolationMode is set to Shared.
    /// </summary>
    public string DefaultConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the database provider (e.g., MySQL, SqlServer, PostgreSQL).
    /// This is used by tenant-aware DbContext factories to configure the appropriate provider.
    /// </summary>
    public string DatabaseProvider { get; set; } = "MySQL";

    /// <summary>
    /// Gets or sets the tenant store type.
    /// Built-in options: "Configuration" (for development), "InMemory" (for testing).
    /// For production environments, it is recommended to implement a custom ITenantStore
    /// that loads tenant information from a database and register it using AddTenantStore&lt;TStore&gt;().
    /// </summary>
    public string TenantStore { get; set; } = "Configuration";

    /// <summary>
    /// Gets or sets a value indicating whether to cache tenant information.
    /// When enabled, the CachedTenantStoreDecorator wraps the ITenantStore implementation.
    /// </summary>
    public bool CacheTenantInfo { get; set; } = true;

    /// <summary>
    /// Gets or sets the cache expiration time in minutes.
    /// Only applicable when CacheTenantInfo is true.
    /// </summary>
    public int CacheExpirationMinutes { get; set; } = 5;

    /// <summary>
    /// Gets or sets the list of configured tenants.
    /// Only used when TenantStore is set to "Configuration".
    /// Note: This configuration is intended for development and testing environments only.
    /// For production environments, implement a custom ITenantStore to load tenants from a database.
    /// </summary>
    public List<TenantInfo> Tenants { get; set; } = new();
}
