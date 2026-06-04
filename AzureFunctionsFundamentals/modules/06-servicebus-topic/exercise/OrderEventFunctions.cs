using System.Text.Json;
using AzureFunctionsFundamentals.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureFunctionsFundamentals.Modules.ServiceBusTopic;

public sealed class OrderEventFunctions
{
    private readonly AuditHandler _auditHandler;
    private readonly FulfilmentHandler _fulfilmentHandler;
    private readonly ILogger<OrderEventFunctions> _logger;

    public OrderEventFunctions(
        AuditHandler auditHandler,
        FulfilmentHandler fulfilmentHandler,
        ILogger<OrderEventFunctions> logger)
    {
        _auditHandler = auditHandler;
        _fulfilmentHandler = fulfilmentHandler;
        _logger = logger;
    }

    // TODO: Implement the AuditSubscriber function.
    // Hints:
    // - Trigger: Use [ServiceBusTrigger("order-events", "audit", Connection = "ServiceBusConnection")] to receive the audit topic message.
    // - Logic: Deserialize the string to an Order object, invoke _auditHandler.Record, and log the audit entry details.
    [Function(nameof(AuditSubscriber))]
    public void AuditSubscriber([ServiceBusTrigger("order-events", "audit", Connection = "ServiceBusConnection")] string message)
    {
        throw new NotImplementedException("TODO: Implement the AuditSubscriber function according to the exercise guidelines.");
    }

    // TODO: Implement the FulfilmentSubscriber function.
    // Hints:
    // - Trigger: Use [ServiceBusTrigger("order-events", "fulfilment", Connection = "ServiceBusConnection")] to receive the fulfilment topic message.
    // - Logic: Deserialize the string to an Order object, invoke _fulfilmentHandler.Decide, and log the decision details.
    [Function(nameof(FulfilmentSubscriber))]
    public void FulfilmentSubscriber([ServiceBusTrigger("order-events", "fulfilment", Connection = "ServiceBusConnection")] string message)
    {
        throw new NotImplementedException("TODO: Implement the FulfilmentSubscriber function according to the exercise guidelines.");
    }

    private static Order DeserializeOrder(string message)
    {
        return JsonSerializer.Deserialize<Order>(message)
            ?? throw new InvalidOperationException("The Service Bus message did not contain a valid order.");
    }
}
