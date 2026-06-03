namespace AzureFunctionsFundamentals.Shared;

/// <summary>
/// An order message that flows through the Service Bus and Cosmos DB modules.
/// </summary>
public sealed record Order
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public int CustomerId { get; init; }
    public string Product { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal Total => Quantity * UnitPrice;
}

/// <summary>
/// An order enriched with customer details read from SQL or Cosmos DB.
/// </summary>
public sealed record EnrichedOrder
{
    public string Id { get; init; } = string.Empty;
    public int CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string CustomerTier { get; init; } = string.Empty;
    public string Product { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal Total { get; init; }
}

/// <summary>
/// A customer record (SQL: dbo.Customers).
/// </summary>
public sealed record Customer
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Tier { get; init; } = string.Empty;
}
