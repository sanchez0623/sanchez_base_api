namespace MyPlatform.SDK.MultiTenancy.Exceptions;

/// <summary>
/// Exception thrown when a tenant is suspended and cannot access the system.
/// </summary>
public class TenantSuspendedException : Exception
{
    /// <summary>
    /// Gets the tenant identifier that is suspended.
    /// </summary>
    public string TenantId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantSuspendedException"/> class.
    /// </summary>
    /// <param name="tenantId">The tenant identifier that is suspended.</param>
    public TenantSuspendedException(string tenantId)
        : base($"Tenant is suspended: {tenantId}")
    {
        TenantId = tenantId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantSuspendedException"/> class.
    /// </summary>
    /// <param name="tenantId">The tenant identifier that is suspended.</param>
    /// <param name="innerException">The inner exception.</param>
    public TenantSuspendedException(string tenantId, Exception innerException)
        : base($"Tenant is suspended: {tenantId}", innerException)
    {
        TenantId = tenantId;
    }
}
