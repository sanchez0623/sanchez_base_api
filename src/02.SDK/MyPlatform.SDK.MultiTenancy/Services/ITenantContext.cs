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

    /// <summary>
    /// Sets the current tenant.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="tenantName">The tenant name.</param>
    /// <param name="connectionString">The tenant connection string.</param>
    public void SetTenant(string tenantId, string? tenantName = null, string? connectionString = null)
    {
        TenantId = tenantId;
        TenantName = tenantName;
        ConnectionString = connectionString;
    }

    /// <summary>
    /// Clears the current tenant.
    /// </summary>
    public void ClearTenant()
    {
        TenantId = null;
        TenantName = null;
        ConnectionString = null;
    }
}
