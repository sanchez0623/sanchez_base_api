namespace MyPlatform.SDK.Storage.Models;

/// <summary>
/// 存储提供商类型枚举
/// </summary>
public enum StorageProviderType
{
    /// <summary>
    /// 阿里云 OSS
    /// </summary>
    AliyunOss,

    /// <summary>
    /// AWS S3
    /// </summary>
    AwsS3,

    /// <summary>
    /// MinIO
    /// </summary>
    MinIO,

    /// <summary>
    /// Azure Blob Storage
    /// </summary>
    AzureBlob,

    /// <summary>
    /// 本地文件系统
    /// </summary>
    Local
}
