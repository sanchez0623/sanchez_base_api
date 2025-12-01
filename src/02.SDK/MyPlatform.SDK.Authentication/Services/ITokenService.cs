using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyPlatform.SDK.Authentication.Configuration;

namespace MyPlatform.SDK.Authentication.Services;

/// <summary>
/// Token generation result.
/// </summary>
public class TokenResult
{
    /// <summary>
    /// Gets or sets the access token.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the refresh token.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the access token expiration time.
    /// </summary>
    public DateTime AccessTokenExpiration { get; set; }

    /// <summary>
    /// Gets or sets the refresh token expiration time.
    /// </summary>
    public DateTime RefreshTokenExpiration { get; set; }

    /// <summary>
    /// Gets or sets the token type.
    /// </summary>
    public string TokenType { get; set; } = "Bearer";
}

/// <summary>
/// Service for generating and validating JWT tokens.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates tokens for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="userName">The user name.</param>
    /// <param name="email">The user email.</param>
    /// <param name="roles">The user roles.</param>
    /// <param name="additionalClaims">Additional claims to include.</param>
    /// <returns>The generated tokens.</returns>
    TokenResult GenerateTokens(string userId, string userName, string? email = null, IEnumerable<string>? roles = null, IDictionary<string, string>? additionalClaims = null);

    /// <summary>
    /// Validates an access token and returns the claims principal.
    /// </summary>
    /// <param name="token">The access token to validate.</param>
    /// <returns>The claims principal if valid; otherwise, null.</returns>
    ClaimsPrincipal? ValidateToken(string token);

    /// <summary>
    /// Generates a new refresh token.
    /// </summary>
    /// <returns>A new refresh token.</returns>
    string GenerateRefreshToken();
}

/// <summary>
/// Default implementation of the token service.
/// </summary>
public class TokenService : ITokenService
{
    private readonly JwtOptions _options;
    private readonly SymmetricSecurityKey _signingKey;

    public TokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
    }

    /// <inheritdoc />
    public TokenResult GenerateTokens(string userId, string userName, string? email = null, IEnumerable<string>? roles = null, IDictionary<string, string>? additionalClaims = null)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Name, userName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        if (!string.IsNullOrEmpty(email))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, email));
        }

        if (roles is not null)
        {
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        if (additionalClaims is not null)
        {
            foreach (var claim in additionalClaims)
            {
                claims.Add(new Claim(claim.Key, claim.Value));
            }
        }

        var now = DateTime.UtcNow;
        var accessTokenExpiration = now.AddMinutes(_options.AccessTokenExpirationMinutes);
        var refreshTokenExpiration = now.AddDays(_options.RefreshTokenExpirationDays);

        var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: accessTokenExpiration,
            signingCredentials: credentials
        );

        return new TokenResult
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken = GenerateRefreshToken(),
            AccessTokenExpiration = accessTokenExpiration,
            RefreshTokenExpiration = refreshTokenExpiration
        };
    }

    /// <inheritdoc />
    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = _options.ValidateIssuer,
                ValidateAudience = _options.ValidateAudience,
                ValidateLifetime = _options.ValidateLifetime,
                ValidateIssuerSigningKey = _options.ValidateIssuerSigningKey,
                ValidIssuer = _options.Issuer,
                ValidAudience = _options.Audience,
                IssuerSigningKey = _signingKey,
                ClockSkew = TimeSpan.FromSeconds(_options.ClockSkewSeconds)
            };

            return tokenHandler.ValidateToken(token, validationParameters, out _);
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc />
    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
