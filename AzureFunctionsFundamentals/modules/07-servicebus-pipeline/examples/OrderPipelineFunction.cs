using System.Text.Json;
using AzureFunctionsFundamentals.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ServiceBusPipelineExample;

public sealed class OrderPipelineFunction(ILogger<OrderPipelineFunction> logger)
{
    [Function(nameof(OrderPipelineFunction))]
    [ServiceBusOutput("orders-out", Connection = "ServiceBusConnection")]
    public string Run([ServiceBusTrigger("orders", Connection = "ServiceBusConnection")] string message)
    {
        Order order = JsonSerializer.Deserialize<Order>(message)
            ?? throw new InvalidOperationException("The Service Bus message did not contain a valid order.");

        logger.LogInformation("Transforming order {OrderId} for customer {CustomerId}.", order.Id, order.CustomerId);

        var output = new
        {
            order.Id,
            order.CustomerId,
            Product = order.Product.Trim(),
            order.Quantity,
            order.Total,
            ProcessedAt = DateTimeOffset.UtcNow
        };

        logger.LogInformation("Forwarding order {OrderId} to orders-out.", order.Id);
        return JsonSerializer.Serialize(output);
    }
}
