using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MyPlatform.SDK.MultiTenancy.Configuration;
using MyPlatform.SDK.MultiTenancy.Models;

namespace MyPlatform.SDK.MultiTenancy.Store;

/// <summary>
/// Decorator that adds caching to any ITenantStore implementation.
/// </summary>
public class CachedTenantStoreDecorator : ITenantStore
{
    private readonly ITenantStore _innerStore;
    private readonly IMemoryCache _cache;
    private readonly MultiTenancyOptions _options;

    private const string TenantCacheKeyPrefix = "tenant:";
    private const string AllTenantsCacheKey = "tenants:all";

    /// <summary>
    /// Initializes a new instance of the <see cref="CachedTenantStoreDecorator"/> class.
    /// </summary>
    /// <param name="innerStore">The underlying tenant store.</param>
    /// <param name="cache">The memory cache.</param>
    /// <param name="options">The multi-tenancy options.</param>
    public CachedTenantStoreDecorator(
        ITenantStore innerStore,
        IMemoryCache cache,
        IOptions<MultiTenancyOptions> options)
    {
        _innerStore = innerStore;
        _cache = cache;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<TenantInfo?> GetTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var cacheKey = TenantCacheKeyPrefix + tenantId;

        if (_cache.TryGetValue(cacheKey, out TenantInfo? cachedTenant))
        {
            return cachedTenant;
        }

        var tenant = await _innerStore.GetTenantAsync(tenantId, cancellationToken);

        if (tenant != null)
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.CacheExpirationMinutes)
            };
            _cache.Set(cacheKey, tenant, cacheOptions);
        }

        return tenant;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TenantInfo>> GetAllTenantsAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(AllTenantsCacheKey, out IReadOnlyList<TenantInfo>? cachedTenants))
        {
            return cachedTenants!;
        }

        var tenants = await _innerStore.GetAllTenantsAsync(cancellationToken);

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.CacheExpirationMinutes)
        };
        _cache.Set(AllTenantsCacheKey, tenants, cacheOptions);

        return tenants;
    }

    /// <inheritdoc />
    public async Task SaveTenantAsync(TenantInfo tenant, CancellationToken cancellationToken = default)
    {
        await _innerStore.SaveTenantAsync(tenant, cancellationToken);
        InvalidateCache(tenant.TenantId);
    }

    /// <inheritdoc />
    public async Task DeleteTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        await _innerStore.DeleteTenantAsync(tenantId, cancellationToken);
        InvalidateCache(tenantId);
    }

    private void InvalidateCache(string tenantId)
    {
        _cache.Remove(TenantCacheKeyPrefix + tenantId);
        _cache.Remove(AllTenantsCacheKey);
    }
}
