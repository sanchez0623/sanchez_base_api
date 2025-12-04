namespace MyPlatform.SDK.MultiTenancy.Exceptions;

/// <summary>
/// Exception thrown when a tenant cannot be found.
/// </summary>
public class TenantNotFoundException : Exception
{
    /// <summary>
    /// Gets the tenant identifier that was not found.
    /// </summary>
    public string TenantId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantNotFoundException"/> class.
    /// </summary>
    /// <param name="tenantId">The tenant identifier that was not found.</param>
    public TenantNotFoundException(string tenantId)
        : base($"Tenant not found: {tenantId}")
    {
        TenantId = tenantId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantNotFoundException"/> class.
    /// </summary>
    /// <param name="tenantId">The tenant identifier that was not found.</param>
    /// <param name="innerException">The inner exception.</param>
    public TenantNotFoundException(string tenantId, Exception innerException)
        : base($"Tenant not found: {tenantId}", innerException)
    {
        TenantId = tenantId;
    }
}
