using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureFunctionsFundamentals.Modules.HttpTrigger.Examples;

public sealed class PingFunctions(ILogger<PingFunctions> logger)
{
    [Function("Ping")]
    public IActionResult Ping([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ping")] HttpRequest request)
    {
        logger.LogInformation("Ping called from {RemoteIp}", request.HttpContext.Connection.RemoteIpAddress);
        return new OkObjectResult(new { status = "ok" });
    }

    [Function("Hello")]
    public IActionResult Hello([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "hello")] HttpRequest request)
    {
        var name = request.Query["name"].ToString();
        if (string.IsNullOrWhiteSpace(name))
        {
            name = "Functions learner";
        }

        logger.LogInformation("Hello called for {Name}", name);
        return new OkObjectResult(new { message = $"Hello, {name}!" });
    }
}
