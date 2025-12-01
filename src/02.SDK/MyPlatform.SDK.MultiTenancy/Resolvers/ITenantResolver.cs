using Microsoft.AspNetCore.Http;

namespace MyPlatform.SDK.MultiTenancy.Resolvers;

/// <summary>
/// Interface for resolving the tenant from an HTTP request.
/// </summary>
public interface ITenantResolver
{
    /// <summary>
    /// Gets the priority of this resolver. Lower values have higher priority.
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Resolves the tenant identifier from the HTTP context.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>The tenant identifier if found; otherwise, null.</returns>
    Task<string?> ResolveAsync(HttpContext context);
}

/// <summary>
/// Resolves tenant from HTTP header.
/// </summary>
public class HeaderTenantResolver : ITenantResolver
{
    private const string DefaultHeaderName = "X-Tenant-Id";
    private readonly string _headerName;

    public int Priority => 1;

    public HeaderTenantResolver(string headerName = DefaultHeaderName)
    {
        _headerName = headerName;
    }

    public Task<string?> ResolveAsync(HttpContext context)
    {
        var tenantId = context.Request.Headers[_headerName].FirstOrDefault();
        return Task.FromResult(tenantId);
    }
}

/// <summary>
/// Resolves tenant from JWT claim.
/// </summary>
public class ClaimTenantResolver : ITenantResolver
{
    private const string DefaultClaimType = "tenant_id";
    private readonly string _claimType;

    public int Priority => 2;

    public ClaimTenantResolver(string claimType = DefaultClaimType)
    {
        _claimType = claimType;
    }

    public Task<string?> ResolveAsync(HttpContext context)
    {
        var tenantId = context.User.FindFirst(_claimType)?.Value;
        return Task.FromResult(tenantId);
    }
}

/// <summary>
/// Resolves tenant from query string.
/// </summary>
public class QueryStringTenantResolver : ITenantResolver
{
    private const string DefaultQueryParam = "tenantId";
    private readonly string _queryParam;

    public int Priority => 3;

    public QueryStringTenantResolver(string queryParam = DefaultQueryParam)
    {
        _queryParam = queryParam;
    }

    public Task<string?> ResolveAsync(HttpContext context)
    {
        var tenantId = context.Request.Query[_queryParam].FirstOrDefault();
        return Task.FromResult(tenantId);
    }
}

/// <summary>
/// Resolves tenant from subdomain.
/// </summary>
public class SubdomainTenantResolver : ITenantResolver
{
    public int Priority => 4;

    public Task<string?> ResolveAsync(HttpContext context)
    {
        var host = context.Request.Host.Host;
        var parts = host.Split('.');

        if (parts.Length > 2)
        {
            return Task.FromResult<string?>(parts[0]);
        }

        return Task.FromResult<string?>(null);
    }
}
