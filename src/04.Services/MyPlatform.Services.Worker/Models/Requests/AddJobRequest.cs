using System.ComponentModel.DataAnnotations;

namespace MyPlatform.Services.Worker.Models.Requests;

/// <summary>
/// 添加任务请求
/// </summary>
public class AddJobRequest
{
    /// <summary>
    /// 任务名称
    /// </summary>
    /// <example>MyCustomJob</example>
    [Required(ErrorMessage = "任务名称是必需的")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "任务名称长度必须在 1-200 之间")]
    public string JobName { get; set; } = string.Empty;

    /// <summary>
    /// 任务组名
    /// </summary>
    /// <example>default</example>
    [StringLength(200, ErrorMessage = "任务组名长度不能超过 200")]
    public string Group { get; set; } = "default";

    /// <summary>
    /// 任务类型的完全限定名
    /// </summary>
    /// <example>MyPlatform.Services.Worker.Jobs.SampleCleanupJob</example>
    [Required(ErrorMessage = "任务类型是必需的")]
    public string JobType { get; set; } = string.Empty;

    /// <summary>
    /// Cron 表达式
    /// </summary>
    /// <example>0 */5 * * * ?</example>
    [Required(ErrorMessage = "Cron 表达式是必需的")]
    public string CronExpression { get; set; } = string.Empty;

    /// <summary>
    /// 任务描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 任务参数
    /// </summary>
    public Dictionary<string, string>? JobData { get; set; }

    /// <summary>
    /// 租户 ID（可选）
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// 是否立即启用
    /// </summary>
    public bool Enabled { get; set; } = true;
}
