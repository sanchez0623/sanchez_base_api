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
    /// Gets or sets the default connection string used for shared databases.
    /// Used when IsolationMode is Shared.
    /// </summary>
    public string DefaultConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the connection string for the tenant management database.
    /// This database stores the tenants table.
    /// Required when TenantStore is set to "Database".
    /// </summary>
    public string TenantManagementConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the database provider (e.g., MySQL, SqlServer, PostgreSQL).
    /// </summary>
    public string DatabaseProvider { get; set; } = "MySQL";

    /// <summary>
    /// Gets or sets the tenant store type.
    /// Valid values: Database (production), Configuration (development), InMemory (testing).
    /// Defaults to Database for production environments.
    /// </summary>
    public string TenantStore { get; set; } = "Database";

    /// <summary>
    /// Gets or sets a value indicating whether to cache tenant information.
    /// </summary>
    public bool CacheTenantInfo { get; set; } = true;

    /// <summary>
    /// Gets or sets the cache expiration time in minutes.
    /// </summary>
    public int CacheExpirationMinutes { get; set; } = 5;

    /// <summary>
    /// Gets or sets the list of configured tenants.
    /// </summary>
    public List<TenantInfo> Tenants { get; set; } = new();
}
