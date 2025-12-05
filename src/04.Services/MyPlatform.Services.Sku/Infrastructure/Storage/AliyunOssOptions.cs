namespace MyPlatform.Services.Sku.Infrastructure.Storage;

/// <summary>
/// 阿里云 OSS 配置选项
/// </summary>
public class AliyunOssOptions
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public const string SectionName = "Storage:AliyunOss";

    /// <summary>
    /// Endpoint 地址
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// AccessKey ID
    /// </summary>
    public string AccessKeyId { get; set; } = string.Empty;

    /// <summary>
    /// AccessKey Secret
    /// </summary>
    public string AccessKeySecret { get; set; } = string.Empty;

    /// <summary>
    /// Bucket 名称
    /// </summary>
    public string BucketName { get; set; } = string.Empty;

    /// <summary>
    /// CDN 域名（可选）
    /// </summary>
    public string? CdnDomain { get; set; }

    /// <summary>
    /// 是否使用 HTTPS
    /// </summary>
    public bool UseHttps { get; set; } = true;
}
