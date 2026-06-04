using AzureFunctionsFundamentals.Shared;

namespace AzureFunctionsFundamentals.Modules.ServiceBusPipeline;

public sealed class OrderTransformer
{
    private readonly IProcessingClock _clock;

    public OrderTransformer(IProcessingClock clock)
    {
        _clock = clock;
    }

    // TODO: Implement order transformation.
    // - Validate and normalize the incoming order according to README.md.
    // - Calculate routing, timestamps, and idempotency values for the transformed message.
    // - Return the fully populated TransformedOrder.
    public TransformedOrder Transform(Order order)
    {
        throw new NotImplementedException("TODO: implement this method.");
    }

    // TODO: Implement product normalization.
    // - Normalize the product name exactly as the exercise requires.
    // - Keep this helper aligned with the transformation rules in README.md.
    private static string NormalizeProduct(string product)
    {
        throw new NotImplementedException("TODO: implement this method.");
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
