using Microsoft.Extensions.Configuration;

namespace AzureFunctionsFundamentals.Modules.AuthOidcOAuth2.Exercise;

public sealed class AuthOptions
{
    public required string Authority { get; init; }
    public required string Audience { get; init; }
    public string? Issuer { get; init; }
    public string RequiredPermission { get; init; } = "orders.read";
    public bool RequireHttpsMetadata { get; init; } = true;

    public static AuthOptions FromConfiguration(IConfiguration configuration)
    {
        var authority = configuration["Auth:Authority"];
        var audience = configuration["Auth:Audience"];

        if (string.IsNullOrWhiteSpace(authority))
        {
            throw new InvalidOperationException("Auth:Authority must be configured.");
        }

        if (string.IsNullOrWhiteSpace(audience))
        {
            throw new InvalidOperationException("Auth:Audience must be configured.");
        }

        return new AuthOptions
        {
            Authority = authority.TrimEnd('/'),
            Audience = audience,
            Issuer = configuration["Auth:Issuer"],
            RequiredPermission = configuration["Auth:RequiredPermission"] ?? "orders.read",
            RequireHttpsMetadata = !bool.TryParse(configuration["Auth:RequireHttpsMetadata"], out var requireHttps) || requireHttps
        };
    }
}
