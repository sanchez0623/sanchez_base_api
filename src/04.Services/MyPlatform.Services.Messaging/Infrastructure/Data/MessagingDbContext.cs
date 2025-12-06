using Microsoft.EntityFrameworkCore;
using MyPlatform.Services.Messaging.Domain.Entities;

namespace MyPlatform.Services.Messaging.Infrastructure.Data;

/// <summary>
/// 消息服务数据库上下文
/// </summary>
public class MessagingDbContext : DbContext
{
    public MessagingDbContext(DbContextOptions<MessagingDbContext> options) : base(options)
    {
    }

    public DbSet<MessagePublishRecord> PublishRecords => Set<MessagePublishRecord>();
    public DbSet<MessageConsumeRecord> ConsumeRecords => Set<MessageConsumeRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MessagePublishRecord>(entity =>
        {
            entity.ToTable("message_publish_records");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).HasMaxLength(256);
            entity.Property(e => e.EventId).HasMaxLength(64);
            entity.Property(e => e.Status).HasMaxLength(32);
            entity.HasIndex(e => e.EventId).IsUnique();
            entity.HasIndex(e => e.Status);
        });

        modelBuilder.Entity<MessageConsumeRecord>(entity =>
        {
            entity.ToTable("message_consume_records");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).HasMaxLength(256);
            entity.Property(e => e.EventId).HasMaxLength(64);
            entity.Property(e => e.ConsumerGroup).HasMaxLength(128);
            entity.HasIndex(e => new { e.EventId, e.ConsumerGroup }).IsUnique();
        });
    }
}
