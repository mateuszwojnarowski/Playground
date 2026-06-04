using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureFunctionsFundamentals.Modules.Auth.OidcOAuth.Examples;

public sealed class ProtectedClaimsFunction(ITokenValidator tokenValidator, ILogger<ProtectedClaimsFunction> logger)
{
    [Function("ProtectedClaims")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "claims")] HttpRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await tokenValidator.ValidateAsync(request.Headers.Authorization.FirstOrDefault(), cancellationToken);
        if (!validation.Succeeded || validation.Principal is null)
        {
            logger.LogWarning("OIDC claims request failed authorization.");
            return new UnauthorizedObjectResult(new { error = validation.Error });
        }

        var principal = validation.Principal;
        var subject = principal.FindFirstValue("sub") ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var scopes = ScopeReader.ReadScopes(principal);
        logger.LogInformation("OIDC claims returned for subject {Subject} with {ScopeCount} scopes.", subject, scopes.Length);
        return new OkObjectResult(new
        {
            subject,
            name = principal.FindFirstValue("name") ?? principal.Identity?.Name,
            scopes,
            roles = principal.FindAll("role").Select(c => c.Value).ToArray()
        });
    }
}

internal static class ScopeReader
{
    public static string[] ReadScopes(ClaimsPrincipal principal) => principal.Claims
        .Where(c => c.Type is "scope" or "scp")
        .SelectMany(c => c.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .Order(StringComparer.OrdinalIgnoreCase)
        .ToArray();
}
