using System.Collections.Concurrent;
using MyPlatform.SDK.MultiTenancy.Models;

namespace MyPlatform.SDK.MultiTenancy.Store;

/// <summary>
/// In-memory implementation of tenant store for development and testing.
/// </summary>
public class InMemoryTenantStore : ITenantStore
{
    private readonly ConcurrentDictionary<string, TenantInfo> _tenants = new();

    /// <inheritdoc />
    public Task<TenantInfo?> GetTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        _tenants.TryGetValue(tenantId, out var tenant);
        return Task.FromResult(tenant);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<TenantInfo>> GetAllTenantsAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TenantInfo> tenants = _tenants.Values.ToList();
        return Task.FromResult(tenants);
    }

    /// <inheritdoc />
    public Task SaveTenantAsync(TenantInfo tenant, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(tenant.TenantId))
        {
            throw new ArgumentException("TenantId cannot be null or empty.", nameof(tenant));
        }

        tenant.UpdatedAt = DateTime.UtcNow;
        if (tenant.CreatedAt == default)
        {
            tenant.CreatedAt = DateTime.UtcNow;
        }

        _tenants.AddOrUpdate(tenant.TenantId, tenant, (_, _) => tenant);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        _tenants.TryRemove(tenantId, out _);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Adds a tenant to the store for testing purposes.
    /// </summary>
    /// <param name="tenant">The tenant to add.</param>
    public void AddTenant(TenantInfo tenant)
    {
        if (string.IsNullOrEmpty(tenant.TenantId))
        {
            throw new ArgumentException("TenantId cannot be null or empty.", nameof(tenant));
        }

        if (tenant.CreatedAt == default)
        {
            tenant.CreatedAt = DateTime.UtcNow;
        }

        _tenants[tenant.TenantId] = tenant;
    }

    /// <summary>
    /// Clears all tenants from the store.
    /// </summary>
    public void Clear()
    {
        _tenants.Clear();
    }
}
