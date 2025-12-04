using MyPlatform.SDK.MultiTenancy.Models;

namespace MyPlatform.SDK.MultiTenancy.DataSource;

/// <summary>
/// Interface for resolving connection strings based on tenant information.
/// </summary>
public interface ITenantConnectionStringResolver
{
    /// <summary>
    /// Gets the connection string for the specified tenant.
    /// </summary>
    /// <param name="tenant">The tenant information.</param>
    /// <returns>The connection string for the tenant.</returns>
    string GetConnectionString(TenantInfo tenant);

    /// <summary>
    /// Gets the connection string for the specified tenant identifier.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <returns>The connection string for the tenant.</returns>
    string GetConnectionString(string tenantId);

    /// <summary>
    /// Gets the connection string for the specified tenant identifier asynchronously.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The connection string for the tenant.</returns>
    Task<string> GetConnectionStringAsync(string tenantId, CancellationToken cancellationToken = default);
}
