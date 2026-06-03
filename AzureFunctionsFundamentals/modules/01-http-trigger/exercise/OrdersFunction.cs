using AzureFunctionsFundamentals.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace AzureFunctionsFundamentals.Modules.HttpTrigger.Exercise;

public sealed class OrdersFunction(OrderService orderService)
{
    [Function("CreateOrder")]
    public async Task<IActionResult> CreateOrderAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orders")] HttpRequest request,
        CancellationToken cancellationToken)
    {
        var order = await request.ReadFromJsonAsync<Order>(cancellationToken);
        var result = orderService.Create(order);

        if (!result.IsValid)
        {
            return new BadRequestObjectResult(new { errors = result.Errors });
        }

        return new CreatedResult($"/api/orders/{result.Order!.Id}", result.Order);
    }
}
