using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPlatform.SDK.Authentication.Services;

namespace MyPlatform.Services.Messaging.Controllers;

/// <summary>
/// 开发专用 Token 生成器 (仅用于开发测试环境)
/// </summary>
[ApiController]
[Route("api/dev/token")]
[AllowAnonymous]
public class DevTokenController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly IHostEnvironment _env;

    public DevTokenController(
        ITokenService tokenService,
        IHostEnvironment env)
    {
        _tokenService = tokenService;
        _env = env;
    }

    /// <summary>
    /// 生成测试用 JWT Token
    /// </summary>
    /// <param name="request">Token请求参数</param>
    /// <returns>包含 AccessToken 的结果</returns>
    [HttpPost]
    public ActionResult<TokenResult> Generate([FromBody] DevTokenRequest request)
    {
        // 安全检查：仅允许在开发环境使用
        if (!_env.IsDevelopment())
        {
            return NotFound();
        }

        // 构造额外的 Claims (包含 tenant_id)
        var additionalClaims = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(request.TenantId))
        {
            additionalClaims.Add("tenant_id", request.TenantId);
        }

        // 默认角色
        var roles = request.Roles ?? new[] { "admin" };

        var tokenResult = _tokenService.GenerateTokens(
            userId: request.UserId ?? "dev_user_001",
            userName: request.UserName ?? "Dev User",
            email: "dev@example.com",
            roles: roles,
            additionalClaims: additionalClaims
        );

        return Ok(tokenResult);
    }
}

public class DevTokenRequest
{
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? TenantId { get; set; }
    public string[]? Roles { get; set; }
}
