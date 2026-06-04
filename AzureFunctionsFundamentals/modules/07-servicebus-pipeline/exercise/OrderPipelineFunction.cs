using System.Text.Json;
using AzureFunctionsFundamentals.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureFunctionsFundamentals.Modules.ServiceBusPipeline;

public sealed class OrderPipelineFunction
{
    private readonly OrderTransformer _transformer;
    private readonly ILogger<OrderPipelineFunction> _logger;

    public OrderPipelineFunction(OrderTransformer transformer, ILogger<OrderPipelineFunction> logger)
    {
        _transformer = transformer;
        _logger = logger;
    }

    // TODO: Implement the OrderPipelineFunction.
    // Hints:
    // - Trigger: Use [ServiceBusTrigger("enrich-in", Connection = "ServiceBusConnection")] to receive input.
    // - Output: Use [ServiceBusOutput("orders-out", Connection = "ServiceBusConnection")] on the return or method.
    // - Signature: Returns a string representing the serialized transformed order.
    // - Logic: Deserialize the string to an Order object, invoke _transformer.Transform, serialize the transformed order, and log the route and idempotency key.
    [Function(nameof(OrderPipelineFunction))]
    [ServiceBusOutput("orders-out", Connection = "ServiceBusConnection")]
    public string Run([ServiceBusTrigger("enrich-in", Connection = "ServiceBusConnection")] string message)
    {
        throw new NotImplementedException("TODO: Implement the Service Bus Pipeline function according to the exercise guidelines.");
    }
}
