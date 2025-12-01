using System.Security.Claims;

namespace MyPlatform.SDK.Authentication.Services;

/// <summary>
/// Represents the current authenticated user.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Gets the user name.
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// Gets the user email.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Gets the tenant identifier.
    /// </summary>
    string? TenantId { get; }

    /// <summary>
    /// Gets a value indicating whether the user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Gets the user roles.
    /// </summary>
    IEnumerable<string> Roles { get; }

    /// <summary>
    /// Gets a claim value by type.
    /// </summary>
    /// <param name="claimType">The claim type.</param>
    /// <returns>The claim value if found; otherwise, null.</returns>
    string? GetClaim(string claimType);

    /// <summary>
    /// Gets all claims of a specific type.
    /// </summary>
    /// <param name="claimType">The claim type.</param>
    /// <returns>All claims of the specified type.</returns>
    IEnumerable<string> GetClaims(string claimType);

    /// <summary>
    /// Checks if the user has a specific role.
    /// </summary>
    /// <param name="role">The role to check.</param>
    /// <returns>True if the user has the role; otherwise, false.</returns>
    bool IsInRole(string role);
}

/// <summary>
/// Default implementation of the current user.
/// </summary>
public class CurrentUser : ICurrentUser
{
    private readonly ClaimsPrincipal? _principal;

    public CurrentUser(ClaimsPrincipal? principal = null)
    {
        _principal = principal;
    }

    /// <inheritdoc />
    public string? UserId => GetClaim(ClaimTypes.NameIdentifier) ?? GetClaim("sub");

    /// <inheritdoc />
    public string? UserName => GetClaim(ClaimTypes.Name) ?? GetClaim("name");

    /// <inheritdoc />
    public string? Email => GetClaim(ClaimTypes.Email) ?? GetClaim("email");

    /// <inheritdoc />
    public string? TenantId => GetClaim("tenant_id") ?? GetClaim("tid");

    /// <inheritdoc />
    public bool IsAuthenticated => _principal?.Identity?.IsAuthenticated ?? false;

    /// <inheritdoc />
    public IEnumerable<string> Roles => GetClaims(ClaimTypes.Role);

    /// <inheritdoc />
    public string? GetClaim(string claimType)
    {
        return _principal?.FindFirst(claimType)?.Value;
    }

    /// <inheritdoc />
    public IEnumerable<string> GetClaims(string claimType)
    {
        return _principal?.FindAll(claimType).Select(c => c.Value) ?? [];
    }

    /// <inheritdoc />
    public bool IsInRole(string role)
    {
        return _principal?.IsInRole(role) ?? false;
    }
}
