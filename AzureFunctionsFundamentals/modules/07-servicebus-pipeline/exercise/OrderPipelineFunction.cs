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

    [Function(nameof(OrderPipelineFunction))]
    [ServiceBusOutput("orders-out", Connection = "ServiceBusConnection")]
    public string Run([ServiceBusTrigger("enrich-in", Connection = "ServiceBusConnection")] string message)
    {
        Order order = JsonSerializer.Deserialize<Order>(message)
            ?? throw new InvalidOperationException("The Service Bus message did not contain a valid order.");

        TransformedOrder transformed = _transformer.Transform(order);
        _logger.LogInformation("Routed order {OrderId} to {Route}; idempotency key {IdempotencyKey}.", transformed.Id, transformed.Route, transformed.IdempotencyKey);
        return JsonSerializer.Serialize(transformed);
    }
}
