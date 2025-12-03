using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyPlatform.Services.Worker.Data;
using MyPlatform.Services.Worker.Models;

namespace MyPlatform.Services.Worker.Services;

/// <summary>
/// 任务执行历史服务接口
/// </summary>
public interface IJobExecutionHistoryService
{
    /// <summary>
    /// 记录任务开始执行
    /// </summary>
    /// <param name="history">执行历史记录</param>
    /// <returns>创建的记录 ID</returns>
    Task<long> RecordJobStartAsync(JobExecutionHistory history);

    /// <summary>
    /// 记录任务执行完成
    /// </summary>
    /// <param name="fireInstanceId">Fire Instance ID</param>
    /// <param name="endTime">结束时间</param>
    /// <param name="durationMs">执行时长（毫秒）</param>
    /// <param name="status">执行状态</param>
    /// <param name="errorMessage">错误消息（如果有）</param>
    Task RecordJobEndAsync(string fireInstanceId, DateTime endTime, long durationMs, string status, string? errorMessage = null);

    /// <summary>
    /// 获取任务执行历史
    /// </summary>
    /// <param name="jobName">任务名称</param>
    /// <param name="jobGroup">任务组</param>
    /// <param name="pageNumber">页码（从 1 开始）</param>
    /// <param name="pageSize">每页大小</param>
    /// <returns>执行历史列表</returns>
    Task<IEnumerable<JobExecutionHistory>> GetHistoryAsync(string jobName, string? jobGroup = null, int pageNumber = 1, int pageSize = 20);

    /// <summary>
    /// 获取最近的执行历史
    /// </summary>
    /// <param name="count">返回数量</param>
    /// <returns>执行历史列表</returns>
    Task<IEnumerable<JobExecutionHistory>> GetRecentHistoryAsync(int count = 50);

    /// <summary>
    /// 清理过期的执行历史
    /// </summary>
    /// <param name="retentionDays">保留天数</param>
    /// <returns>删除的记录数</returns>
    Task<int> CleanupOldHistoryAsync(int retentionDays);
}

/// <summary>
/// 任务执行历史服务实现
/// </summary>
public class JobExecutionHistoryService : IJobExecutionHistoryService
{
    private readonly WorkerDbContext _dbContext;
    private readonly ILogger<JobExecutionHistoryService> _logger;

    /// <summary>
    /// 初始化任务执行历史服务
    /// </summary>
    /// <param name="dbContext">数据库上下文</param>
    /// <param name="logger">日志记录器</param>
    public JobExecutionHistoryService(
        WorkerDbContext dbContext,
        ILogger<JobExecutionHistoryService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<long> RecordJobStartAsync(JobExecutionHistory history)
    {
        try
        {
            _dbContext.JobExecutionHistories.Add(history);
            await _dbContext.SaveChangesAsync();
            _logger.LogDebug("记录任务开始: {JobName}, FireInstanceId: {FireInstanceId}", 
                history.JobName, history.FireInstanceId);
            return history.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "记录任务开始失败: {JobName}", history.JobName);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task RecordJobEndAsync(string fireInstanceId, DateTime endTime, long durationMs, string status, string? errorMessage = null)
    {
        try
        {
            var history = await _dbContext.JobExecutionHistories
                .FirstOrDefaultAsync(h => h.FireInstanceId == fireInstanceId);

            if (history != null)
            {
                history.EndTime = endTime;
                history.DurationMs = durationMs;
                history.Status = status;
                history.ErrorMessage = errorMessage;
                await _dbContext.SaveChangesAsync();
                _logger.LogDebug("记录任务结束: {JobName}, 状态: {Status}, 耗时: {DurationMs}ms", 
                    history.JobName, status, durationMs);
            }
            else
            {
                _logger.LogWarning("未找到对应的任务开始记录: FireInstanceId={FireInstanceId}", fireInstanceId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "记录任务结束失败: FireInstanceId={FireInstanceId}", fireInstanceId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<JobExecutionHistory>> GetHistoryAsync(string jobName, string? jobGroup = null, int pageNumber = 1, int pageSize = 20)
    {
        var query = _dbContext.JobExecutionHistories
            .Where(h => h.JobName == jobName);

        if (!string.IsNullOrEmpty(jobGroup))
        {
            query = query.Where(h => h.JobGroup == jobGroup);
        }

        return await query
            .OrderByDescending(h => h.StartTime)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<JobExecutionHistory>> GetRecentHistoryAsync(int count = 50)
    {
        return await _dbContext.JobExecutionHistories
            .OrderByDescending(h => h.StartTime)
            .Take(count)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<int> CleanupOldHistoryAsync(int retentionDays)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
            var deletedCount = await _dbContext.JobExecutionHistories
                .Where(h => h.StartTime < cutoffDate)
                .ExecuteDeleteAsync();

            _logger.LogInformation("清理过期执行历史: 删除 {DeletedCount} 条记录", deletedCount);
            return deletedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清理过期执行历史失败");
            throw;
        }
    }
}
