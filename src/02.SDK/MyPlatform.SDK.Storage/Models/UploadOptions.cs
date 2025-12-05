namespace MyPlatform.SDK.Storage.Models;

/// <summary>
/// 上传选项
/// </summary>
public class UploadOptions
{
    /// <summary>
    /// 文件夹路径
    /// </summary>
    public string? Folder { get; set; }

    /// <summary>
    /// 内容类型
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// 是否公开访问
    /// </summary>
    public bool IsPublic { get; set; } = false;

    /// <summary>
    /// 是否覆盖已存在的文件
    /// </summary>
    public bool OverwriteExisting { get; set; } = false;

    /// <summary>
    /// 元数据
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }

    /// <summary>
    /// 自定义文件名
    /// </summary>
    public string? CustomFileName { get; set; }
}
