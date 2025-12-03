using MyPlatform.SDK.Scheduler.Jobs;
using MyPlatform.SDK.MultiTenancy.Services;
using Quartz;

namespace MyPlatform.Services.Worker.Jobs;

/// <summary>
/// 示例多租户任务 - 演示 TenantAwareJob 的使用
/// </summary>
/// <remarks>
/// 此任务展示了如何创建一个支持多租户的定时任务:
/// - 继承 TenantAwareJob 基类
/// - 在任务执行时自动设置租户上下文
/// - 使用 scoped 服务提供者获取租户相关服务
/// 
/// 典型使用场景:
/// - 为每个租户生成账单
/// - 为每个租户发送定期报告
/// - 为每个租户执行数据同步
/// 
/// 调度时可以在 JobDataMap 中设置 TenantId:
/// <code>
/// q.AddJob&lt;SampleTenantJob&gt;(opts => opts
///     .WithIdentity("tenant-job", "group")
///     .UsingJobData("TenantId", "tenant-001"));
/// </code>
/// </remarks>
[DisallowConcurrentExecution]
public class SampleTenantJob : TenantAwareJob
{
    private readonly ILogger<SampleTenantJob> _logger;

    /// <summary>
    /// 初始化多租户任务
    /// </summary>
    /// <param name="serviceProvider">服务提供者</param>
    /// <param name="logger">日志记录器</param>
    public SampleTenantJob(
        IServiceProvider serviceProvider,
        ILogger<SampleTenantJob> logger)
        : base(serviceProvider)
    {
        _logger = logger;
    }

    /// <summary>
    /// 在租户上下文中执行任务逻辑
    /// </summary>
    /// <param name="context">任务执行上下文</param>
    /// <param name="serviceProvider">作用域服务提供者</param>
    protected override async Task ExecuteInTenantContextAsync(
        IJobExecutionContext context,
        IServiceProvider serviceProvider)
    {
        var jobKey = context.JobDetail.Key;
        var tenantId = GetTenantId(context);
        var fireTime = context.FireTimeUtc.ToLocalTime();

        _logger.LogInformation(
            "[{JobKey}] 多租户任务开始执行, 租户: {TenantId}, 触发时间: {FireTime:yyyy-MM-dd HH:mm:ss}",
            jobKey,
            tenantId ?? "(无租户)",
            fireTime);

        try
        {
            // ============================================================
            // 从作用域服务提供者获取租户上下文
            // 此时租户上下文已经被 TenantAwareJob 基类自动设置
            // ============================================================
            var tenantContext = serviceProvider.GetRequiredService<ITenantContext>();

            _logger.LogDebug(
                "[{JobKey}] 当前租户上下文: TenantId={CurrentTenantId}",
                jobKey,
                tenantContext.TenantId ?? "(空)");

            // ============================================================
            // 在这里实现具体的租户相关业务逻辑
            // 例如:
            // - 使用租户隔离的 DbContext 查询数据
            // - 调用租户特定的外部服务
            // - 生成租户专属的报表
            // ============================================================

            // 模拟业务操作
            await Task.Delay(TimeSpan.FromSeconds(2), context.CancellationToken);

            var processedCount = Random.Shared.Next(50, 200);

            _logger.LogInformation(
                "[{JobKey}] 多租户任务执行完成, 租户: {TenantId}, 处理记录数: {ProcessedCount}",
                jobKey,
                tenantId ?? "(无租户)",
                processedCount);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                "[{JobKey}] 多租户任务被取消, 租户: {TenantId}",
                jobKey,
                tenantId ?? "(无租户)");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "[{JobKey}] 多租户任务执行失败, 租户: {TenantId}",
                jobKey,
                tenantId ?? "(无租户)");
            throw new JobExecutionException(ex, refireImmediately: false);
        }
    }
}
