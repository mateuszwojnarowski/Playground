using System.Text.Json;
using AzureFunctionsFundamentals.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureFunctionsFundamentals.Modules.SqlRead;

public sealed class OrderEnrichmentFunction
{
    private readonly OrderEnricher _enricher;
    private readonly ILogger<OrderEnrichmentFunction> _logger;

    public OrderEnrichmentFunction(OrderEnricher enricher, ILogger<OrderEnrichmentFunction> logger)
    {
        _enricher = enricher;
        _logger = logger;
    }

    // TODO: Implement the OrderEnrichmentFunction.
    // Hints:
    // - Trigger: Use [ServiceBusTrigger("enrich-in", Connection = "ServiceBusConnection")] to receive input message as a string.
    // - Output: Use [ServiceBusOutput("orders-out", Connection = "ServiceBusConnection")] to output the enriched order string.
    // - Signature: Returns a Task<string> representing the serialized enriched order.
    // - Logic: Deserialize the string to an Order object, call _enricher.EnrichAsync, serialize the result, and log the enrichment details.
    [Function(nameof(OrderEnrichmentFunction))]
    [ServiceBusOutput("orders-out", Connection = "ServiceBusConnection")]
    public Task<string> RunAsync(
        [ServiceBusTrigger("enrich-in", Connection = "ServiceBusConnection")] string message,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException("TODO: Implement the SQL read-based enrichment function according to the exercise guidelines.");
    }
}
