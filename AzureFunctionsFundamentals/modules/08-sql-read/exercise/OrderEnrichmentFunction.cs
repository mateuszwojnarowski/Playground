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

    [Function(nameof(OrderEnrichmentFunction))]
    [ServiceBusOutput("orders-out", Connection = "ServiceBusConnection")]
    public async Task<string> RunAsync(
        [ServiceBusTrigger("enrich-in", Connection = "ServiceBusConnection")] string message,
        CancellationToken cancellationToken)
    {
        Order order = JsonSerializer.Deserialize<Order>(message)
            ?? throw new InvalidOperationException("The Service Bus message did not contain a valid order.");

        EnrichedOrder enriched = await _enricher.EnrichAsync(order, cancellationToken);
        _logger.LogInformation("Enriched order {OrderId} with customer {CustomerId}.", enriched.Id, enriched.CustomerId);
        return JsonSerializer.Serialize(enriched);
    }
}
