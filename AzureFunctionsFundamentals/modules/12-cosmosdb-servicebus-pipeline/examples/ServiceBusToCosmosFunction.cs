using System.Text.Json;
using AzureFunctionsFundamentals.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureFunctionsFundamentals.Modules.CosmosDbServiceBusPipeline.Examples;

public sealed class ServiceBusToCosmosFunction(ILogger<ServiceBusToCosmosFunction> logger)
{
    [Function(nameof(ServiceBusToCosmosFunction))]
    [CosmosDBOutput("LearningDb", "audit", Connection = "CosmosDbConnection")]
    public EnrichedOrder Run([ServiceBusTrigger("enrich-in", Connection = "ServiceBusConnection")] string message)
    {
        var order = JsonSerializer.Deserialize<Order>(message)
            ?? throw new InvalidOperationException("The message did not contain a valid order.");

        logger.LogInformation("Preparing enriched order {OrderId} for customer {CustomerId}.", order.Id, order.CustomerId);
        var enrichedOrder = new EnrichedOrder
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            CustomerName = "Unknown",
            CustomerTier = "Unknown",
            Product = order.Product,
            Quantity = order.Quantity,
            Total = order.Total
        };

        logger.LogInformation("Writing enriched order {OrderId} to Cosmos DB output.", order.Id);
        return enrichedOrder;
    }
}
