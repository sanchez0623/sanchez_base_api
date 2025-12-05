using MyPlatform.SDK.Storage.Models;

namespace MyPlatform.SDK.Storage.Abstractions;

/// <summary>
/// 存储服务工厂接口
/// </summary>
public interface IStorageServiceFactory
{
    /// <summary>
    /// 获取默认存储服务
    /// </summary>
    /// <returns>存储服务</returns>
    IStorageService GetService();

    /// <summary>
    /// 获取指定类型的存储服务
    /// </summary>
    /// <param name="providerType">存储提供商类型</param>
    /// <returns>存储服务</returns>
    IStorageService GetService(StorageProviderType providerType);
}
