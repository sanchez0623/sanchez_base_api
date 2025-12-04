using MyPlatform.SDK.MultiTenancy.Models;

namespace MyPlatform.SDK.MultiTenancy.Store;

/// <summary>
/// Interface for storing and retrieving tenant information.
/// </summary>
public interface ITenantStore
{
    /// <summary>
    /// Gets tenant information by tenant identifier.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The tenant information if found; otherwise, null.</returns>
    Task<TenantInfo?> GetTenantAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all tenants from the store.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A read-only list of all tenants.</returns>
    Task<IReadOnlyList<TenantInfo>> GetAllTenantsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves or updates tenant information.
    /// </summary>
    /// <param name="tenant">The tenant information to save.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task SaveTenantAsync(TenantInfo tenant, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a tenant from the store.
    /// </summary>
    /// <param name="tenantId">The tenant identifier to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task DeleteTenantAsync(string tenantId, CancellationToken cancellationToken = default);
}
