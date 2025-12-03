namespace MyPlatform.Services.Worker.Models;

/// <summary>
/// 任务信息 DTO
/// </summary>
public class JobInfo
{
    /// <summary>
    /// 任务名称
    /// </summary>
    public string JobName { get; set; } = string.Empty;

    /// <summary>
    /// 任务组
    /// </summary>
    public string JobGroup { get; set; } = string.Empty;

    /// <summary>
    /// 任务描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 任务类型
    /// </summary>
    public string? JobType { get; set; }

    /// <summary>
    /// Cron 表达式
    /// </summary>
    public string? CronExpression { get; set; }

    /// <summary>
    /// 任务状态
    /// </summary>
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// 上次执行时间
    /// </summary>
    public DateTime? LastFireTime { get; set; }

    /// <summary>
    /// 下次执行时间
    /// </summary>
    public DateTime? NextFireTime { get; set; }

    /// <summary>
    /// 任务数据
    /// </summary>
    public Dictionary<string, object?>? JobData { get; set; }

    /// <summary>
    /// 租户 ID
    /// </summary>
    public string? TenantId { get; set; }
}
