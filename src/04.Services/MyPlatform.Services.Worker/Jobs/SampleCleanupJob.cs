using Quartz;

namespace MyPlatform.Services.Worker.Jobs;

/// <summary>
/// 示例清理任务 - 演示基本的 Quartz IJob 实现
/// </summary>
/// <remarks>
/// 此任务展示了如何创建一个简单的定时任务:
/// - 实现 IJob 接口
/// - 使用依赖注入获取日志服务
/// - 记录任务执行信息
/// 
/// 典型使用场景:
/// - 定期清理过期数据
/// - 定期生成报表
/// - 定期发送通知
/// </remarks>
[DisallowConcurrentExecution]
public class SampleCleanupJob : IJob
{
    private readonly ILogger<SampleCleanupJob> _logger;

    /// <summary>
    /// 初始化清理任务
    /// </summary>
    /// <param name="logger">日志记录器</param>
    public SampleCleanupJob(ILogger<SampleCleanupJob> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 执行清理任务
    /// </summary>
    /// <param name="context">任务执行上下文</param>
    public async Task Execute(IJobExecutionContext context)
    {
        var jobKey = context.JobDetail.Key;
        var fireTime = context.FireTimeUtc.ToLocalTime();

        _logger.LogInformation(
            "[{JobKey}] 清理任务开始执行, 触发时间: {FireTime:yyyy-MM-dd HH:mm:ss}",
            jobKey,
            fireTime);

        try
        {
            // ============================================================
            // 在这里实现具体的清理逻辑
            // 例如:
            // - 清理过期的缓存数据
            // - 删除临时文件
            // - 清理过期的会话记录
            // ============================================================

            // 模拟清理操作
            await Task.Delay(TimeSpan.FromSeconds(1), context.CancellationToken);

            var cleanedCount = Random.Shared.Next(10, 100);

            _logger.LogInformation(
                "[{JobKey}] 清理任务执行完成, 清理记录数: {CleanedCount}",
                jobKey,
                cleanedCount);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("[{JobKey}] 清理任务被取消", jobKey);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{JobKey}] 清理任务执行失败", jobKey);
            throw new JobExecutionException(ex, refireImmediately: false);
        }
    }
}
