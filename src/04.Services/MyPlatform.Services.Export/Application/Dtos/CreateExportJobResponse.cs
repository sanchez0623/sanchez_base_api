namespace MyPlatform.Services.Export.Application.Dtos;

/// <summary>
/// 创建导出作业响应
/// </summary>
public class CreateExportJobResponse
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
    /// 消息
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
