using System.Security.Claims;
using AzureFunctionsFundamentals.Modules.Auth.JwtAuth.Exercise;
using Microsoft.Extensions.Options;
using Xunit;

namespace JwtAuthExercise.Tests;

public sealed class DocumentsAuthorizerTests
{
    private const string Issuer = "test-issuer";
    private const string Audience = "documents-api";
    private const string SigningKey = "unit-test-signing-key-which-is-32-bytes-or-more!";
    private const string RequiredRole = "documents.read";

    private static JwtTokenService CreateTokenService(
        string issuer = Issuer,
        string audience = Audience,
        string signingKey = SigningKey)
    {
        var options = Options.Create(new JwtOptions
        {
            Issuer = issuer,
            Audience = audience,
            SigningKey = signingKey
        });
        return new JwtTokenService(options);
    }

    private static string Bearer(string token) => "Bearer " + token;

    [Fact]
    public void ValidTokenWithRequiredRoleIsAuthorized()
    {
        var service = CreateTokenService();
        var authorizer = new DocumentsAuthorizer(service);
        var token = service.IssueToken("user-1", [new Claim("role", RequiredRole)]);

        var result = authorizer.Authorize(Bearer(token), RequiredRole);

        Assert.Equal(AuthorizationStatus.Authorized, result.Status);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public void ValidTokenMissingRoleIsForbidden()
    {
        var service = CreateTokenService();
        var authorizer = new DocumentsAuthorizer(service);
        var token = service.IssueToken("user-1", [new Claim("role", "documents.write")]);

        var result = authorizer.Authorize(Bearer(token), RequiredRole);

        Assert.Equal(AuthorizationStatus.Forbidden, result.Status);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public void MissingTokenIsUnauthorized()
    {
        var authorizer = new DocumentsAuthorizer(CreateTokenService());

        var result = authorizer.Authorize(null, RequiredRole);

        Assert.Equal(AuthorizationStatus.Unauthorized, result.Status);
        Assert.Equal(401, result.StatusCode);
    }

    [Fact]
    public void TokenSignedWithDifferentKeyIsUnauthorized()
    {
        var issuingService = CreateTokenService(signingKey: "a-totally-different-signing-key-32-bytes-x!");
        var validatingAuthorizer = new DocumentsAuthorizer(CreateTokenService());
        var token = issuingService.IssueToken("user-1", [new Claim("role", RequiredRole)]);

        var result = validatingAuthorizer.Authorize(Bearer(token), RequiredRole);

        Assert.Equal(AuthorizationStatus.Unauthorized, result.Status);
    }

    [Fact]
    public void TokenForWrongAudienceIsUnauthorized()
    {
        var issuingService = CreateTokenService(audience: "some-other-api");
        var validatingAuthorizer = new DocumentsAuthorizer(CreateTokenService());
        var token = issuingService.IssueToken("user-1", [new Claim("role", RequiredRole)]);

        var result = validatingAuthorizer.Authorize(Bearer(token), RequiredRole);

        Assert.Equal(AuthorizationStatus.Unauthorized, result.Status);
    }
}
