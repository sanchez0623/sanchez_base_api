using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MyPlatform.SDK.Storage.Abstractions;
using MyPlatform.SDK.Storage.Configuration;
using MyPlatform.SDK.Storage.Models;

namespace MyPlatform.Services.Sku.Infrastructure.Storage;

/// <summary>
/// 存储服务工厂实现
/// </summary>
public class StorageServiceFactory : IStorageServiceFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly StorageOptions _options;

    /// <summary>
    /// 初始化存储服务工厂
    /// </summary>
    public StorageServiceFactory(
        IServiceProvider serviceProvider,
        IOptions<StorageOptions> options)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
    }

    /// <inheritdoc />
    public IStorageService GetService()
    {
        return GetService(_options.DefaultProvider);
    }

    /// <inheritdoc />
    public IStorageService GetService(StorageProviderType providerType)
    {
        return providerType switch
        {
            StorageProviderType.AliyunOss => _serviceProvider.GetRequiredService<AliyunOssStorageService>(),
            StorageProviderType.AwsS3 => _serviceProvider.GetRequiredService<AwsS3StorageService>(),
            _ => throw new NotSupportedException($"Storage provider {providerType} is not supported")
        };
    }
}
