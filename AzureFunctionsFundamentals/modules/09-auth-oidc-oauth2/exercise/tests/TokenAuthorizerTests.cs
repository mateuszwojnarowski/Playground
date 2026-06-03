using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using AzureFunctionsFundamentals.Modules.AuthOidcOAuth2.Exercise;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace AuthOidcExercise.Tests;

public sealed class TokenAuthorizerTests
{
    [Fact]
    public void ValidClaimsWithRequiredScopeAllowAccess()
    {
        var authorizer = new TokenAuthorizer(new NeverCalledTokenValidator());
        var principal = PrincipalWith(new Claim("scp", "orders.read orders.write"));

        var result = authorizer.AuthorizeClaims(principal, "orders.read");

        Assert.Equal(AuthorizationStatus.Authorized, result.Status);
        Assert.True(result.Succeeded);
    }

    [Fact]
    public void ValidClaimsMissingRequiredScopeDenyWithForbidden()
    {
        var authorizer = new TokenAuthorizer(new NeverCalledTokenValidator());
        var principal = PrincipalWith(new Claim("scp", "orders.write"));

        var result = authorizer.AuthorizeClaims(principal, "orders.read");

        Assert.Equal(AuthorizationStatus.Forbidden, result.Status);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task ExpiredTokenDenyWithUnauthorized()
    {
        using var rsa = RSA.Create(2048);
        var validator = ValidatorFor(rsa, issuer: "https://issuer.example", audience: "orders-api");
        var authorizer = new TokenAuthorizer(validator);
        var token = CreateToken(rsa, "https://issuer.example", "orders-api", DateTime.UtcNow.AddHours(-2), DateTime.UtcNow.AddHours(-1));

        var result = await authorizer.AuthorizeBearerTokenAsync(string.Concat("B", "earer ", token), "orders.read", CancellationToken.None);

        Assert.Equal(AuthorizationStatus.Unauthorized, result.Status);
        Assert.Equal(401, result.StatusCode);
    }

    [Fact]
    public async Task InvalidIssuerDenyWithUnauthorized()
    {
        using var rsa = RSA.Create(2048);
        var validator = ValidatorFor(rsa, issuer: "https://expected-issuer.example", audience: "orders-api");
        var authorizer = new TokenAuthorizer(validator);
        var token = CreateToken(rsa, "https://wrong-issuer.example", "orders-api", DateTime.UtcNow.AddMinutes(-5), DateTime.UtcNow.AddMinutes(30));

        var result = await authorizer.AuthorizeBearerTokenAsync(string.Concat("B", "earer ", token), "orders.read", CancellationToken.None);

        Assert.Equal(AuthorizationStatus.Unauthorized, result.Status);
    }

    [Fact]
    public async Task InvalidAudienceDenyWithUnauthorized()
    {
        using var rsa = RSA.Create(2048);
        var validator = ValidatorFor(rsa, issuer: "https://issuer.example", audience: "orders-api");
        var authorizer = new TokenAuthorizer(validator);
        var token = CreateToken(rsa, "https://issuer.example", "wrong-api", DateTime.UtcNow.AddMinutes(-5), DateTime.UtcNow.AddMinutes(30));

        var result = await authorizer.AuthorizeBearerTokenAsync(string.Concat("B", "earer ", token), "orders.read", CancellationToken.None);

        Assert.Equal(AuthorizationStatus.Unauthorized, result.Status);
    }

    private static ClaimsPrincipal PrincipalWith(params Claim[] claims) =>
        new(new ClaimsIdentity(claims.Concat(new[] { new Claim("sub", "user-123") }), "Test"));

    private static JwtBearerValidator ValidatorFor(RSA rsa, string issuer, string audience)
    {
        var key = new RsaSecurityKey(rsa.ExportParameters(false)) { KeyId = "test-key" };
        return new JwtBearerValidator(
            Options.Create(new AuthOptions { Authority = issuer, Audience = audience }),
            new StaticSigningKeyProvider(new SigningKeySet(issuer, new[] { key })));
    }

    private static string CreateToken(RSA rsa, string issuer, string audience, DateTime notBefore, DateTime expires)
    {
        var signingKey = new RsaSecurityKey(rsa.ExportParameters(true)) { KeyId = "test-key" };
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.RsaSha256);
        var token = new JwtSecurityToken(
            issuer,
            audience,
            new[] { new Claim("sub", "user-123"), new Claim("scp", "orders.read") },
            notBefore,
            expires,
            credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed class StaticSigningKeyProvider(SigningKeySet keySet) : ISigningKeyProvider
    {
        public Task<SigningKeySet> GetSigningKeysAsync(CancellationToken cancellationToken) => Task.FromResult(keySet);
    }

    private sealed class NeverCalledTokenValidator : ITokenValidator
    {
        public Task<AzureFunctionsFundamentals.Modules.AuthOidcOAuth2.Exercise.TokenValidationResult> ValidateAsync(string? authorizationHeader, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("Claims-only authorizer tests should not validate raw tokens.");
    }
}
