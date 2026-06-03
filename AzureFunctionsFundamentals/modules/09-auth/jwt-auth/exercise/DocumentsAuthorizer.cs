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
    public AuthorizationResult Authorize(string? authorizationHeader, string requiredRole)
    {
        var validation = tokenService.Validate(authorizationHeader);
        if (!validation.Succeeded || validation.Principal is null)
        {
            return new AuthorizationResult(AuthorizationStatus.Unauthorized, null, validation.Error);
        }

        return AuthorizeClaims(validation.Principal, requiredRole);
    }

    public AuthorizationResult AuthorizeClaims(ClaimsPrincipal principal, string requiredRole)
    {
        var hasRole = principal.FindAll("role")
            .Concat(principal.FindAll(ClaimTypes.Role))
            .Any(c => string.Equals(c.Value, requiredRole, StringComparison.OrdinalIgnoreCase));

        return hasRole
            ? new AuthorizationResult(AuthorizationStatus.Authorized, principal, null)
            : new AuthorizationResult(
                AuthorizationStatus.Forbidden,
                principal,
                $"The token is valid but does not contain the required '{requiredRole}' role.");
    }
}
