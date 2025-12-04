using Microsoft.Extensions.Options;
using MyPlatform.SDK.MultiTenancy.Configuration;
using MyPlatform.SDK.MultiTenancy.Exceptions;
using MyPlatform.SDK.MultiTenancy.Models;
using MyPlatform.SDK.MultiTenancy.Store;

namespace MyPlatform.SDK.MultiTenancy.DataSource;

/// <summary>
/// Resolves connection strings based on tenant isolation mode.
/// </summary>
public class TenantConnectionStringResolver : ITenantConnectionStringResolver
{
    private readonly ITenantStore _tenantStore;
    private readonly MultiTenancyOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantConnectionStringResolver"/> class.
    /// </summary>
    /// <param name="tenantStore">The tenant store.</param>
    /// <param name="options">The multi-tenancy options.</param>
    public TenantConnectionStringResolver(
        ITenantStore tenantStore,
        IOptions<MultiTenancyOptions> options)
    {
        _tenantStore = tenantStore;
        _options = options.Value;
    }

    /// <inheritdoc />
    public string GetConnectionString(TenantInfo tenant)
    {
        if (tenant == null)
        {
            throw new ArgumentNullException(nameof(tenant));
        }

        return tenant.IsolationMode switch
        {
            TenantIsolationMode.Isolated => tenant.ConnectionString
                ?? throw new InvalidOperationException($"Tenant {tenant.TenantId} is configured for isolated mode but has no connection string."),
            TenantIsolationMode.Shared => _options.DefaultConnectionString,
            _ => throw new ArgumentOutOfRangeException(nameof(tenant), tenant.IsolationMode, "Unknown isolation mode.")
        };
    }

    /// <inheritdoc />
    public string GetConnectionString(string tenantId)
    {
        if (string.IsNullOrEmpty(tenantId))
        {
            throw new ArgumentException("TenantId cannot be null or empty.", nameof(tenantId));
        }

        var tenant = _tenantStore.GetTenantAsync(tenantId).GetAwaiter().GetResult();

        if (tenant == null)
        {
            throw new TenantNotFoundException(tenantId);
        }

        return GetConnectionString(tenant);
    }
}
