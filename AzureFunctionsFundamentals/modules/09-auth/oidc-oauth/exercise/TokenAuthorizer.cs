using System.Security.Claims;

namespace AzureFunctionsFundamentals.Modules.Auth.OidcOAuth.Exercise;

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

public sealed class TokenAuthorizer(ITokenValidator tokenValidator)
{
    // TODO: Implement bearer-token authorization.
    // - Validate the bearer token with ITokenValidator.
    // - Return Unauthorized when token validation fails.
    // - Delegate successful principals to AuthorizeClaims, following README.md.
    public async Task<AuthorizationResult> AuthorizeBearerTokenAsync(
        string? authorizationHeader,
        string requiredPermission,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException("TODO: implement this method.");
    }

    // TODO: Implement claims authorization.
    // - Check the principal for the required scope or role accepted by the module.
    // - Return Authorized or Forbidden with the expected error details from README.md.
    public AuthorizationResult AuthorizeClaims(ClaimsPrincipal principal, string requiredPermission)
    {
        throw new NotImplementedException("TODO: implement this method.");
    }

    private static bool HasScopeOrRole(ClaimsPrincipal principal, string requiredPermission)
    {
        return principal.Claims.Any(claim =>
            (claim.Type is "scope" or "scp" && claim.Value
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Contains(requiredPermission, StringComparer.OrdinalIgnoreCase)) ||
            (claim.Type is "role" or "roles" && string.Equals(claim.Value, requiredPermission, StringComparison.OrdinalIgnoreCase)));
    }
}
