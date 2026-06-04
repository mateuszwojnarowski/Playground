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

    // TODO: Implement the GetDocuments function.
    // Hints:
    // - Trigger: Use [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documents")] to expose the GET endpoint.
    // - Signature: Returns an IActionResult (such as OkObjectResult, UnauthorizedObjectResult, Forbidden, etc.).
    // - Logic: Read the configuration to get the required role, authorize the request using the authorizer, and return the corresponding HTTP response (Unauthorized/Forbidden/Ok with documents list).
    [Function("GetDocuments")]
    public IActionResult GetDocuments(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documents")] HttpRequest request)
    {
        throw new NotImplementedException("TODO: Implement the Documents API function with JWT authentication according to the exercise guidelines.");
    }
}
