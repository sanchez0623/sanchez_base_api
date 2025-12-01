using Microsoft.AspNetCore.Authorization;
using MyPlatform.SDK.Authorization.Requirements;
using MyPlatform.SDK.Authorization.Services;

namespace MyPlatform.SDK.Authorization.Handlers;

/// <summary>
/// Authorization handler for permission requirements.
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionChecker _permissionChecker;

    public PermissionAuthorizationHandler(IPermissionChecker permissionChecker)
    {
        _permissionChecker = permissionChecker;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            return;
        }

        if (await _permissionChecker.HasPermissionAsync(requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }
}
