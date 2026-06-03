using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace AzureFunctionsFundamentals.Modules.HttpTrigger.Examples;

public sealed class PingFunctions
{
    [Function("Ping")]
    public IActionResult Ping([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ping")] HttpRequest request)
    {
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

        return new OkObjectResult(new { message = $"Hello, {name}!" });
    }
}
