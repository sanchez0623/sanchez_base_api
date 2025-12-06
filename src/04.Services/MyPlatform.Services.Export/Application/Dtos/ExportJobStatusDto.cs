namespace MyPlatform.Services.Export.Application.Dtos;

/// <summary>
/// 导出作业状态DTO
/// </summary>
public class ExportJobStatusDto
{
    /// <summary>
    /// 作业ID
    /// </summary>
    public Guid JobId { get; set; }
    
    /// <summary>
    /// 作业状态
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// 进度百分比（0-100）
    /// </summary>
    public int? ProgressPercent { get; set; }
    
    /// <summary>
    /// 下载URL（完成后）
    /// </summary>
    public string? DownloadUrl { get; set; }
    
    /// <summary>
    /// 错误信息（失败时）
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// 完成时间
    /// </summary>
    public DateTime? CompletedAt { get; set; }
}
