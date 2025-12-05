namespace MyPlatform.SDK.Storage.Models;

/// <summary>
/// 预签名 URL 选项
/// </summary>
public class PresignedUrlOptions
{
    /// <summary>
    /// 过期时间
    /// </summary>
    public TimeSpan Expiry { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// 内容类型
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// 元数据
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}
