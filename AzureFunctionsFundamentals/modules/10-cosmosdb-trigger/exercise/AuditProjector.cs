using System.Text.Json.Serialization;
using AzureFunctionsFundamentals.Shared;

namespace AzureFunctionsFundamentals.Modules.CosmosDbTrigger.Exercise;

public sealed class AuditProjector
{
    private readonly Func<DateTimeOffset> _clock;

    public AuditProjector() : this(() => DateTimeOffset.UtcNow)
    {
    }

    public AuditProjector(Func<DateTimeOffset> clock)
    {
        _clock = clock;
    }

    // TODO: Implement Cosmos audit projection.
    // - Validate the incoming order according to README.md.
    // - Project the order into the audit document shape and timestamp it with the injected clock.
    // - Return the OrderAuditProjection expected by the tests.
    public OrderAuditProjection Project(Order order)
    {
        throw new NotImplementedException("TODO: implement this method.");
    }
}

public sealed record OrderAuditProjection
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;
    public string OrderId { get; init; } = string.Empty;
    public int CustomerId { get; init; }
    public string Product { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal Total { get; init; }
    public DateTimeOffset LastSeenUtc { get; init; }
}
