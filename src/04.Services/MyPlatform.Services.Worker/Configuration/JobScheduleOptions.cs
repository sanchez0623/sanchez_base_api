namespace MyPlatform.Services.Worker.Configuration;

/// <summary>
/// 任务调度配置选项
/// </summary>
public class JobScheduleOptions
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public const string SectionName = "JobSchedules";

    /// <summary>
    /// 任务配置字典，Key 为任务名称
    /// </summary>
    public Dictionary<string, JobConfig> Jobs { get; set; } = new();
}

/// <summary>
/// 单个任务配置
/// </summary>
public class JobConfig
{
    /// <summary>
    /// 是否启用该任务
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Cron 表达式，用于定义任务调度时间
    /// </summary>
    public string CronExpression { get; set; } = string.Empty;

    /// <summary>
    /// 任务描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 任务组名
    /// </summary>
    public string Group { get; set; } = "default";

    /// <summary>
    /// 任务参数
    /// </summary>
    public Dictionary<string, string>? JobData { get; set; }

    /// <summary>
    /// 任务类型的完全限定名（用于动态创建任务）
    /// </summary>
    public string? JobType { get; set; }
}
