using MyPlatform.SDK.DataExchange.Abstractions;

namespace MyPlatform.Services.Export.Application.Dtos;

/// <summary>
/// 创建导出作业请求
/// </summary>
public class CreateExportJobRequest
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// 导出格式
    /// </summary>
    public DataFormat Format { get; set; } = DataFormat.Excel;
    
    /// <summary>
    /// 开始日期（可选）
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// 结束日期（可选）
    /// </summary>
    public DateTime? EndDate { get; set; }
}
