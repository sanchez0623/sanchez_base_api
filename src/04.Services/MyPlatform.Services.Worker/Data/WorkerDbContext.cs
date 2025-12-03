using Microsoft.EntityFrameworkCore;
using MyPlatform.Services.Worker.Models;

namespace MyPlatform.Services.Worker.Data;

/// <summary>
/// Worker 数据库上下文
/// </summary>
public class WorkerDbContext : DbContext
{
    /// <summary>
    /// 初始化 Worker 数据库上下文
    /// </summary>
    /// <param name="options">数据库上下文选项</param>
    public WorkerDbContext(DbContextOptions<WorkerDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// 任务执行历史表
    /// </summary>
    public DbSet<JobExecutionHistory> JobExecutionHistories => Set<JobExecutionHistory>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WorkerDbContext).Assembly);
    }
}
