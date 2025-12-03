using Microsoft.Extensions.Logging;
using MyPlatform.Services.Worker.Models;
using MyPlatform.Services.Worker.Models.Requests;
using Quartz;
using Quartz.Impl.Matchers;

namespace MyPlatform.Services.Worker.Services;

/// <summary>
/// 任务管理服务实现
/// </summary>
public class JobManagementService : IJobManagementService
{
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly ILogger<JobManagementService> _logger;

    /// <summary>
    /// 初始化任务管理服务
    /// </summary>
    /// <param name="schedulerFactory">调度器工厂</param>
    /// <param name="logger">日志记录器</param>
    public JobManagementService(
        ISchedulerFactory schedulerFactory,
        ILogger<JobManagementService> logger)
    {
        _schedulerFactory = schedulerFactory;
        _logger = logger;
    }

    private async Task<IScheduler> GetSchedulerAsync()
    {
        return await _schedulerFactory.GetScheduler();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<JobInfo>> GetAllJobsAsync()
    {
        var scheduler = await GetSchedulerAsync();
        var jobGroups = await scheduler.GetJobGroupNames();
        var jobs = new List<JobInfo>();

        foreach (var groupName in jobGroups)
        {
            var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(groupName));
            foreach (var jobKey in jobKeys)
            {
                var jobInfo = await GetJobInfoAsync(scheduler, jobKey);
                if (jobInfo != null)
                {
                    jobs.Add(jobInfo);
                }
            }
        }

        return jobs;
    }

    /// <inheritdoc />
    public async Task<JobInfo?> GetJobAsync(string jobName, string? groupName = null)
    {
        var scheduler = await GetSchedulerAsync();
        var jobKey = new JobKey(jobName, groupName ?? "DEFAULT");
        return await GetJobInfoAsync(scheduler, jobKey);
    }

