using Microsoft.AspNetCore.Http;
using MyPlatform.SDK.MultiTenancy.Resolvers;
using MyPlatform.SDK.MultiTenancy.Services;

namespace MyPlatform.SDK.MultiTenancy.Middleware;

/// <summary>
/// Middleware for resolving and setting the tenant context.
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IEnumerable<ITenantResolver> _resolvers;

    public TenantResolutionMiddleware(RequestDelegate next, IEnumerable<ITenantResolver> resolvers)
    {
        _next = next;
        _resolvers = resolvers.OrderBy(r => r.Priority);
    }

    public async Task InvokeAsync(HttpContext context, TenantContext tenantContext)
    {
        foreach (var resolver in _resolvers)
        {
            var tenantId = await resolver.ResolveAsync(context);
            if (!string.IsNullOrEmpty(tenantId))
            {
                tenantContext.SetTenant(tenantId);
                break;
            }
        }

        await _next(context);
    }
}
