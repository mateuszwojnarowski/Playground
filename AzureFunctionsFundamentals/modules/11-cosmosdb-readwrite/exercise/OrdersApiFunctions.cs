using AzureFunctionsFundamentals.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace AzureFunctionsFundamentals.Modules.CosmosDbReadWrite.Exercise;

public sealed class OrdersApiFunctions(OrderService service)
{
    [Function("UpsertOrder")]
    public async Task<IActionResult> UpsertAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orders")] HttpRequest request,
        CancellationToken cancellationToken)
    {
        var order = await request.ReadFromJsonAsync<Order>(cancellationToken);
        SaveOrderResult result = await service.SaveAsync(order, cancellationToken);
        if (!result.IsValid)
        {
            return new BadRequestObjectResult(new { errors = result.Errors });
        }

        return result.Created
            ? new CreatedResult($"/api/orders/{result.Order!.CustomerId}/{result.Order.Id}", result.Order)
            : new OkObjectResult(result.Order);
    }

    [Function("GetOrdersByCustomer")]
    public async Task<IActionResult> GetByCustomerAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orders/{customerId:int}")] HttpRequest request,
        int customerId,
        CancellationToken cancellationToken)
    {
        QueryOrdersResult result = await service.QueryByCustomerAsync(customerId, cancellationToken);
        return result.IsValid ? new OkObjectResult(result.Orders) : new BadRequestObjectResult(new { errors = result.Errors });
    }
}
