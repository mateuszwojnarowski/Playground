using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace AzureFunctionsFundamentals.Modules.Auth.BasicAuth.Exercise;

public sealed class ReportsApiFunction(BasicAuthenticator authenticator)
{
    [Function("Health")]
    public IActionResult Health(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequest request)
    {
        return new OkObjectResult(new { status = "ok" });
    }

    // TODO: Implement the GetReports function.
    // Hints:
    // - Trigger: Use [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "reports")] to expose the GET endpoint.
    // - Signature: Returns an IActionResult (such as OkObjectResult, UnauthorizedObjectResult, etc.).
    // - Logic: Extract the Authorization header, pass it to basic authenticator, and return Unauthorized with WWW-Authenticate header if authentication fails. Return Ok with reports data if it succeeds.
    [Function("GetReports")]
    public IActionResult GetReports(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "reports")] HttpRequest request)
    {
        throw new NotImplementedException("TODO: Implement the Reports API function with Basic authentication according to the exercise guidelines.");
    }
}
