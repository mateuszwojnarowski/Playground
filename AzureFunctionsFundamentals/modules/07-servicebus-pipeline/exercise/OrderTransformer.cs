using AzureFunctionsFundamentals.Shared;

namespace AzureFunctionsFundamentals.Modules.ServiceBusPipeline;

public sealed class OrderTransformer
{
    private readonly IProcessingClock _clock;

    public OrderTransformer(IProcessingClock clock)
    {
        _clock = clock;
    }

    public TransformedOrder Transform(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        string normalizedProduct = NormalizeProduct(order.Product);
        string route = order.Total >= 1_000m || order.Quantity >= 10 ? "priority" : "standard";

        return new TransformedOrder
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            Product = normalizedProduct,
            Quantity = order.Quantity,
            UnitPrice = order.UnitPrice,
            Total = order.Total,
            Route = route,
            ProcessingStamp = _clock.UtcNow,
            IdempotencyKey = $"order:{order.Id}"
        };
    }

    private static string NormalizeProduct(string product)
    {
        return string.Join(' ', product.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}

public interface IProcessingClock
{
    DateTimeOffset UtcNow { get; }
}

public sealed class SystemProcessingClock : IProcessingClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

public sealed record TransformedOrder
{
    public string Id { get; init; } = string.Empty;
    public int CustomerId { get; init; }
    public string Product { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal Total { get; init; }
    public string Route { get; init; } = string.Empty;
    public DateTimeOffset ProcessingStamp { get; init; }
    public string IdempotencyKey { get; init; } = string.Empty;
}
