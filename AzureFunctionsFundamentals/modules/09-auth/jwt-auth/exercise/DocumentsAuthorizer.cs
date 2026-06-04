using System.Security.Claims;

namespace AzureFunctionsFundamentals.Modules.Auth.JwtAuth.Exercise;

public enum AuthorizationStatus
{
    Authorized,
    Unauthorized,
    Forbidden
}

public sealed record AuthorizationResult(AuthorizationStatus Status, ClaimsPrincipal? Principal, string? Error)
{
    public bool Succeeded => Status == AuthorizationStatus.Authorized;
    public int StatusCode => Status switch
    {
        AuthorizationStatus.Authorized => 200,
        AuthorizationStatus.Unauthorized => 401,
        AuthorizationStatus.Forbidden => 403,
        _ => 500
    };
}

/// <summary>
/// Validates a first-party JWT and then checks that it carries the role required
/// for the operation. Validation and the claim check are separated so each can be
/// unit-tested directly.
/// </summary>
public sealed class DocumentsAuthorizer(JwtTokenService tokenService)
{
    // TODO: Implement JWT authorization.
    // - Validate the bearer token with JwtTokenService.
    // - Return Unauthorized when token validation fails.
    // - Delegate successful principals to AuthorizeClaims, following README.md.
    public AuthorizationResult Authorize(string? authorizationHeader, string requiredRole)
    {
        throw new NotImplementedException("TODO: implement this method.");
    }

    // TODO: Implement JWT claim authorization.
    // - Check the principal for the required role using the accepted claim types.
    // - Return Authorized or Forbidden with the expected error details from README.md.
    public AuthorizationResult AuthorizeClaims(ClaimsPrincipal principal, string requiredRole)
    {
        throw new NotImplementedException("TODO: implement this method.");
    }
}
