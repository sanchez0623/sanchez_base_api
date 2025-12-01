using Microsoft.AspNetCore.Authorization;

namespace MyPlatform.SDK.Authorization.Attributes;

/// <summary>
/// Attribute for requiring a specific permission.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Gets the required permission.
    /// </summary>
    public string Permission { get; }

    public RequirePermissionAttribute(string permission)
        : base(policy: $"Permission:{permission}")
    {
        Permission = permission;
    }
}
