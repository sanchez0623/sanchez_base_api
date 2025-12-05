using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyPlatform.SDK.Storage.Abstractions;
using MyPlatform.SDK.Storage.Configuration;

namespace MyPlatform.SDK.Storage.Extensions;

/// <summary>
/// 服务集合扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加存储服务基础配置
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddPlatformStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<StorageOptions>(
            configuration.GetSection(StorageOptions.SectionName));
        return services;
    }

    /// <summary>
    /// 添加自定义存储服务实现
    /// </summary>
    /// <typeparam name="TService">存储服务类型</typeparam>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddStorageService<TService>(
        this IServiceCollection services)
        where TService : class, IStorageService
    {
        services.AddScoped<IStorageService, TService>();
        return services;
    }

    /// <summary>
    /// 添加自定义存储服务工厂实现
    /// </summary>
    /// <typeparam name="TFactory">存储服务工厂类型</typeparam>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddStorageServiceFactory<TFactory>(
        this IServiceCollection services)
        where TFactory : class, IStorageServiceFactory
    {
        services.AddScoped<IStorageServiceFactory, TFactory>();
        return services;
    }
}
