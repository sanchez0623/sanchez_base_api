using Microsoft.EntityFrameworkCore;

namespace MyPlatform.SDK.MultiTenancy.DataSource;

/// <summary>
/// Factory interface for creating tenant-specific DbContext instances.
/// </summary>
/// <typeparam name="TContext">The type of DbContext.</typeparam>
public interface ITenantDbContextFactory<TContext> where TContext : DbContext
{
    /// <summary>
    /// Creates a new DbContext for the current tenant.
    /// </summary>
    /// <returns>A new DbContext instance configured for the current tenant.</returns>
    TContext CreateDbContext();

    /// <summary>
    /// Creates a new DbContext for the current tenant asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A new DbContext instance configured for the current tenant.</returns>
    Task<TContext> CreateDbContextAsync(CancellationToken cancellationToken = default);
}
