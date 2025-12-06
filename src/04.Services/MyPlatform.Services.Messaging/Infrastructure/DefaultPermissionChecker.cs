using MyPlatform.SDK.Authorization.Services;

namespace MyPlatform.Services.Messaging.Infrastructure;

/// <summary>
/// 默认权限检查器（开发环境使用，允许所有权限）
/// 生产环境应替换为基于数据库或缓存的实现
/// </summary>
public class DefaultPermissionChecker : IPermissionChecker
{
    public Task<bool> HasPermissionAsync(string permission)
    {
        // 开发环境默认允许所有权限
        return Task.FromResult(true);
    }

    public Task<bool> HasPermissionAsync(string userId, string permission)
    {
        return Task.FromResult(true);
    }

    public Task<IEnumerable<string>> GetPermissionsAsync()
    {
        return Task.FromResult<IEnumerable<string>>(new[] { "*" });
    }

    public Task<IEnumerable<string>> GetPermissionsAsync(string userId)
    {
        return Task.FromResult<IEnumerable<string>>(new[] { "*" });
    }
}
