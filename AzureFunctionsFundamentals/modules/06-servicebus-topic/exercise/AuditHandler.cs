using AzureFunctionsFundamentals.Shared;

namespace AzureFunctionsFundamentals.Modules.ServiceBusTopic;

public sealed class AuditHandler
{
    // TODO: Implement audit entry creation.
    // - Validate the order data required for the audit stream.
    // - Build the audit description and projection described in README.md.
    // - Return the completed AuditEntry for the observed message.
    public AuditEntry Record(Order order, DateTimeOffset observedAt)
    {
        throw new NotImplementedException("TODO: implement this method.");
    }
}

public sealed record AuditEntry(string OrderId, int CustomerId, string Description, DateTimeOffset ObservedAt);
