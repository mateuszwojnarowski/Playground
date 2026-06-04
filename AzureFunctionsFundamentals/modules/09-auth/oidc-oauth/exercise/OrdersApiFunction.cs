using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;

namespace AzureFunctionsFundamentals.Modules.Auth.OidcOAuth.Exercise;

public sealed class OrdersApiFunction(TokenAuthorizer authorizer, IOptions<AuthOptions> options)
{
    [Function("Health")]
    public IActionResult Health(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequest request)
    {
        return new OkObjectResult(new { status = "ok" });
    }

    // TODO: Implement the GetOrdersAsync function.
    // Hints:
    // - Trigger: Use [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orders")] to expose the GET endpoint.
    // - Signature: Returns a Task<IActionResult> representing the async result (such as OkObjectResult, UnauthorizedObjectResult, Forbidden, etc.).
    // - Logic: Read the required permission from configuration/options, authorize the bearer token using authorizer, and return the corresponding HTTP response.
    [Function("GetOrders")]
    public Task<IActionResult> GetOrdersAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orders")] HttpRequest request,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException("TODO: Implement the Orders API function with OIDC/OAuth authentication according to the exercise guidelines.");
    }
}
