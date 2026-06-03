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
    public async Task<AuthorizationResult> AuthorizeBearerTokenAsync(
        string? authorizationHeader,
        string requiredPermission,
        CancellationToken cancellationToken)
    {
        var validation = await tokenValidator.ValidateAsync(authorizationHeader, cancellationToken);
        if (!validation.Succeeded || validation.Principal is null)
        {
            return new AuthorizationResult(AuthorizationStatus.Unauthorized, null, validation.Error);
        }

        return AuthorizeClaims(validation.Principal, requiredPermission);
    }

    public AuthorizationResult AuthorizeClaims(ClaimsPrincipal principal, string requiredPermission)
    {
        if (HasScopeOrRole(principal, requiredPermission))
        {
            return new AuthorizationResult(AuthorizationStatus.Authorized, principal, null);
        }

        return new AuthorizationResult(
            AuthorizationStatus.Forbidden,
            principal,
            $"The token is valid but does not contain the required '{requiredPermission}' scope or role.");
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
