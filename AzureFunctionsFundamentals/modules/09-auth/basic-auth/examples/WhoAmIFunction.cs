using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureFunctionsFundamentals.Modules.Auth.BasicAuth.Examples;

public sealed class WhoAmIFunction(BasicAuthenticator authenticator, ILogger<WhoAmIFunction> logger)
{
    [Function("WhoAmI")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "whoami")] HttpRequest request)
    {
        logger.LogInformation("WhoAmI requested from {RemoteIp}.", request.HttpContext.Connection.RemoteIpAddress);

        var result = authenticator.Authenticate(request.Headers.Authorization.FirstOrDefault());
        if (!result.Succeeded || result.Principal is null)
        {
            logger.LogWarning("Basic authentication failed for WhoAmI.");
            request.HttpContext.Response.Headers.WWWAuthenticate = "Basic realm=\"functions\", charset=\"UTF-8\"";
            return new UnauthorizedObjectResult(new { error = result.Error });
        }

        logger.LogInformation("Basic authentication succeeded for {UserName}.", result.Principal.Identity?.Name);
        return new OkObjectResult(new { user = result.Principal.Identity?.Name });
    }
}
