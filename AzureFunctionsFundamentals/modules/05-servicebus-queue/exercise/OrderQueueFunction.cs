using System.Text.Json;
using AzureFunctionsFundamentals.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureFunctionsFundamentals.Modules.ServiceBusQueue;

public sealed class OrderQueueFunction
{
    private readonly OrderConsumer _consumer;
    private readonly ILogger<OrderQueueFunction> _logger;

    public OrderQueueFunction(OrderConsumer consumer, ILogger<OrderQueueFunction> logger)
    {
        _consumer = consumer;
        _logger = logger;
    }

    [Function(nameof(OrderQueueFunction))]
    public async Task RunAsync(
        [ServiceBusTrigger("orders", Connection = "ServiceBusConnection")] string message,
        CancellationToken cancellationToken)
    {
        Order order = JsonSerializer.Deserialize<Order>(message)
            ?? throw new InvalidOperationException("The Service Bus message did not contain a valid order.");

        ProcessedOrder processed = await _consumer.ProcessAsync(order, cancellationToken);
        _logger.LogInformation("Processed order {OrderId} for customer {CustomerId}; total {Total:C}.", processed.OrderId, processed.CustomerId, processed.Total);
    }
}
