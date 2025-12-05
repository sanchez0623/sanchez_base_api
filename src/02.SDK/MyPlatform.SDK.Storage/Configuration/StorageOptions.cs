using MyPlatform.SDK.Storage.Models;

namespace MyPlatform.SDK.Storage.Configuration;

/// <summary>
/// 存储配置选项
/// </summary>
public class StorageOptions
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public const string SectionName = "Storage";

    /// <summary>
    /// 默认存储提供商
    /// </summary>
    public StorageProviderType DefaultProvider { get; set; } = StorageProviderType.AliyunOss;

    /// <summary>
    /// 默认 Bucket 名称
    /// </summary>
    public string DefaultBucketName { get; set; } = string.Empty;

    /// <summary>
    /// 文件 Key 前缀
    /// </summary>
    public string KeyPrefix { get; set; } = string.Empty;

    /// <summary>
    /// 最大文件大小（字节），默认 100MB
    /// </summary>
    public long MaxFileSize { get; set; } = 100 * 1024 * 1024;

    /// <summary>
    /// 允许的文件扩展名（为空则不限制）
    /// </summary>
    public List<string> AllowedExtensions { get; set; } = new();
}
