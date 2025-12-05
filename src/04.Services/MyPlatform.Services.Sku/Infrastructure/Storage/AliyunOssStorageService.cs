using Aliyun.OSS;
using Aliyun.OSS.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyPlatform.SDK.Storage.Abstractions;
using MyPlatform.SDK.Storage.Exceptions;
using MyPlatform.SDK.Storage.Models;

namespace MyPlatform.Services.Sku.Infrastructure.Storage;

/// <summary>
/// 阿里云 OSS 存储服务实现
/// </summary>
public class AliyunOssStorageService : IStorageService
{
    private readonly OssClient _client;
    private readonly AliyunOssOptions _options;
    private readonly ILogger<AliyunOssStorageService> _logger;

    /// <inheritdoc />
    public StorageProviderType ProviderType => StorageProviderType.AliyunOss;

    /// <summary>
    /// 初始化阿里云 OSS 存储服务
    /// </summary>
    public AliyunOssStorageService(
        IOptions<AliyunOssOptions> options,
        ILogger<AliyunOssStorageService> logger)
    {
        _options = options.Value;
        _logger = logger;
        _client = new OssClient(_options.Endpoint, _options.AccessKeyId, _options.AccessKeySecret);
    }

    /// <inheritdoc />
    public async Task<UploadResult> UploadAsync(
        Stream stream,
        string fileName,
        UploadOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var key = StorageUtilities.GenerateKey(fileName, options);

        try
        {
            var metadata = new ObjectMetadata();
            if (!string.IsNullOrEmpty(options?.ContentType))
            {
                metadata.ContentType = options.ContentType;
            }
            else
            {
                metadata.ContentType = StorageUtilities.GetContentType(fileName);
            }

            if (options?.Metadata != null)
            {
                foreach (var (metaKey, value) in options.Metadata)
                {
                    metadata.UserMetadata.Add(metaKey, value);
                }
            }

            var result = await Task.Run(() =>
                _client.PutObject(_options.BucketName, key, stream, metadata), cancellationToken);

            _logger.LogInformation("Successfully uploaded file {Key} to Aliyun OSS", key);

            return new UploadResult
            {
                Key = key,
                ETag = result.ETag,
                Size = stream.Length,
                ContentType = metadata.ContentType,
                PublicUrl = options?.IsPublic == true ? GetPublicUrl(key) : null
            };
        }
        catch (OssException ex)
        {
            _logger.LogError(ex, "Failed to upload file {Key} to Aliyun OSS: {ErrorCode}", key, ex.ErrorCode);
            throw new StorageException($"Failed to upload file: {ex.Message}", key, StorageProviderType.AliyunOss);
        }
    }

    /// <inheritdoc />
    public async Task<UploadResult> UploadAsync(
        byte[] data,
        string fileName,
        UploadOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream(data);
        return await UploadAsync(stream, fileName, options, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Stream> DownloadAsync(
        string fileKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await Task.Run(() =>
                _client.GetObject(_options.BucketName, fileKey), cancellationToken);

            var memoryStream = new MemoryStream();
            await result.Content.CopyToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0;

            _logger.LogInformation("Successfully downloaded file {Key} from Aliyun OSS", fileKey);
            return memoryStream;
        }
        catch (OssException ex) when (ex.ErrorCode == "NoSuchKey")
        {
            _logger.LogWarning("File {Key} not found in Aliyun OSS", fileKey);
            throw new StorageFileNotFoundException(fileKey);
        }
        catch (OssException ex)
        {
            _logger.LogError(ex, "Failed to download file {Key} from Aliyun OSS: {ErrorCode}", fileKey, ex.ErrorCode);
            throw new StorageException($"Failed to download file: {ex.Message}", fileKey, StorageProviderType.AliyunOss);
        }
    }

    /// <inheritdoc />
    public async Task<byte[]> DownloadAsBytesAsync(
        string fileKey,
        CancellationToken cancellationToken = default)
    {
        using var stream = await DownloadAsync(fileKey, cancellationToken);
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, cancellationToken);
        return memoryStream.ToArray();
    }

