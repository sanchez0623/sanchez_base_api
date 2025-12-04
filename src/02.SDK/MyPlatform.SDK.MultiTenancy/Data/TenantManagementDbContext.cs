using Microsoft.EntityFrameworkCore;
using MyPlatform.SDK.MultiTenancy.Data.Entities;

namespace MyPlatform.SDK.MultiTenancy.Data;

/// <summary>
/// Database context for managing tenant information.
/// This context is used to store and retrieve tenant data from the database.
/// </summary>
public class TenantManagementDbContext : DbContext
{
    /// <summary>
    /// Gets or sets the tenants DbSet.
    /// </summary>
    public DbSet<TenantEntity> Tenants => Set<TenantEntity>();

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantManagementDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by the DbContext.</param>
    public TenantManagementDbContext(DbContextOptions<TenantManagementDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Configures the model for the tenant management database.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TenantEntity>(entity =>
        {
            entity.ToTable("tenants");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TenantId).IsUnique();
            entity.HasIndex(e => e.Status);

            entity.Property(e => e.TenantId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.IsolationMode).HasMaxLength(20).IsRequired();
            entity.Property(e => e.ConnectionString).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
        });
    }
}
