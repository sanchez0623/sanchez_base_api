using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyPlatform.SDK.MultiTenancy.Data;
using MyPlatform.SDK.MultiTenancy.Data.Entities;
using MyPlatform.SDK.MultiTenancy.Models;

namespace MyPlatform.SDK.MultiTenancy.Store;

/// <summary>
/// Database-based implementation of tenant store for production environments.
/// Loads and manages tenant information from a database.
/// </summary>
public class DatabaseTenantStore : ITenantStore
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseTenantStore> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseTenantStore"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider for creating scoped services.</param>
    /// <param name="logger">The logger instance.</param>
    public DatabaseTenantStore(
        IServiceProvider serviceProvider,
        ILogger<DatabaseTenantStore> logger)
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
    /// Maps a TenantEntity to a TenantInfo object.
    /// </summary>
    /// <param name="entity">The entity to map.</param>
    /// <returns>The mapped TenantInfo object.</returns>
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
