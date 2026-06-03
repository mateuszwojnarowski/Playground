using AzureFunctionsFundamentals.Shared;

namespace AzureFunctionsFundamentals.Modules.ServiceBusTopic;

public sealed class AuditHandler
{
    public AuditEntry Record(Order order, DateTimeOffset observedAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(order.Id);

        return new AuditEntry(
            order.Id,
            order.CustomerId,
            $"Order {order.Id} for {order.Product} was observed by the audit subscription.",
            observedAt);
    }
}

public sealed record AuditEntry(string OrderId, int CustomerId, string Description, DateTimeOffset ObservedAt);
