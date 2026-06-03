using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AzureFunctionsFundamentals.Modules.Auth.JwtAuth.Examples;

public sealed class JwtOptions
{
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public required string SigningKey { get; init; }
    public TimeSpan TokenLifetime { get; init; } = TimeSpan.FromHours(1);

    public SymmetricSecurityKey CreateSecurityKey() => new(Encoding.UTF8.GetBytes(SigningKey));

    public static JwtOptions FromConfiguration(IConfiguration configuration)
    {
        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];
        var signingKey = configuration["Jwt:SigningKey"];

        if (string.IsNullOrWhiteSpace(issuer))
        {
            throw new InvalidOperationException("Jwt:Issuer must be configured.");
        }

        if (string.IsNullOrWhiteSpace(audience))
        {
            throw new InvalidOperationException("Jwt:Audience must be configured.");
        }

        // HS256 needs a key of at least 256 bits (32 bytes).
        if (string.IsNullOrWhiteSpace(signingKey) || Encoding.UTF8.GetByteCount(signingKey) < 32)
        {
            throw new InvalidOperationException("Jwt:SigningKey must be configured and at least 32 bytes long.");
        }

        return new JwtOptions
        {
            Issuer = issuer,
            Audience = audience,
            SigningKey = signingKey
        };
    }
}
