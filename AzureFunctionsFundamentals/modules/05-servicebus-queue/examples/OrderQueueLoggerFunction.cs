using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ServiceBusQueueExample;

public sealed class OrderQueueLoggerFunction(ILogger<OrderQueueLoggerFunction> logger)
{
    [Function(nameof(OrderQueueLoggerFunction))]
    public void Run([ServiceBusTrigger("orders", Connection = "ServiceBusConnection")] string message)
    {
        logger.LogInformation("Service Bus queue message received with {CharacterCount} characters.", message.Length);
    }
}
