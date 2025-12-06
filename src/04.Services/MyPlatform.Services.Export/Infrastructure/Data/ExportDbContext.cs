using Microsoft.EntityFrameworkCore;
using MyPlatform.SDK.DataExchange.Jobs;

namespace MyPlatform.Services.Export.Infrastructure.Data;

/// <summary>
/// 导出服务数据库上下文
/// </summary>
public class ExportDbContext : DbContext
{
    public ExportDbContext(DbContextOptions<ExportDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// 导出作业表
    /// </summary>
    public DbSet<DataExchangeJob> ExportJobs => Set<DataExchangeJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DataExchangeJob>(entity =>
        {
            entity.ToTable("export_jobs");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Type).HasColumnName("type").HasConversion<string>();
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(e => e.SourceFileUrl).HasColumnName("source_file_url").HasMaxLength(1024);
            entity.Property(e => e.ResultFileUrl).HasColumnName("result_file_url").HasMaxLength(1024);
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message").HasMaxLength(2048);
            entity.Property(e => e.ProcessedCount).HasColumnName("processed_count");
            entity.Property(e => e.TotalCount).HasColumnName("total_count");
            entity.Property(e => e.InitiatedBy).HasColumnName("initiated_by").HasMaxLength(128);
            entity.Property(e => e.Version).HasColumnName("version").IsConcurrencyToken();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");

            // 索引
            entity.HasIndex(e => e.Status).HasDatabaseName("ix_export_jobs_status");
            entity.HasIndex(e => e.InitiatedBy).HasDatabaseName("ix_export_jobs_initiated_by");
            entity.HasIndex(e => e.CreatedAt).HasDatabaseName("ix_export_jobs_created_at");
        });
    }
}
