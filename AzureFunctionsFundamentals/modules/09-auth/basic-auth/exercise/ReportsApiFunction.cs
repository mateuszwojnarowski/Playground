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

    [Function("GetReports")]
    public IActionResult GetReports(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "reports")] HttpRequest request)
    {
        var result = authenticator.Authenticate(request.Headers.Authorization.FirstOrDefault());
        if (!result.Succeeded || result.Principal is null)
        {
            request.HttpContext.Response.Headers.WWWAuthenticate = "Basic realm=\"reports\", charset=\"UTF-8\"";
            return new UnauthorizedObjectResult(new { error = result.Error });
        }

        return new OkObjectResult(new
        {
            owner = result.Principal.Identity?.Name,
            reports = new[]
            {
                new { id = "R-1", title = "Q1 revenue" },
                new { id = "R-2", title = "Open incidents" }
            }
        });
    }
}
