using System.Text.Json;
using AzureFunctionsFundamentals.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureFunctionsFundamentals.Modules.CosmosDbServiceBusPipeline.Exercise;

public sealed class EnrichOrderFunction(CosmosOrderEnricher enricher, ILogger<EnrichOrderFunction> logger)
{
    [Function(nameof(EnrichOrderFunction))]
    [CosmosDBOutput("LearningDb", "orders", Connection = "CosmosDbConnection")]
    public async Task<EnrichedOrder> RunAsync(
        [ServiceBusTrigger("enrich-in", Connection = "ServiceBusConnection")] string message,
        CancellationToken cancellationToken)
    {
        var order = JsonSerializer.Deserialize<Order>(message)
            ?? throw new InvalidOperationException("The Service Bus message did not contain a valid order.");

        EnrichedOrder enriched = await enricher.EnrichAsync(order, cancellationToken);
        logger.LogInformation("Enriched order {OrderId} for customer {CustomerId}.", enriched.Id, enriched.CustomerId);
        return enriched;
    }
}
