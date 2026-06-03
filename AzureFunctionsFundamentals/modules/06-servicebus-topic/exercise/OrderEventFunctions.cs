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

    [Function(nameof(AuditSubscriber))]
    public void AuditSubscriber([ServiceBusTrigger("order-events", "audit", Connection = "ServiceBusConnection")] string message)
    {
        Order order = DeserializeOrder(message);
        AuditEntry entry = _auditHandler.Record(order, DateTimeOffset.UtcNow);

        _logger.LogInformation("Audit entry for order {OrderId}: {Description}", entry.OrderId, entry.Description);
    }

    [Function(nameof(FulfilmentSubscriber))]
    public void FulfilmentSubscriber([ServiceBusTrigger("order-events", "fulfilment", Connection = "ServiceBusConnection")] string message)
    {
        Order order = DeserializeOrder(message);
        FulfilmentDecision decision = _fulfilmentHandler.Decide(order);

        _logger.LogInformation("Fulfilment decision for order {OrderId}: ship={ShouldShip}; {Reason}", decision.OrderId, decision.ShouldShip, decision.Reason);
    }

    private static Order DeserializeOrder(string message)
    {
        return JsonSerializer.Deserialize<Order>(message)
            ?? throw new InvalidOperationException("The Service Bus message did not contain a valid order.");
    }
}
