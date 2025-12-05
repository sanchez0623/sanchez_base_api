using Microsoft.Extensions.Options;
using MyPlatform.SDK.MultiTenancy.Configuration;
using MyPlatform.SDK.MultiTenancy.Models;

namespace MyPlatform.SDK.MultiTenancy.Store;

/// <summary>
/// Configuration-based implementation of tenant store.
/// Loads tenant information from appsettings.json.
/// </summary>
/// <remarks>
/// This implementation is intended for development and testing environments only.
/// For production environments, implement a custom ITenantStore that loads tenant
/// information from a database (e.g., MySqlTenantStore) and register it using
/// <see cref="Extensions.ServiceCollectionExtensions.AddTenantStore{TStore}"/>.
/// </remarks>
public class ConfigurationTenantStore : ITenantStore
{
    private readonly IOptionsMonitor<MultiTenancyOptions> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationTenantStore"/> class.
    /// </summary>
    /// <param name="options">The multi-tenancy options.</param>
    public ConfigurationTenantStore(IOptionsMonitor<MultiTenancyOptions> options)
    {
        _options = options;
    }

    /// <inheritdoc />
    public Task<TenantInfo?> GetTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = _options.CurrentValue.Tenants
            .FirstOrDefault(t => t.TenantId.Equals(tenantId, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(tenant);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<TenantInfo>> GetAllTenantsAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TenantInfo> tenants = _options.CurrentValue.Tenants.ToList();
        return Task.FromResult(tenants);
    }

    /// <inheritdoc />
    /// <remarks>
    /// This operation is not supported by ConfigurationTenantStore as configuration is read-only.
    /// For production use cases requiring tenant management, implement a custom ITenantStore.
    /// </remarks>
    public Task SaveTenantAsync(TenantInfo tenant, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("ConfigurationTenantStore does not support saving tenants. Use a database-backed ITenantStore implementation for production environments.");
    }

    /// <inheritdoc />
    /// <remarks>
    /// This operation is not supported by ConfigurationTenantStore as configuration is read-only.
    /// For production use cases requiring tenant management, implement a custom ITenantStore.
    /// </remarks>
    public Task DeleteTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("ConfigurationTenantStore does not support deleting tenants. Use a database-backed ITenantStore implementation for production environments.");
    }
}
