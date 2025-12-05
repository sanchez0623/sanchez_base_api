using MyPlatform.SDK.Storage.Models;

namespace MyPlatform.SDK.Storage.Abstractions;

/// <summary>
/// 文件存储服务接口
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// 存储提供商类型
    /// </summary>
    StorageProviderType ProviderType { get; }

    /// <summary>
    /// 上传文件流
    /// </summary>
    /// <param name="stream">文件流</param>
    /// <param name="fileName">文件名</param>
    /// <param name="options">上传选项</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>上传结果</returns>
    Task<UploadResult> UploadAsync(
        Stream stream,
        string fileName,
        UploadOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 上传字节数组
    /// </summary>
    /// <param name="data">字节数组</param>
    /// <param name="fileName">文件名</param>
    /// <param name="options">上传选项</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>上传结果</returns>
    Task<UploadResult> UploadAsync(
        byte[] data,
        string fileName,
        UploadOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 下载文件
    /// </summary>
    /// <param name="fileKey">文件键</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>文件流</returns>
    Task<Stream> DownloadAsync(
        string fileKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 下载为字节数组
    /// </summary>
    /// <param name="fileKey">文件键</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>字节数组</returns>
    Task<byte[]> DownloadAsBytesAsync(
        string fileKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="fileKey">文件键</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task DeleteAsync(
        string fileKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量删除文件
    /// </summary>
    /// <param name="fileKeys">文件键集合</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task DeleteManyAsync(
        IEnumerable<string> fileKeys,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取预签名下载 URL
    /// </summary>
    /// <param name="fileKey">文件键</param>
    /// <param name="options">预签名选项</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>预签名 URL</returns>
    Task<string> GetPresignedDownloadUrlAsync(
        string fileKey,
        PresignedUrlOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取预签名上传 URL（客户端直传）
    /// </summary>
    /// <param name="fileKey">文件键</param>
    /// <param name="options">预签名选项</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>预签名 URL</returns>
    Task<string> GetPresignedUploadUrlAsync(
        string fileKey,
        PresignedUrlOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取文件信息
    /// </summary>
    /// <param name="fileKey">文件键</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>存储对象信息，如果不存在返回 null</returns>
    Task<StorageObject?> GetObjectInfoAsync(
        string fileKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查文件是否存在
    /// </summary>
    /// <param name="fileKey">文件键</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否存在</returns>
    Task<bool> ExistsAsync(
        string fileKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 复制文件
    /// </summary>
    /// <param name="sourceKey">源文件键</param>
    /// <param name="destKey">目标文件键</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>上传结果</returns>
    Task<UploadResult> CopyAsync(
        string sourceKey,
        string destKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 列出文件
    /// </summary>
    /// <param name="prefix">前缀</param>
    /// <param name="maxKeys">最大数量</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>存储对象列表</returns>
    Task<IReadOnlyList<StorageObject>> ListObjectsAsync(
        string? prefix = null,
        int maxKeys = 1000,
        CancellationToken cancellationToken = default);
}
