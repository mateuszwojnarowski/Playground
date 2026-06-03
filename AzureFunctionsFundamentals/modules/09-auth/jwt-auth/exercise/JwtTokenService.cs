using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AzureFunctionsFundamentals.Modules.Auth.JwtAuth.Exercise;

public sealed record JwtValidationResult(bool Succeeded, ClaimsPrincipal? Principal, string? Error)
{
    public static JwtValidationResult Success(ClaimsPrincipal principal) => new(true, principal, null);
    public static JwtValidationResult Failure(string error) => new(false, null, error);
}

/// <summary>
/// Issues and validates first-party JSON Web Tokens signed with a shared secret
/// (HMAC-SHA256). The same service that mints the token also verifies it, so there
/// is no identity provider, discovery document, or public-key download involved.
/// </summary>
public sealed class JwtTokenService(IOptions<JwtOptions> options)
{
    private readonly JwtOptions options = options.Value;
    private readonly JwtSecurityTokenHandler tokenHandler = new();

    public string IssueToken(string subject, IEnumerable<Claim>? extraClaims = null)
    {
        var credentials = new SigningCredentials(options.CreateSecurityKey(), SecurityAlgorithms.HmacSha256);
        var now = DateTime.UtcNow;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, subject),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        if (extraClaims is not null)
        {
            claims.AddRange(extraClaims);
        }

        var token = new JwtSecurityToken(
            options.Issuer,
            options.Audience,
            claims,
            now,
            now.Add(options.TokenLifetime),
            credentials);

        return tokenHandler.WriteToken(token);
    }

    public JwtValidationResult Validate(string? authorizationHeader)
    {
        var token = ReadBearerToken(authorizationHeader);
        if (token is null)
        {
            return JwtValidationResult.Failure("Missing bearer token.");
        }

        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = options.Issuer,
            ValidateAudience = true,
            ValidAudience = options.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = options.CreateSecurityKey(),
            RequireSignedTokens = true,
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            // Pin the algorithm so a token cannot downgrade to "none" or swap schemes.
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256]
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, parameters, out _);
            return JwtValidationResult.Success(principal);
        }
        catch (SecurityTokenException ex)
        {
            return JwtValidationResult.Failure(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return JwtValidationResult.Failure(ex.Message);
        }
    }

    private static string? ReadBearerToken(string? authorizationHeader)
    {
        const string prefix = "Bearer ";
        return authorizationHeader?.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) == true
            ? authorizationHeader[prefix.Length..].Trim()
            : null;
    }
}
