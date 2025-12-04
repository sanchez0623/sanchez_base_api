using Microsoft.EntityFrameworkCore;

namespace MyPlatform.SDK.Saga.Persistence;

/// <summary>
/// DbContext for saga state persistence.
/// </summary>
public class SagaDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SagaDbContext"/> class.
    /// </summary>
    /// <param name="options">The DbContext options.</param>
    public SagaDbContext(DbContextOptions<SagaDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the saga states DbSet.
    /// </summary>
    public DbSet<SagaStateEntity> SagaStates => Set<SagaStateEntity>();

    /// <summary>
    /// Configures the entity model.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SagaStateEntity>(entity =>
        {
            entity.ToTable("saga_states");
            entity.HasKey(e => e.SagaId);
            entity.Property(e => e.SagaId).HasMaxLength(50);
            entity.Property(e => e.SagaType).HasMaxLength(255).IsRequired();
            entity.Property(e => e.State).HasMaxLength(20).IsRequired();
            entity.Property(e => e.TenantId).HasMaxLength(50);
            entity.Property(e => e.CorrelationId).HasMaxLength(100);
            
            entity.HasIndex(e => e.State);
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.NextRetryAt);
        });
    }
}
