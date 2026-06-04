using AzureFunctionsFundamentals.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace AzureFunctionsFundamentals.Modules.CosmosDbReadWrite.Examples;

public sealed class OrderBindingFunctions(ILogger<OrderBindingFunctions> logger)
{
    [Function("CreateOrderWithBinding")]
    public async Task<CreateOrderOutput> CreateAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "binding/orders")] HttpRequest request,
        CancellationToken cancellationToken)
    {
        var order = await request.ReadFromJsonAsync<Order>(cancellationToken);
        if (order is null || order.CustomerId <= 0)
        {
            logger.LogWarning("CreateOrderWithBinding received an invalid order payload.");
            return new CreateOrderOutput { HttpResponse = new BadRequestObjectResult(new { error = "A valid order payload is required." }) };
        }

        var document = order with { Id = string.IsNullOrWhiteSpace(order.Id) ? Guid.NewGuid().ToString() : order.Id };
        logger.LogInformation("Creating order document {OrderId} for customer {CustomerId}.", document.Id, document.CustomerId);
        return new CreateOrderOutput
        {
            Order = document,
            HttpResponse = new CreatedResult($"/api/binding/orders/{document.CustomerId}/{document.Id}", document)
        };
    }

    [Function("ReadOrderWithBinding")]
    public IActionResult Read(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "binding/orders/{customerId:int}/{id}")] HttpRequest request,
        string id,
        int customerId,
        [CosmosDBInput("LearningDb", "orders", Connection = "CosmosDbConnection", Id = "{id}", PartitionKey = "{customerId}")] Order? order)
    {
        if (order is null)
        {
            logger.LogWarning("Order {OrderId} for customer {CustomerId} was not found.", id, customerId);
            return new NotFoundResult();
        }

        logger.LogInformation("Order {OrderId} for customer {CustomerId} was read with binding.", id, customerId);
        return new OkObjectResult(order);
    }
}

public sealed class CreateOrderOutput
{
    [CosmosDBOutput("LearningDb", "orders", Connection = "CosmosDbConnection")]
    public Order? Order { get; init; }

    [HttpResult]
    public IActionResult HttpResponse { get; init; } = new OkResult();
}
