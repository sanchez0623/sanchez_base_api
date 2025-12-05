using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyPlatform.SDK.Storage.Abstractions;
using MyPlatform.SDK.Storage.Exceptions;
using MyPlatform.SDK.Storage.Models;

namespace MyPlatform.Services.Sku.Infrastructure.Storage;

/// <summary>
/// AWS S3 存储服务实现
/// </summary>
public class AwsS3StorageService : IStorageService
{
    private readonly IAmazonS3 _client;
    private readonly AwsS3Options _options;
    private readonly ILogger<AwsS3StorageService> _logger;

    /// <inheritdoc />
    public StorageProviderType ProviderType => StorageProviderType.AwsS3;

    /// <summary>
    /// 初始化 AWS S3 存储服务
    /// </summary>
    public AwsS3StorageService(
        IOptions<AwsS3Options> options,
        ILogger<AwsS3StorageService> logger)
    {
        _options = options.Value;
        _logger = logger;

        var config = new AmazonS3Config
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(_options.Region),
            UseAccelerateEndpoint = _options.UseAccelerateEndpoint
        };

        _client = new AmazonS3Client(_options.AccessKeyId, _options.SecretAccessKey, config);
    }

    /// <inheritdoc />
    public async Task<UploadResult> UploadAsync(
        Stream stream,
        string fileName,
        UploadOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var key = GenerateKey(fileName, options);

        try
        {
            var request = new PutObjectRequest
            {
                BucketName = _options.BucketName,
                Key = key,
                InputStream = stream,
                ContentType = options?.ContentType ?? GetContentType(fileName)
            };

            if (options?.IsPublic == true)
            {
                request.CannedACL = S3CannedACL.PublicRead;
            }

            if (options?.Metadata != null)
            {
                foreach (var (metaKey, value) in options.Metadata)
                {
                    request.Metadata.Add(metaKey, value);
                }
            }

            var response = await _client.PutObjectAsync(request, cancellationToken);

            _logger.LogInformation("Successfully uploaded file {Key} to AWS S3", key);

            return new UploadResult
            {
                Key = key,
                ETag = response.ETag?.Trim('"'),
                Size = stream.Length,
                ContentType = request.ContentType,
                PublicUrl = options?.IsPublic == true ? GetPublicUrl(key) : null
            };
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file {Key} to AWS S3: {ErrorCode}", key, ex.ErrorCode);
            throw new StorageException($"Failed to upload file: {ex.Message}", key, StorageProviderType.AwsS3);
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
            var request = new GetObjectRequest
            {
                BucketName = _options.BucketName,
                Key = fileKey
            };

            var response = await _client.GetObjectAsync(request, cancellationToken);

            var memoryStream = new MemoryStream();
            await response.ResponseStream.CopyToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0;

            _logger.LogInformation("Successfully downloaded file {Key} from AWS S3", fileKey);
            return memoryStream;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("File {Key} not found in AWS S3", fileKey);
            throw new StorageFileNotFoundException(fileKey);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Failed to download file {Key} from AWS S3: {ErrorCode}", fileKey, ex.ErrorCode);
            throw new StorageException($"Failed to download file: {ex.Message}", fileKey, StorageProviderType.AwsS3);
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
            var request = new DeleteObjectRequest
            {
                BucketName = _options.BucketName,
                Key = fileKey
            };

            await _client.DeleteObjectAsync(request, cancellationToken);

            _logger.LogInformation("Successfully deleted file {Key} from AWS S3", fileKey);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file {Key} from AWS S3: {ErrorCode}", fileKey, ex.ErrorCode);
            throw new StorageException($"Failed to delete file: {ex.Message}", fileKey, StorageProviderType.AwsS3);
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
            var request = new DeleteObjectsRequest
            {
                BucketName = _options.BucketName,
                Objects = keys.Select(k => new KeyVersion { Key = k }).ToList()
            };

            await _client.DeleteObjectsAsync(request, cancellationToken);

            _logger.LogInformation("Successfully deleted {Count} files from AWS S3", keys.Count);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Failed to delete multiple files from AWS S3: {ErrorCode}", ex.ErrorCode);
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

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = fileKey,
            Expires = DateTime.UtcNow.Add(expiry),
            Verb = HttpVerb.GET
        };

        var url = _client.GetPreSignedURL(request);
        _logger.LogDebug("Generated presigned download URL for {Key}", fileKey);

        return Task.FromResult(url);
    }

    /// <inheritdoc />
    public Task<string> GetPresignedUploadUrlAsync(
        string fileKey,
        PresignedUrlOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var expiry = options?.Expiry ?? TimeSpan.FromHours(1);

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = fileKey,
            Expires = DateTime.UtcNow.Add(expiry),
            Verb = HttpVerb.PUT
        };

        if (!string.IsNullOrEmpty(options?.ContentType))
        {
            request.ContentType = options.ContentType;
        }

        var url = _client.GetPreSignedURL(request);
        _logger.LogDebug("Generated presigned upload URL for {Key}", fileKey);

        return Task.FromResult(url);
    }

    /// <inheritdoc />
    public async Task<StorageObject?> GetObjectInfoAsync(
        string fileKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = _options.BucketName,
                Key = fileKey
            };

            var response = await _client.GetObjectMetadataAsync(request, cancellationToken);

            return new StorageObject
            {
                Key = fileKey,
                FileName = Path.GetFileName(fileKey),
                Size = response.ContentLength,
                ContentType = response.Headers.ContentType,
                ETag = response.ETag?.Trim('"'),
                LastModified = response.LastModified,
                Metadata = response.Metadata.Keys
                    .Cast<string>()
                    .ToDictionary(k => k, k => response.Metadata[k])
            };
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Failed to get object info for {Key} from AWS S3: {ErrorCode}", fileKey, ex.ErrorCode);
            throw new StorageException($"Failed to get object info: {ex.Message}", fileKey, StorageProviderType.AwsS3);
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(
        string fileKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = _options.BucketName,
                Key = fileKey
            };

            await _client.GetObjectMetadataAsync(request, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Failed to check if file {Key} exists in AWS S3: {ErrorCode}", fileKey, ex.ErrorCode);
            throw new StorageException($"Failed to check file existence: {ex.Message}", fileKey, StorageProviderType.AwsS3);
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
            var request = new CopyObjectRequest
            {
                SourceBucket = _options.BucketName,
                SourceKey = sourceKey,
                DestinationBucket = _options.BucketName,
                DestinationKey = destKey
            };

            var response = await _client.CopyObjectAsync(request, cancellationToken);

            _logger.LogInformation("Successfully copied file from {SourceKey} to {DestKey} in AWS S3", sourceKey, destKey);

            return new UploadResult
            {
                Key = destKey,
                ETag = response.ETag?.Trim('"')
            };
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Source file {Key} not found in AWS S3", sourceKey);
            throw new StorageFileNotFoundException(sourceKey);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Failed to copy file from {SourceKey} to {DestKey} in AWS S3: {ErrorCode}", sourceKey, destKey, ex.ErrorCode);
            throw new StorageException($"Failed to copy file: {ex.Message}", sourceKey, StorageProviderType.AwsS3);
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
            var request = new ListObjectsV2Request
            {
                BucketName = _options.BucketName,
                MaxKeys = maxKeys
            };

            if (!string.IsNullOrEmpty(prefix))
            {
                request.Prefix = prefix;
            }

            var response = await _client.ListObjectsV2Async(request, cancellationToken);

            var objects = response.S3Objects
                .Select(x => new StorageObject
                {
                    Key = x.Key,
                    FileName = Path.GetFileName(x.Key),
                    Size = x.Size,
                    ETag = x.ETag?.Trim('"'),
                    LastModified = x.LastModified
                })
                .ToList();

            _logger.LogDebug("Listed {Count} objects with prefix {Prefix} from AWS S3", objects.Count, prefix);
            return objects;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Failed to list objects with prefix {Prefix} from AWS S3: {ErrorCode}", prefix, ex.ErrorCode);
            throw new StorageException($"Failed to list objects: {ex.Message}", ex);
        }
    }

    private string GenerateKey(string fileName, UploadOptions? options)
    {
        var actualFileName = options?.CustomFileName ?? fileName;
        var key = string.IsNullOrEmpty(options?.Folder)
            ? actualFileName
            : $"{options.Folder.TrimEnd('/')}/{actualFileName}";

        return key;
    }

    private string GetPublicUrl(string key)
    {
        if (!string.IsNullOrEmpty(_options.CdnDomain))
        {
            return $"https://{_options.CdnDomain}/{key}";
        }
        return $"https://{_options.BucketName}.s3.{_options.Region}.amazonaws.com/{key}";
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".csv" => "text/csv",
            ".txt" => "text/plain",
            ".json" => "application/json",
            ".xml" => "application/xml",
            ".zip" => "application/zip",
            _ => "application/octet-stream"
        };
    }
}
