using AzureFunctionsFundamentals.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace AzureFunctionsFundamentals.Modules.CosmosDbReadWrite.Exercise;

public sealed class OrdersApiFunctions(OrderService service)
{
    // TODO: Implement the UpsertAsync function.
    // Hints:
    // - Trigger: Use [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orders")] to expose the POST endpoint.
    // - Signature: Returns a Task<IActionResult> representing the async result (such as CreatedResult or OkObjectResult, or BadRequestObjectResult on validation failure).
    // - Logic: Read the Order from JSON request body, pass to service.SaveAsync, and return a CreatedResult with the appropriate route or an OkObjectResult.
    [Function("UpsertOrder")]
    public Task<IActionResult> UpsertAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orders")] HttpRequest request,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException("TODO: Implement the UpsertAsync CosmosDB HTTP endpoint according to the exercise guidelines.");
    }

    // TODO: Implement the GetByCustomerAsync function.
    // Hints:
    // - Trigger: Use [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orders/{customerId:int}")] to expose the GET endpoint with route parameter.
    // - Signature: Returns a Task<IActionResult> (such as OkObjectResult or BadRequestObjectResult on validation failure).
    // - Logic: Query the orders by customerId using service.QueryByCustomerAsync, and return OkObjectResult or BadRequestObjectResult.
    [Function("GetOrdersByCustomer")]
    public Task<IActionResult> GetByCustomerAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orders/{customerId:int}")] HttpRequest request,
        int customerId,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException("TODO: Implement the GetByCustomerAsync CosmosDB HTTP endpoint according to the exercise guidelines.");
    }
}
