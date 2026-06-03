using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;

namespace AzureFunctionsFundamentals.Modules.AuthOidcOAuth2.Exercise;

public sealed class OrdersApiFunction(TokenAuthorizer authorizer, IOptions<AuthOptions> options)
{
    [Function("Health")]
    public IActionResult Health(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequest request)
    {
        return new OkObjectResult(new { status = "ok" });
    }

    [Function("GetOrders")]
    public async Task<IActionResult> GetOrdersAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orders")] HttpRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authorizer.AuthorizeBearerTokenAsync(
            request.Headers.Authorization.FirstOrDefault(),
            options.Value.RequiredPermission,
            cancellationToken);

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
            caller = principal.FindFirstValue("sub") ?? principal.FindFirstValue(ClaimTypes.NameIdentifier),
            orders = new[]
            {
                new { id = "A100", customer = "Contoso", total = 123.45m },
                new { id = "B200", customer = "Fabrikam", total = 67.89m }
            }
        });
    }
}
