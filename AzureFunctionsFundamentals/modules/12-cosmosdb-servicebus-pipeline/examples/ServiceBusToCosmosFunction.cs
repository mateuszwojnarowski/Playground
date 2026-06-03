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

        logger.LogInformation("Writing minimal enriched order for {OrderId}.", order.Id);
        return new EnrichedOrder
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            CustomerName = "Unknown",
            CustomerTier = "Unknown",
            Product = order.Product,
            Quantity = order.Quantity,
            Total = order.Total
        };
    }
}
