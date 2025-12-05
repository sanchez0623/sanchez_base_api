using Microsoft.EntityFrameworkCore;
using MyPlatform.Services.Sku.Infrastructure.MultiTenancy.Entities;

namespace MyPlatform.Services.Sku.Infrastructure.MultiTenancy;

/// <summary>
/// DbContext for tenant management.
/// </summary>
/// <remarks>
/// This is an example implementation demonstrating how to store tenant
/// information in a MySQL database. This context is separate from your
/// business data context and is used for managing tenant configuration.
/// 
/// For production use, consider:
/// - Adding indexes for TenantId lookups
/// - Implementing audit trails
/// - Adding tenant metadata tables
/// </remarks>
public class TenantManagementDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TenantManagementDbContext"/> class.
    /// </summary>
    /// <param name="options">The DbContext options.</param>
    public TenantManagementDbContext(DbContextOptions<TenantManagementDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the tenants table.
    /// </summary>
    public DbSet<TenantEntity> Tenants { get; set; } = null!;

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TenantEntity>(entity =>
        {
            entity.ToTable("tenants");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.TenantId)
                .HasColumnName("tenant_id")
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(e => e.IsolationMode)
                .HasColumnName("isolation_mode")
                .HasMaxLength(32)
                .IsRequired()
                .HasDefaultValue("Shared");

            entity.Property(e => e.ConnectionString)
                .HasColumnName("connection_string")
                .HasMaxLength(1024);

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasMaxLength(32)
                .IsRequired()
                .HasDefaultValue("Active");

            entity.Property(e => e.Configuration)
                .HasColumnName("configuration")
                .HasColumnType("text");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            entity.Property(e => e.DeletedAt)
                .HasColumnName("deleted_at");

            // Add unique index on tenant_id
            entity.HasIndex(e => e.TenantId)
                .IsUnique()
                .HasDatabaseName("ix_tenants_tenant_id");

            // Add index on status for filtering
            entity.HasIndex(e => e.Status)
                .HasDatabaseName("ix_tenants_status");
        });
    }
}