    /// <inheritdoc />
    public async Task<bool> AddJobAsync(AddJobRequest request)
    {
        try
        {
            var scheduler = await GetSchedulerAsync();
            var jobKey = new JobKey(request.JobName, request.Group);

            // 检查任务是否已存在
            if (await scheduler.CheckExists(jobKey))
            {
                _logger.LogWarning("任务 {JobName} 已存在，无法添加", request.JobName);
                return false;
            }

            // 获取任务类型
            var jobType = Type.GetType(request.JobType);
            if (jobType == null || !typeof(IJob).IsAssignableFrom(jobType))
            {
                _logger.LogWarning("无效的任务类型: {JobType}", request.JobType);
                return false;
            }

            // 创建任务
            var jobBuilder = JobBuilder.Create(jobType)
                .WithIdentity(jobKey)
                .WithDescription(request.Description)
                .StoreDurably();

            if (request.JobData != null)
            {
                foreach (var (key, value) in request.JobData)
                {
                    jobBuilder.UsingJobData(key, value);
                }
            }

            if (!string.IsNullOrEmpty(request.TenantId))
            {
                jobBuilder.UsingJobData("TenantId", request.TenantId);
            }

            var job = jobBuilder.Build();

            // 创建触发器
            var triggerKey = new TriggerKey($"{request.JobName}-trigger", request.Group);
            var trigger = TriggerBuilder.Create()
                .WithIdentity(triggerKey)
                .WithCronSchedule(request.CronExpression)
                .Build();

            await scheduler.ScheduleJob(job, trigger);

            // 如果不启用，立即暂停
            if (!request.Enabled)
            {
                await scheduler.PauseJob(jobKey);
            }

            _logger.LogInformation("成功添加任务: {JobName}, 组: {Group}", request.JobName, request.Group);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "添加任务失败: {JobName}", request.JobName);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteJobAsync(string jobName, string? groupName = null)
    {
        try
        {
            var scheduler = await GetSchedulerAsync();
            var jobKey = new JobKey(jobName, groupName ?? "DEFAULT");

            if (!await scheduler.CheckExists(jobKey))
            {
                _logger.LogWarning("任务 {JobName} 不存在", jobName);
                return false;
            }

            var result = await scheduler.DeleteJob(jobKey);
            _logger.LogInformation("删除任务: {JobName}, 结果: {Result}", jobName, result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除任务失败: {JobName}", jobName);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> PauseJobAsync(string jobName, string? groupName = null)
    {
        try
        {
            var scheduler = await GetSchedulerAsync();
            var jobKey = new JobKey(jobName, groupName ?? "DEFAULT");

            if (!await scheduler.CheckExists(jobKey))
            {
                _logger.LogWarning("任务 {JobName} 不存在", jobName);
                return false;
            }

            await scheduler.PauseJob(jobKey);
            _logger.LogInformation("暂停任务: {JobName}", jobName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "暂停任务失败: {JobName}", jobName);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ResumeJobAsync(string jobName, string? groupName = null)
    {
        try
        {
            var scheduler = await GetSchedulerAsync();
            var jobKey = new JobKey(jobName, groupName ?? "DEFAULT");

            if (!await scheduler.CheckExists(jobKey))
            {
                _logger.LogWarning("任务 {JobName} 不存在", jobName);
                return false;
            }

            await scheduler.ResumeJob(jobKey);
            _logger.LogInformation("恢复任务: {JobName}", jobName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "恢复任务失败: {JobName}", jobName);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> TriggerJobAsync(string jobName, string? groupName = null)
    {
        try
        {
            var scheduler = await GetSchedulerAsync();
            var jobKey = new JobKey(jobName, groupName ?? "DEFAULT");

            if (!await scheduler.CheckExists(jobKey))
            {
                _logger.LogWarning("任务 {JobName} 不存在", jobName);
                return false;
            }

            await scheduler.TriggerJob(jobKey);
            _logger.LogInformation("触发任务: {JobName}", jobName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "触发任务失败: {JobName}", jobName);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> UpdateScheduleAsync(string jobName, UpdateScheduleRequest request, string? groupName = null)
    {
        try
        {
            var scheduler = await GetSchedulerAsync();
            var jobKey = new JobKey(jobName, groupName ?? "DEFAULT");

            if (!await scheduler.CheckExists(jobKey))
            {
                _logger.LogWarning("任务 {JobName} 不存在", jobName);
                return false;
            }

            // 获取现有触发器
            var triggers = await scheduler.GetTriggersOfJob(jobKey);
            if (!triggers.Any())
            {
                _logger.LogWarning("任务 {JobName} 没有触发器", jobName);
                return false;
            }

            // 更新第一个触发器的 Cron 表达式
            var oldTrigger = triggers.First();
            var newTrigger = TriggerBuilder.Create()
                .WithIdentity(oldTrigger.Key)
                .ForJob(jobKey)
                .WithCronSchedule(request.CronExpression)
                .WithDescription(request.Description)
                .Build();

            await scheduler.RescheduleJob(oldTrigger.Key, newTrigger);
            _logger.LogInformation("更新任务调度: {JobName}, 新 Cron: {CronExpression}", jobName, request.CronExpression);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新任务调度失败: {JobName}", jobName);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetAllGroupsAsync()
    {
        var scheduler = await GetSchedulerAsync();
        return await scheduler.GetJobGroupNames();
    }

    private async Task<JobInfo?> GetJobInfoAsync(IScheduler scheduler, JobKey jobKey)
    {
        var jobDetail = await scheduler.GetJobDetail(jobKey);
        if (jobDetail == null)
        {
            return null;
        }

        var triggers = await scheduler.GetTriggersOfJob(jobKey);
        var trigger = triggers.FirstOrDefault();

        string? cronExpression = null;
        if (trigger is ICronTrigger cronTrigger)
        {
            cronExpression = cronTrigger.CronExpressionString;
        }

        var triggerState = trigger != null
            ? (await scheduler.GetTriggerState(trigger.Key)).ToString()
            : "None";

        var jobData = new Dictionary<string, object?>();
        foreach (var key in jobDetail.JobDataMap.Keys)
        {
            jobData[key] = jobDetail.JobDataMap[key];
        }

        return new JobInfo
        {
            JobName = jobKey.Name,
            JobGroup = jobKey.Group,
            Description = jobDetail.Description,
            JobType = jobDetail.JobType.FullName,
            CronExpression = cronExpression,
            State = triggerState,
            LastFireTime = trigger?.GetPreviousFireTimeUtc()?.LocalDateTime,
            NextFireTime = trigger?.GetNextFireTimeUtc()?.LocalDateTime,
            JobData = jobData.Count > 0 ? jobData : null,
            TenantId = jobDetail.JobDataMap.GetString("TenantId")
        };
    }
}
