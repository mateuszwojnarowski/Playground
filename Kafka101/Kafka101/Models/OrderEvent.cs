// =============================================================================
// OrderEvent.cs - Sample domain model representing an e-commerce order event
// =============================================================================
//
// PURPOSE:
//   This model represents a realistic business event that might be published to
//   Kafka in a real e-commerce system. Using domain-relevant models (rather than
//   plain strings) demonstrates how Kafka is used in production applications.
//
// REAL-WORLD CONTEXT:
//   In microservices architectures, services communicate asynchronously via
//   events. When a user places an order, the Order Service publishes an
//   OrderCreated event to Kafka. Other services (Inventory, Shipping, Email)
//   consume this event and react accordingly - all without direct coupling.
//
// KAFKA RELEVANCE:
//   - This object will be SERIALIZED to JSON (or Avro/Protobuf in production)
//     before being sent to Kafka as a byte array.
//   - The 'OrderId' field makes a good KAFKA MESSAGE KEY, ensuring all events
//     for the same order go to the same partition (preserving order).
//   - The 'EventType' field enables consumers to filter events by type.

namespace Kafka101.Models;

/// <summary>
/// Represents an order lifecycle event in an e-commerce system.
/// This model demonstrates a realistic payload for Kafka messages.
/// </summary>
public class OrderEvent
{
    // -------------------------------------------------------------------------
    // KAFKA KEY CANDIDATE:
    // Using OrderId as the Kafka message key ensures that all events for the
    // same order are sent to the same partition. This guarantees ordering
    // of events for a single order (e.g., Created -> Paid -> Shipped).
    // -------------------------------------------------------------------------

    /// <summary>
    /// Unique identifier for the order. Used as the Kafka message key
    /// to ensure per-order event ordering within a partition.
    /// </summary>
    public string OrderId { get; set; } = string.Empty;

    /// <summary>
    /// ID of the customer who placed the order.
    /// Could be used as partition key if you want per-customer ordering.
    /// </summary>
    public string CustomerId { get; set; } = string.Empty;

    /// <summary>
    /// The type of order event (e.g., "OrderCreated", "OrderPaid", "OrderShipped").
    /// Consumers can use this to filter events relevant to their domain.
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// List of items in the order. Demonstrates nested object serialization.
    /// </summary>
    public List<OrderItem> Items { get; set; } = new();

    /// <summary>
    /// Total monetary value of the order.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Currency code (e.g., "USD", "EUR"). Important for global systems.
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// When the event occurred. Always use UTC for distributed systems
    /// to avoid timezone confusion across services in different regions.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Current status of the order in its lifecycle.
    /// </summary>
    public OrderStatus Status { get; set; }

    /// <summary>
    /// Shipping address for the order.
    /// </summary>
    public Address ShippingAddress { get; set; } = new();

    // -------------------------------------------------------------------------
    // SCHEMA EVOLUTION NOTE:
    // When evolving this schema in production, be careful about:
    // 1. Removing fields - consumers may still expect them
    // 2. Changing field types - can break deserialization
    // 3. Renaming fields - equivalent to remove + add
    //
    // BEST PRACTICE: Only add new optional fields. Use Avro/Protobuf (not JSON)
    // in production for proper schema evolution support.
    // -------------------------------------------------------------------------

    /// <summary>Returns a human-readable summary of this order event.</summary>
    public override string ToString() =>
        $"[{EventType}] Order {OrderId} | Customer: {CustomerId} | " +
        $"Total: {TotalAmount:C} {Currency} | Status: {Status} | " +
        $"Items: {Items.Count} | Time: {Timestamp:yyyy-MM-dd HH:mm:ss} UTC";
}

/// <summary>
/// Represents a single item within an order.
/// </summary>
public class OrderItem
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    /// <summary>Computed subtotal for this line item.</summary>
    public decimal Subtotal => Quantity * UnitPrice;
}

/// <summary>
/// Shipping address details.
/// </summary>
public class Address
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
}

/// <summary>
/// Possible states in the order lifecycle.
/// Using an enum prevents typos and makes the code self-documenting.
/// </summary>
public enum OrderStatus
{
    Pending,
    Confirmed,
    Paid,
    Processing,
    Shipped,
    Delivered,
    Cancelled,
    Refunded
}

/// <summary>
/// Factory class to generate sample order events for demonstration purposes.
/// In a real application, this data would come from your business logic.
/// </summary>
public static class OrderEventFactory
{
    private static readonly Random _random = new();

    private static readonly string[] _customerIds =
        ["CUST-001", "CUST-002", "CUST-003", "CUST-004", "CUST-005"];

    private static readonly (string Id, string Name, decimal Price)[] _products =
    [
        ("PROD-A1", "Wireless Keyboard", 79.99m),
        ("PROD-B2", "USB-C Hub", 49.99m),
        ("PROD-C3", "Monitor Stand", 39.99m),
        ("PROD-D4", "Webcam HD", 89.99m),
        ("PROD-E5", "Desk Lamp LED", 29.99m),
        ("PROD-F6", "Mouse Pad XL", 19.99m),
    ];

    private static readonly string[] _eventTypes =
        ["OrderCreated", "OrderPaid", "OrderShipped", "OrderDelivered"];

    /// <summary>
    /// Creates a sample order event with realistic data.
    /// </summary>
    /// <param name="orderId">Optional specific order ID. If null, generates one.</param>
    /// <param name="eventType">Optional event type. If null, uses "OrderCreated".</param>
    public static OrderEvent Create(string? orderId = null, string? eventType = null)
    {
        var itemCount = _random.Next(1, 4);
        var items = Enumerable.Range(0, itemCount)
            .Select(_ =>
            {
                var product = _products[_random.Next(_products.Length)];
                return new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    Quantity = _random.Next(1, 3)
                };
            })
            .ToList();

        return new OrderEvent
        {
            OrderId = orderId ?? $"ORD-{_random.Next(10000, 99999)}",
            CustomerId = _customerIds[_random.Next(_customerIds.Length)],
            EventType = eventType ?? "OrderCreated",
            Items = items,
            TotalAmount = items.Sum(i => i.Subtotal),
            Currency = "USD",
            Timestamp = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            ShippingAddress = new Address
            {
                Street = "123 Main St",
                City = "Seattle",
                Country = "USA",
                PostalCode = "98101"
            }
        };
    }

    /// <summary>
    /// Creates multiple sample order events.
    /// Useful for batch testing and load simulation.
    /// </summary>
    public static IEnumerable<OrderEvent> CreateMany(int count) =>
        Enumerable.Range(0, count).Select(_ => Create());
}
