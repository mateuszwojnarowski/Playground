using System.Text.Json;
using AzureFunctionsFundamentals.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureFunctionsFundamentals.Modules.CosmosDbServiceBusPipeline.Exercise;

public sealed class EnrichOrderFunction(CosmosOrderEnricher enricher, ILogger<EnrichOrderFunction> logger)
{
    // TODO: Implement the EnrichOrderFunction.
    // Hints:
    // - Trigger: Use [ServiceBusTrigger("enrich-in", Connection = "ServiceBusConnection")] to receive input message as a string.
    // - Output: Use [CosmosDBOutput("LearningDb", "orders", Connection = "CosmosDbConnection")] on the method or return type to output the enriched order.
    // - Signature: Returns a Task<EnrichedOrder> representing the async result.
    // - Logic: Deserialize the Service Bus message string into an Order object, call enricher.EnrichAsync, log the details, and return the enriched order.
    [Function(nameof(EnrichOrderFunction))]
    [CosmosDBOutput("LearningDb", "orders", Connection = "CosmosDbConnection")]
    public Task<EnrichedOrder> RunAsync(
        [ServiceBusTrigger("enrich-in", Connection = "ServiceBusConnection")] string message,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException("TODO: Implement the CosmosDB/ServiceBus pipeline enrichment function according to the exercise guidelines.");
    }
}
