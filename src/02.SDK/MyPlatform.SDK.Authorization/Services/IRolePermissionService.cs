namespace MyPlatform.SDK.Authorization.Services;

/// <summary>
/// Service for managing roles and permissions.
/// </summary>
public interface IRolePermissionService
{
    /// <summary>
    /// Gets the permissions for a role.
    /// </summary>
    /// <param name="roleName">The role name.</param>
    /// <returns>A collection of permissions.</returns>
    Task<IEnumerable<string>> GetRolePermissionsAsync(string roleName);

    /// <summary>
    /// Gets the roles for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A collection of role names.</returns>
    Task<IEnumerable<string>> GetUserRolesAsync(string userId);

    /// <summary>
    /// Gets all permissions for a user based on their roles.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A collection of permissions.</returns>
    Task<IEnumerable<string>> GetUserPermissionsAsync(string userId);

    /// <summary>
    /// Assigns a role to a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="roleName">The role name.</param>
    Task AssignRoleAsync(string userId, string roleName);

    /// <summary>
    /// Removes a role from a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="roleName">The role name.</param>
    Task RemoveRoleAsync(string userId, string roleName);

    /// <summary>
    /// Grants a permission to a role.
    /// </summary>
    /// <param name="roleName">The role name.</param>
    /// <param name="permission">The permission to grant.</param>
    Task GrantPermissionAsync(string roleName, string permission);

    /// <summary>
    /// Revokes a permission from a role.
    /// </summary>
    /// <param name="roleName">The role name.</param>
    /// <param name="permission">The permission to revoke.</param>
    Task RevokePermissionAsync(string roleName, string permission);
}
