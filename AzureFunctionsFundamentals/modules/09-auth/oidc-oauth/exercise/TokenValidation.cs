using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace AzureFunctionsFundamentals.Modules.Auth.OidcOAuth.Exercise;

public sealed record SigningKeySet(string Issuer, ICollection<SecurityKey> SigningKeys);

public interface ISigningKeyProvider
{
    Task<SigningKeySet> GetSigningKeysAsync(CancellationToken cancellationToken);
}

public sealed class OidcSigningKeyProvider : ISigningKeyProvider
{
    private readonly ConfigurationManager<OpenIdConnectConfiguration> configurationManager;

    public OidcSigningKeyProvider(IOptions<AuthOptions> options)
    {
        var auth = options.Value;
        var metadataAddress = $"{auth.Authority}/.well-known/openid-configuration";
        var documentRetriever = new HttpDocumentRetriever { RequireHttps = auth.RequireHttpsMetadata };
        configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            metadataAddress,
            new OpenIdConnectConfigurationRetriever(),
            documentRetriever);
    }

    public async Task<SigningKeySet> GetSigningKeysAsync(CancellationToken cancellationToken)
    {
        var configuration = await configurationManager.GetConfigurationAsync(cancellationToken);
        return new SigningKeySet(configuration.Issuer, configuration.SigningKeys);
    }
}

public sealed record TokenValidationResult(bool Succeeded, ClaimsPrincipal? Principal, string? Error)
{
    public static TokenValidationResult Success(ClaimsPrincipal principal) => new(true, principal, null);
    public static TokenValidationResult Failure(string error) => new(false, null, error);
}

public interface ITokenValidator
{
    Task<TokenValidationResult> ValidateAsync(string? authorizationHeader, CancellationToken cancellationToken);
}

public sealed class JwtBearerValidator(IOptions<AuthOptions> options, ISigningKeyProvider signingKeyProvider) : ITokenValidator
{
    private readonly JwtSecurityTokenHandler tokenHandler = new();

    public async Task<TokenValidationResult> ValidateAsync(string? authorizationHeader, CancellationToken cancellationToken)
    {
        var token = ReadBearerToken(authorizationHeader);
        if (token is null)
        {
            return TokenValidationResult.Failure("Missing bearer token.");
        }

        var keySet = await signingKeyProvider.GetSigningKeysAsync(cancellationToken);
        var auth = options.Value;
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = auth.Issuer ?? keySet.Issuer,
            ValidateAudience = true,
            ValidAudience = auth.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = keySet.SigningKeys,
            RequireSignedTokens = true,
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.FromMinutes(2),
            NameClaimType = "name",
            RoleClaimType = "role"
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            return TokenValidationResult.Success(principal);
        }
        catch (SecurityTokenException ex)
        {
            return TokenValidationResult.Failure(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return TokenValidationResult.Failure(ex.Message);
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
