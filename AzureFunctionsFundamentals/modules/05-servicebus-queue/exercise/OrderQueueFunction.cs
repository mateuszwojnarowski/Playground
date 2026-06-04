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

    // TODO: Implement the OrderQueueFunction.
    // Hints:
    // - Trigger: Use [ServiceBusTrigger("orders", Connection = "ServiceBusConnection")] to receive the message as a string.
    // - Parameter: Needs a string message and a CancellationToken.
    // - Logic: Deserialize the string to an Order object, call _consumer.ProcessAsync, and log the processed order details.
    [Function(nameof(OrderQueueFunction))]
    public async Task RunAsync(
        [ServiceBusTrigger("orders", Connection = "ServiceBusConnection")] string message,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException("TODO: Implement the Service Bus Queue-triggered function according to the exercise guidelines.");
    }
}
