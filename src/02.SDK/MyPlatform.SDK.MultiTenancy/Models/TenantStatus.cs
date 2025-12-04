namespace MyPlatform.SDK.MultiTenancy.Models;

/// <summary>
/// Defines the status of a tenant.
/// </summary>
public enum TenantStatus
{
    /// <summary>
    /// Tenant is active and can access the system.
    /// </summary>
    Active,

    /// <summary>
    /// Tenant is suspended and cannot access the system.
    /// </summary>
    Suspended,

    /// <summary>
    /// Tenant has been deleted and cannot access the system.
    /// </summary>
    Deleted
}
