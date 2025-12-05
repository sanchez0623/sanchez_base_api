using MyPlatform.SDK.Storage.Models;

namespace MyPlatform.SDK.Storage.Exceptions;

/// <summary>
/// 存储异常基类
/// </summary>
public class StorageException : Exception
{
    /// <summary>
    /// 文件键
    /// </summary>
    public string? FileKey { get; }

    /// <summary>
    /// 存储提供商类型
    /// </summary>
    public StorageProviderType? ProviderType { get; }

    /// <summary>
    /// 初始化存储异常
    /// </summary>
    /// <param name="message">异常消息</param>
    public StorageException(string message) : base(message) { }

    /// <summary>
    /// 初始化存储异常
    /// </summary>
    /// <param name="message">异常消息</param>
    /// <param name="fileKey">文件键</param>
    public StorageException(string message, string fileKey) : base(message)
    {
        FileKey = fileKey;
    }

    /// <summary>
    /// 初始化存储异常
    /// </summary>
    /// <param name="message">异常消息</param>
    /// <param name="fileKey">文件键</param>
    /// <param name="providerType">存储提供商类型</param>
    public StorageException(string message, string fileKey, StorageProviderType providerType) : base(message)
    {
        FileKey = fileKey;
        ProviderType = providerType;
    }

    /// <summary>
    /// 初始化存储异常
    /// </summary>
    /// <param name="message">异常消息</param>
    /// <param name="innerException">内部异常</param>
    public StorageException(string message, Exception innerException)
        : base(message, innerException) { }
}
