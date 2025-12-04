namespace MyPlatform.SDK.MultiTenancy.Models;

/// <summary>
/// Defines the data isolation mode for a tenant.
/// </summary>
public enum TenantIsolationMode
{
    /// <summary>
    /// Tenant shares a database with other tenants.
    /// </summary>
    Shared,

    /// <summary>
    /// Tenant has an isolated, dedicated database.
    /// </summary>
    Isolated
}
