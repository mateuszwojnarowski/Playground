using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ServiceBusQueueExample;

public sealed class OrderQueueLoggerFunction
{
    private readonly ILogger<OrderQueueLoggerFunction> _logger;

    public OrderQueueLoggerFunction(ILogger<OrderQueueLoggerFunction> logger)
    {
        _logger = logger;
    }

    [Function(nameof(OrderQueueLoggerFunction))]
    public void Run([ServiceBusTrigger("orders", Connection = "ServiceBusConnection")] string message)
    {
        _logger.LogInformation("Received order queue message: {Message}", message);
    }
}
