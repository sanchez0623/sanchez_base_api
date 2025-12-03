namespace MyPlatform.Services.Worker.Models;

/// <summary>
/// 任务执行历史实体
/// </summary>
public class JobExecutionHistory
{
    /// <summary>
    /// 唯一标识
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 任务名称
    /// </summary>
    public string JobName { get; set; } = string.Empty;

    /// <summary>
    /// 任务组
    /// </summary>
    public string JobGroup { get; set; } = string.Empty;

    /// <summary>
    /// 触发器名称
    /// </summary>
    public string? TriggerName { get; set; }

    /// <summary>
    /// 触发器组
    /// </summary>
    public string? TriggerGroup { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 执行时长（毫秒）
    /// </summary>
    public long? DurationMs { get; set; }

    /// <summary>
    /// 执行状态：Success, Failed, Running, Vetoed
    /// </summary>
    public string Status { get; set; } = "Running";

    /// <summary>
    /// 错误消息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 租户 ID
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// 调度器实例 ID
    /// </summary>
    public string? SchedulerInstanceId { get; set; }

    /// <summary>
    /// Fire Instance ID，用于关联开始和结束记录
    /// </summary>
    public string? FireInstanceId { get; set; }
}
