using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace AzureFunctionsFundamentals.Modules.Auth.BasicAuth.Examples;

public sealed class WhoAmIFunction(BasicAuthenticator authenticator)
{
    [Function("WhoAmI")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "whoami")] HttpRequest request)
    {
        var result = authenticator.Authenticate(request.Headers.Authorization.FirstOrDefault());
        if (!result.Succeeded || result.Principal is null)
        {
            // Tell the client which scheme to use, per RFC 7235.
            request.HttpContext.Response.Headers.WWWAuthenticate = "Basic realm=\"functions\", charset=\"UTF-8\"";
            return new UnauthorizedObjectResult(new { error = result.Error });
        }

        return new OkObjectResult(new { user = result.Principal.Identity?.Name });
    }
}
