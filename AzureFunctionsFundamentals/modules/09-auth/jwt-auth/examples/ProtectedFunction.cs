using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace AzureFunctionsFundamentals.Modules.Auth.JwtAuth.Examples;

public sealed class ProtectedFunction(JwtTokenService tokenService)
{
    [Function("Protected")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "protected")] HttpRequest request)
    {
        var result = tokenService.Validate(request.Headers.Authorization.FirstOrDefault());
        if (!result.Succeeded || result.Principal is null)
        {
            return new UnauthorizedObjectResult(new { error = result.Error });
        }

        var principal = result.Principal;
        return new OkObjectResult(new
        {
            subject = principal.FindFirstValue("sub") ?? principal.FindFirstValue(ClaimTypes.NameIdentifier),
            roles = principal.FindAll("role").Select(c => c.Value).ToArray()
        });
    }
}
