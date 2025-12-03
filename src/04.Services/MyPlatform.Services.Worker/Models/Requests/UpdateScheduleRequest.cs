using System.ComponentModel.DataAnnotations;

namespace MyPlatform.Services.Worker.Models.Requests;

/// <summary>
/// 更新任务调度请求
/// </summary>
public class UpdateScheduleRequest
{
    /// <summary>
    /// Cron 表达式
    /// </summary>
    /// <example>0 */5 * * * ?</example>
    [Required(ErrorMessage = "Cron 表达式是必需的")]
    public string CronExpression { get; set; } = string.Empty;

    /// <summary>
    /// 任务描述（可选）
    /// </summary>
    public string? Description { get; set; }
}
