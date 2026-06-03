using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace AzureFunctionsFundamentals.Modules.Auth.OidcOAuth.Examples;

public sealed class ProtectedClaimsFunction(ITokenValidator tokenValidator)
{
    [Function("ProtectedClaims")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "claims")] HttpRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await tokenValidator.ValidateAsync(request.Headers.Authorization.FirstOrDefault(), cancellationToken);
        if (!validation.Succeeded || validation.Principal is null)
        {
            return new UnauthorizedObjectResult(new { error = validation.Error });
        }

        var principal = validation.Principal;
        return new OkObjectResult(new
        {
            subject = principal.FindFirstValue("sub") ?? principal.FindFirstValue(ClaimTypes.NameIdentifier),
            name = principal.FindFirstValue("name") ?? principal.Identity?.Name,
            scopes = ScopeReader.ReadScopes(principal),
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
