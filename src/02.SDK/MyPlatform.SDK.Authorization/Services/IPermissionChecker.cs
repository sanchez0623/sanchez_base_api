namespace MyPlatform.SDK.Authorization.Services;

/// <summary>
/// Service for checking user permissions.
/// </summary>
public interface IPermissionChecker
{
    /// <summary>
    /// Checks if the current user has a specific permission.
    /// </summary>
    /// <param name="permission">The permission to check.</param>
    /// <returns>True if the user has the permission; otherwise, false.</returns>
    Task<bool> HasPermissionAsync(string permission);

    /// <summary>
    /// Checks if a specific user has a permission.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="permission">The permission to check.</param>
    /// <returns>True if the user has the permission; otherwise, false.</returns>
    Task<bool> HasPermissionAsync(string userId, string permission);

    /// <summary>
    /// Gets all permissions for the current user.
    /// </summary>
    /// <returns>A collection of permissions.</returns>
    Task<IEnumerable<string>> GetPermissionsAsync();

    /// <summary>
    /// Gets all permissions for a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A collection of permissions.</returns>
    Task<IEnumerable<string>> GetPermissionsAsync(string userId);
}
