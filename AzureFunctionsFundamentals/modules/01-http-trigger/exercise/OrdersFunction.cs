using AzureFunctionsFundamentals.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace AzureFunctionsFundamentals.Modules.HttpTrigger.Exercise;

public sealed class OrdersFunction(OrderService orderService)
{
    // TODO: Implement the CreateOrder function.
    // Hints:
    // - Trigger: Use [HttpTrigger] attribute configured for AuthorizationLevel.Anonymous, "post" method, and route "orders".
    // - Parameter: Needs HttpRequest to read the JSON body and a CancellationToken.
    // - Output/Return: Returns Task<IActionResult> representing the HTTP response.
    // - Logic: Read the Order payload from HttpRequest, call orderService.Create(order), and return BadRequestObjectResult with errors if invalid, or CreatedResult if valid.
    [Function("CreateOrder")]
    public async Task<IActionResult> CreateOrderAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orders")] HttpRequest request,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException("TODO: Implement the HTTP-triggered function according to the exercise guidelines.");
    }
}
