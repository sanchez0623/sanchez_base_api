namespace MyPlatform.SDK.Storage.Models;

/// <summary>
/// 上传结果
/// </summary>
public class UploadResult
{
    /// <summary>
    /// 对象键（路径）
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 公开访问 URL
    /// </summary>
    public string? PublicUrl { get; set; }

    /// <summary>
    /// ETag
    /// </summary>
    public string? ETag { get; set; }

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// 内容类型
    /// </summary>
    public string? ContentType { get; set; }
}
