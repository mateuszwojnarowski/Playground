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

    public OrderAuditProjection Project(Order order)
    {
        if (order.CustomerId <= 0)
        {
            throw new ArgumentException("CustomerId must be greater than zero.", nameof(order));
        }

        if (string.IsNullOrWhiteSpace(order.Id))
        {
            throw new ArgumentException("Order Id is required.", nameof(order));
        }

        return new OrderAuditProjection
        {
            Id = $"{order.Id}:audit",
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            Product = order.Product.Trim(),
            Quantity = order.Quantity,
            Total = order.Total,
            LastSeenUtc = _clock()
        };
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
