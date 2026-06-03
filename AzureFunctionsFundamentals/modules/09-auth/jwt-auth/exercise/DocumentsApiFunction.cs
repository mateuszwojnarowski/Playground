using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;

namespace AzureFunctionsFundamentals.Modules.Auth.JwtAuth.Exercise;

public sealed class DocumentsApiFunction(DocumentsAuthorizer authorizer, IConfiguration configuration)
{
    [Function("Health")]
    public IActionResult Health(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequest request)
    {
        return new OkObjectResult(new { status = "ok" });
    }

    [Function("GetDocuments")]
    public IActionResult GetDocuments(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documents")] HttpRequest request)
    {
        var requiredRole = configuration["Jwt:RequiredRole"] ?? "documents.read";
        var result = authorizer.Authorize(request.Headers.Authorization.FirstOrDefault(), requiredRole);

        if (result.Status == AuthorizationStatus.Unauthorized)
        {
            return new UnauthorizedObjectResult(new { error = result.Error });
        }

        if (result.Status == AuthorizationStatus.Forbidden)
        {
            return new ObjectResult(new { error = result.Error }) { StatusCode = StatusCodes.Status403Forbidden };
        }

        var principal = result.Principal!;
        return new OkObjectResult(new
        {
            owner = principal.FindFirstValue("sub") ?? principal.FindFirstValue(ClaimTypes.NameIdentifier),
            documents = new[]
            {
                new { id = "DOC-1", title = "Onboarding guide" },
                new { id = "DOC-2", title = "Runbook" }
            }
        });
    }
}
