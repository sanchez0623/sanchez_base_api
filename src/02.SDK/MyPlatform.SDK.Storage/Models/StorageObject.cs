namespace MyPlatform.SDK.Storage.Models;

/// <summary>
/// 存储对象信息
/// </summary>
public class StorageObject
{
    /// <summary>
    /// 对象键（路径）
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 文件名
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// 内容类型
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// ETag
    /// </summary>
    public string? ETag { get; set; }

    /// <summary>
    /// 最后修改时间
    /// </summary>
    public DateTime LastModified { get; set; }

    /// <summary>
    /// 元数据
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
}