    /// <inheritdoc />
    public async Task DeleteAsync(
        string fileKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Run(() =>
                _client.DeleteObject(_options.BucketName, fileKey), cancellationToken);

            _logger.LogInformation("Successfully deleted file {Key} from Aliyun OSS", fileKey);
        }
        catch (OssException ex)
        {
            _logger.LogError(ex, "Failed to delete file {Key} from Aliyun OSS: {ErrorCode}", fileKey, ex.ErrorCode);
            throw new StorageException($"Failed to delete file: {ex.Message}", fileKey, StorageProviderType.AliyunOss);
        }
    }

    /// <inheritdoc />
    public async Task DeleteManyAsync(
        IEnumerable<string> fileKeys,
        CancellationToken cancellationToken = default)
    {
        var keys = fileKeys.ToList();
        if (keys.Count == 0) return;

        try
        {
            var request = new DeleteObjectsRequest(_options.BucketName, keys);
            await Task.Run(() => _client.DeleteObjects(request), cancellationToken);

            _logger.LogInformation("Successfully deleted {Count} files from Aliyun OSS", keys.Count);
        }
        catch (OssException ex)
        {
            _logger.LogError(ex, "Failed to delete multiple files from Aliyun OSS: {ErrorCode}", ex.ErrorCode);
            throw new StorageException($"Failed to delete files: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public Task<string> GetPresignedDownloadUrlAsync(
        string fileKey,
        PresignedUrlOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var expiry = options?.Expiry ?? TimeSpan.FromHours(1);
        var expirationTime = DateTime.UtcNow.Add(expiry);

        var request = new GeneratePresignedUriRequest(_options.BucketName, fileKey, SignHttpMethod.Get)
        {
            Expiration = expirationTime
        };

        var uri = _client.GeneratePresignedUri(request);
        _logger.LogDebug("Generated presigned download URL for {Key}", fileKey);

        return Task.FromResult(uri.ToString());
    }

    /// <inheritdoc />
    public Task<string> GetPresignedUploadUrlAsync(
        string fileKey,
        PresignedUrlOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var expiry = options?.Expiry ?? TimeSpan.FromHours(1);
        var expirationTime = DateTime.UtcNow.Add(expiry);

        var request = new GeneratePresignedUriRequest(_options.BucketName, fileKey, SignHttpMethod.Put)
        {
            Expiration = expirationTime
        };

        if (!string.IsNullOrEmpty(options?.ContentType))
        {
            request.ContentType = options.ContentType;
        }

        var uri = _client.GeneratePresignedUri(request);
        _logger.LogDebug("Generated presigned upload URL for {Key}", fileKey);

        return Task.FromResult(uri.ToString());
    }

    /// <inheritdoc />
    public async Task<StorageObject?> GetObjectInfoAsync(
        string fileKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var metadata = await Task.Run(() =>
                _client.GetObjectMetadata(_options.BucketName, fileKey), cancellationToken);

            return new StorageObject
            {
                Key = fileKey,
                FileName = Path.GetFileName(fileKey),
                Size = metadata.ContentLength,
                ContentType = metadata.ContentType,
                ETag = metadata.ETag,
                LastModified = metadata.LastModified,
                Metadata = metadata.UserMetadata.ToDictionary(x => x.Key, x => x.Value)
            };
        }
        catch (OssException ex) when (ex.ErrorCode == "NoSuchKey")
        {
            return null;
        }
        catch (OssException ex)
        {
            _logger.LogError(ex, "Failed to get object info for {Key} from Aliyun OSS: {ErrorCode}", fileKey, ex.ErrorCode);
            throw new StorageException($"Failed to get object info: {ex.Message}", fileKey, StorageProviderType.AliyunOss);
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(
        string fileKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var exists = await Task.Run(() =>
                _client.DoesObjectExist(_options.BucketName, fileKey), cancellationToken);
            return exists;
        }
        catch (OssException ex)
        {
            _logger.LogError(ex, "Failed to check if file {Key} exists in Aliyun OSS: {ErrorCode}", fileKey, ex.ErrorCode);
            throw new StorageException($"Failed to check file existence: {ex.Message}", fileKey, StorageProviderType.AliyunOss);
        }
    }

    /// <inheritdoc />
    public async Task<UploadResult> CopyAsync(
        string sourceKey,
        string destKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new CopyObjectRequest(_options.BucketName, sourceKey, _options.BucketName, destKey);
            var result = await Task.Run(() => _client.CopyObject(request), cancellationToken);

            _logger.LogInformation("Successfully copied file from {SourceKey} to {DestKey} in Aliyun OSS", sourceKey, destKey);

            return new UploadResult
            {
                Key = destKey,
                ETag = result.ETag
            };
        }
        catch (OssException ex) when (ex.ErrorCode == "NoSuchKey")
        {
            _logger.LogWarning("Source file {Key} not found in Aliyun OSS", sourceKey);
            throw new StorageFileNotFoundException(sourceKey);
        }
        catch (OssException ex)
        {
            _logger.LogError(ex, "Failed to copy file from {SourceKey} to {DestKey} in Aliyun OSS: {ErrorCode}", sourceKey, destKey, ex.ErrorCode);
            throw new StorageException($"Failed to copy file: {ex.Message}", sourceKey, StorageProviderType.AliyunOss);
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<StorageObject>> ListObjectsAsync(
        string? prefix = null,
        int maxKeys = 1000,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new ListObjectsRequest(_options.BucketName)
            {
                MaxKeys = maxKeys
            };

            if (!string.IsNullOrEmpty(prefix))
            {
                request.Prefix = prefix;
            }

            var listing = await Task.Run(() => _client.ListObjects(request), cancellationToken);

            var objects = listing.ObjectSummaries
                .Select(x => new StorageObject
                {
                    Key = x.Key,
                    FileName = Path.GetFileName(x.Key),
                    Size = x.Size,
                    ETag = x.ETag,
                    LastModified = x.LastModified
                })
                .ToList();

            _logger.LogDebug("Listed {Count} objects with prefix {Prefix} from Aliyun OSS", objects.Count, prefix);
            return objects;
        }
        catch (OssException ex)
        {
            _logger.LogError(ex, "Failed to list objects with prefix {Prefix} from Aliyun OSS: {ErrorCode}", prefix, ex.ErrorCode);
            throw new StorageException($"Failed to list objects: {ex.Message}", ex);
        }
    }

    private string GetPublicUrl(string key)
    {
        var protocol = _options.UseHttps ? "https" : "http";
        if (!string.IsNullOrEmpty(_options.CdnDomain))
        {
            return $"{protocol}://{_options.CdnDomain}/{key}";
        }
        return $"{protocol}://{_options.BucketName}.{_options.Endpoint}/{key}";
    }
}
