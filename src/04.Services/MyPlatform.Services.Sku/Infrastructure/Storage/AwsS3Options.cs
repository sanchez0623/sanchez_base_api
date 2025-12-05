namespace MyPlatform.Services.Sku.Infrastructure.Storage;

/// <summary>
/// AWS S3 配置选项
/// </summary>
public class AwsS3Options
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public const string SectionName = "Storage:AwsS3";

    /// <summary>
    /// AWS 区域
    /// </summary>
    public string Region { get; set; } = string.Empty;

    /// <summary>
    /// AccessKey ID
    /// </summary>
    public string AccessKeyId { get; set; } = string.Empty;

    /// <summary>
    /// Secret AccessKey
    /// </summary>
    public string SecretAccessKey { get; set; } = string.Empty;

    /// <summary>
    /// Bucket 名称
    /// </summary>
    public string BucketName { get; set; } = string.Empty;

    /// <summary>
    /// CDN 域名（可选）
    /// </summary>
    public string? CdnDomain { get; set; }

    /// <summary>
    /// 是否使用加速端点
    /// </summary>
    public bool UseAccelerateEndpoint { get; set; } = false;
}
