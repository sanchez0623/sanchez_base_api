using MyPlatform.SDK.MultiTenancy.Models;

namespace MyPlatform.SDK.MultiTenancy.Services;

/// <summary>
/// Represents the current tenant context.
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// Gets the current tenant identifier.
    /// </summary>
    string? TenantId { get; }

    /// <summary>
    /// Gets the current tenant name.
    /// </summary>
    string? TenantName { get; }

    /// <summary>
    /// Gets a value indicating whether a tenant is currently set.
    /// </summary>
    bool HasTenant { get; }

    /// <summary>
    /// Gets the tenant connection string if applicable.
    /// </summary>
    string? ConnectionString { get; }

    /// <summary>
    /// Gets the current tenant information.
    /// </summary>
    TenantInfo? CurrentTenant { get; }

    /// <summary>
    /// Sets the current tenant by tenant identifier.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    void SetTenant(string tenantId);

    /// <summary>
    /// Sets the current tenant with full tenant information.
    /// </summary>
    /// <param name="tenant">The tenant information.</param>
    void SetTenant(TenantInfo tenant);
}

/// <summary>
/// Default implementation of tenant context.
/// </summary>
public class TenantContext : ITenantContext
{
    /// <inheritdoc />
    public string? TenantId { get; private set; }

    /// <inheritdoc />
    public string? TenantName { get; private set; }

    /// <inheritdoc />
    public bool HasTenant => !string.IsNullOrEmpty(TenantId);

    /// <inheritdoc />
    public string? ConnectionString { get; private set; }

    /// <inheritdoc />
    public TenantInfo? CurrentTenant { get; private set; }

    /// <inheritdoc />
    public void SetTenant(string tenantId)
    {
        TenantId = tenantId;
        TenantName = null;
        ConnectionString = null;
        CurrentTenant = null;
    }

    /// <inheritdoc />
    public void SetTenant(TenantInfo tenant)
    {
        if (tenant == null)
        {
            throw new ArgumentNullException(nameof(tenant));
        }

        TenantId = tenant.TenantId;
        TenantName = tenant.Name;
        ConnectionString = tenant.ConnectionString;
        CurrentTenant = tenant;
    }

    /// <summary>
    /// Sets the current tenant with optional name and connection string.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="tenantName">The tenant name.</param>
    /// <param name="connectionString">The tenant connection string.</param>
    public void SetTenant(string tenantId, string? tenantName, string? connectionString)
    {
        TenantId = tenantId;
        TenantName = tenantName;
        ConnectionString = connectionString;
        CurrentTenant = null;
    }

    /// <summary>
    /// Clears the current tenant.
    /// </summary>
    public void ClearTenant()
    {
        TenantId = null;
        TenantName = null;
        ConnectionString = null;
        CurrentTenant = null;
    }
}
