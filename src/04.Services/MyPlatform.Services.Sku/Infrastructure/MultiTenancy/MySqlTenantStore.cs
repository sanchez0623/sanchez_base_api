using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyPlatform.SDK.MultiTenancy.Models;
using MyPlatform.SDK.MultiTenancy.Store;
using MyPlatform.Services.Sku.Infrastructure.MultiTenancy.Entities;

namespace MyPlatform.Services.Sku.Infrastructure.MultiTenancy;

/// <summary>
/// MySQL-based implementation of ITenantStore.
/// </summary>
/// <remarks>
/// This is an example implementation demonstrating how to load tenant information
/// from a MySQL database. This implementation can be used as a reference for
/// implementing your own database-backed tenant store.
/// 
/// Key features:
/// - Loads tenant information from the tenants table
/// - Supports creating, updating, and soft-deleting tenants
/// - Uses IServiceProvider to create scoped DbContext instances
/// 
/// Registration example:
/// <code>
/// // Register the tenant management database context
/// services.AddDbContext&lt;TenantManagementDbContext&gt;(options =>
///     options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
/// 
/// // Register multi-tenancy with custom store
/// services.AddPlatformMultiTenancy(configuration)
///         .AddTenantStore&lt;MySqlTenantStore&gt;();
/// </code>
/// </remarks>
public class MySqlTenantStore : ITenantStore
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MySqlTenantStore> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MySqlTenantStore"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider for creating scoped DbContext instances.</param>
    /// <param name="logger">The logger.</param>
    public MySqlTenantStore(
        IServiceProvider serviceProvider,
        ILogger<MySqlTenantStore> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<TenantInfo?> GetTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TenantManagementDbContext>();

        var entity = await dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Status != "Deleted", cancellationToken);

        if (entity == null)
        {
            _logger.LogDebug("Tenant not found: {TenantId}", tenantId);
            return null;
        }

        return MapToTenantInfo(entity);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TenantInfo>> GetAllTenantsAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TenantManagementDbContext>();

        var entities = await dbContext.Tenants
            .AsNoTracking()
            .Where(t => t.Status != "Deleted")
            .ToListAsync(cancellationToken);

        return entities.Select(MapToTenantInfo).ToList();
    }

    /// <inheritdoc />
    public async Task SaveTenantAsync(TenantInfo tenant, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TenantManagementDbContext>();

        var entity = await dbContext.Tenants
            .FirstOrDefaultAsync(t => t.TenantId == tenant.TenantId, cancellationToken);

        if (entity == null)
        {
            entity = new TenantEntity
            {
                TenantId = tenant.TenantId,
                CreatedAt = DateTime.UtcNow
            };
            dbContext.Tenants.Add(entity);
        }

        entity.Name = tenant.Name;
        entity.IsolationMode = tenant.IsolationMode.ToString();
        entity.ConnectionString = tenant.ConnectionString;
        entity.Status = tenant.Status.ToString();
        entity.Configuration = tenant.Configuration != null && tenant.Configuration.Count > 0
            ? JsonSerializer.Serialize(tenant.Configuration)
            : null;
        entity.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Tenant saved: {TenantId}", tenant.TenantId);
    }

    /// <inheritdoc />
    public async Task DeleteTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TenantManagementDbContext>();

        var entity = await dbContext.Tenants
            .FirstOrDefaultAsync(t => t.TenantId == tenantId, cancellationToken);

        if (entity != null)
        {
            // Soft delete
            entity.Status = "Deleted";
            entity.DeletedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Tenant deleted: {TenantId}", tenantId);
        }
    }

    /// <summary>
    /// Maps a TenantEntity to a TenantInfo model.
    /// </summary>
    /// <param name="entity">The entity to map.</param>
    /// <returns>The mapped TenantInfo.</returns>
    private static TenantInfo MapToTenantInfo(TenantEntity entity)
    {
        return new TenantInfo
        {
            TenantId = entity.TenantId,
            Name = entity.Name,
            IsolationMode = Enum.Parse<TenantIsolationMode>(entity.IsolationMode),
            ConnectionString = entity.ConnectionString,
            Status = Enum.Parse<TenantStatus>(entity.Status),
            Configuration = string.IsNullOrEmpty(entity.Configuration)
                ? new Dictionary<string, string>()
                : JsonSerializer.Deserialize<Dictionary<string, string>>(entity.Configuration) ?? new Dictionary<string, string>(),
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
