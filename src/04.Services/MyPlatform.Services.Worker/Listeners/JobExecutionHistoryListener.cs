using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyPlatform.Services.Worker.Models;
using MyPlatform.Services.Worker.Services;
using Quartz;

namespace MyPlatform.Services.Worker.Listeners;

/// <summary>
/// 任务执行历史监听器 - 记录任务执行历史
/// </summary>
public class JobExecutionHistoryListener : IJobListener
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<JobExecutionHistoryListener> _logger;

    /// <summary>
    /// 监听器名称
    /// </summary>
    public string Name => "JobExecutionHistoryListener";

    /// <summary>
    /// 初始化任务执行历史监听器
    /// </summary>
    /// <param name="serviceProvider">服务提供者</param>
    /// <param name="logger">日志记录器</param>
    public JobExecutionHistoryListener(
        IServiceProvider serviceProvider,
        ILogger<JobExecutionHistoryListener> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// 任务即将执行
    /// </summary>
    public async Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var historyService = scope.ServiceProvider.GetRequiredService<IJobExecutionHistoryService>();

            var jobDetail = context.JobDetail;
            var trigger = context.Trigger;

            var history = new JobExecutionHistory
            {
                JobName = jobDetail.Key.Name,
                JobGroup = jobDetail.Key.Group,
                TriggerName = trigger.Key.Name,
                TriggerGroup = trigger.Key.Group,
                StartTime = context.FireTimeUtc.UtcDateTime,
                Status = "Running",
                TenantId = jobDetail.JobDataMap.GetString("TenantId"),
                SchedulerInstanceId = context.Scheduler.SchedulerInstanceId,
                FireInstanceId = context.FireInstanceId
            };

            await historyService.RecordJobStartAsync(history);
            _logger.LogDebug("任务开始执行: {JobName}, FireInstanceId: {FireInstanceId}", 
                jobDetail.Key.Name, context.FireInstanceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "记录任务开始执行失败: {JobName}", context.JobDetail.Key.Name);
        }
    }

    /// <summary>
    /// 任务执行被否决
    /// </summary>
    public async Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var historyService = scope.ServiceProvider.GetRequiredService<IJobExecutionHistoryService>();

            var endTime = DateTime.UtcNow;
            var startTime = context.FireTimeUtc.UtcDateTime;
            var durationMs = (long)(endTime - startTime).TotalMilliseconds;

            await historyService.RecordJobEndAsync(
                context.FireInstanceId,
                endTime,
                durationMs,
                "Vetoed",
                "任务执行被否决");

            _logger.LogWarning("任务执行被否决: {JobName}", context.JobDetail.Key.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "记录任务否决失败: {JobName}", context.JobDetail.Key.Name);
        }
    }

    /// <summary>
    /// 任务执行完成
    /// </summary>
    public async Task JobWasExecuted(IJobExecutionContext context, JobExecutionException? jobException, CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var historyService = scope.ServiceProvider.GetRequiredService<IJobExecutionHistoryService>();

            var endTime = DateTime.UtcNow;
            var startTime = context.FireTimeUtc.UtcDateTime;
            var durationMs = (long)(endTime - startTime).TotalMilliseconds;

            var status = jobException == null ? "Success" : "Failed";
            var errorMessage = jobException?.Message;

            await historyService.RecordJobEndAsync(
                context.FireInstanceId,
                endTime,
                durationMs,
                status,
                errorMessage);

            if (jobException != null)
            {
                _logger.LogError(jobException, "任务执行失败: {JobName}", context.JobDetail.Key.Name);
            }
            else
            {
                _logger.LogDebug("任务执行成功: {JobName}, 耗时: {DurationMs}ms", 
                    context.JobDetail.Key.Name, durationMs);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "记录任务执行完成失败: {JobName}", context.JobDetail.Key.Name);
        }
    }
}
