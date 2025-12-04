using Microsoft.AspNetCore.Http;
using MyPlatform.SDK.MultiTenancy.Models;
using MyPlatform.SDK.MultiTenancy.Resolvers;
using MyPlatform.SDK.MultiTenancy.Services;
using MyPlatform.SDK.MultiTenancy.Store;

namespace MyPlatform.SDK.MultiTenancy.Middleware;

/// <summary>
/// Middleware for resolving and setting the tenant context.
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IEnumerable<ITenantResolver> _resolvers;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantResolutionMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="resolvers">The collection of tenant resolvers.</param>
    public TenantResolutionMiddleware(RequestDelegate next, IEnumerable<ITenantResolver> resolvers)
    {
        _next = next;
        _resolvers = resolvers.OrderBy(r => r.Priority);
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="tenantContext">The tenant context.</param>
    /// <param name="tenantStore">The tenant store (optional, injected if available).</param>
    public async Task InvokeAsync(HttpContext context, TenantContext tenantContext, ITenantStore? tenantStore = null)
    {
        foreach (var resolver in _resolvers)
        {
            var tenantId = await resolver.ResolveAsync(context);
            if (!string.IsNullOrEmpty(tenantId))
            {
                // If tenant store is available, load full tenant info
                if (tenantStore != null)
                {
                    var tenantInfo = await tenantStore.GetTenantAsync(tenantId, context.RequestAborted);

                    if (tenantInfo != null)
                    {
                        // Check tenant status
                        if (tenantInfo.Status == TenantStatus.Suspended)
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            await context.Response.WriteAsync($"Tenant is suspended: {tenantId}");
                            return;
                        }

                        if (tenantInfo.Status == TenantStatus.Deleted)
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            await context.Response.WriteAsync($"Tenant not found: {tenantId}");
                            return;
                        }

                        tenantContext.SetTenant(tenantInfo);
                    }
                    else
                    {
                        // Tenant not found in store, just set tenant ID
                        tenantContext.SetTenant(tenantId);
                    }
                }
                else
                {
                    // No tenant store, just set tenant ID
                    tenantContext.SetTenant(tenantId);
                }
                break;
            }
        }

        await _next(context);
    }
}
