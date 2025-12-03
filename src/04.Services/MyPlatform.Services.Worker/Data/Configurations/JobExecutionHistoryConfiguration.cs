using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyPlatform.Services.Worker.Models;

namespace MyPlatform.Services.Worker.Data.Configurations;

/// <summary>
/// 任务执行历史 EF Core 配置
/// </summary>
public class JobExecutionHistoryConfiguration : IEntityTypeConfiguration<JobExecutionHistory>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<JobExecutionHistory> builder)
    {
        builder.ToTable("job_execution_histories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.JobName)
            .HasColumnName("job_name")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.JobGroup)
            .HasColumnName("job_group")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.TriggerName)
            .HasColumnName("trigger_name")
            .HasMaxLength(200);

        builder.Property(x => x.TriggerGroup)
            .HasColumnName("trigger_group")
            .HasMaxLength(200);

        builder.Property(x => x.StartTime)
            .HasColumnName("start_time")
            .IsRequired();

        builder.Property(x => x.EndTime)
            .HasColumnName("end_time");

        builder.Property(x => x.DurationMs)
            .HasColumnName("duration_ms");

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(4000);

        builder.Property(x => x.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100);

        builder.Property(x => x.SchedulerInstanceId)
            .HasColumnName("scheduler_instance_id")
            .HasMaxLength(200);

        builder.Property(x => x.FireInstanceId)
            .HasColumnName("fire_instance_id")
            .HasMaxLength(200);

        // 索引
        builder.HasIndex(x => x.JobName);
        builder.HasIndex(x => x.JobGroup);
        builder.HasIndex(x => x.StartTime);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.FireInstanceId);
        builder.HasIndex(x => new { x.JobName, x.JobGroup, x.StartTime });
    }
}
