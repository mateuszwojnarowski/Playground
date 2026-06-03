using System.Text.Json;
using AzureFunctionsFundamentals.Shared;

namespace StorageQueueExercise;

public sealed class JobProcessor
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public ProcessedJob Process(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Queue message must contain an order JSON document.", nameof(message));
        }

        var order = JsonSerializer.Deserialize<Order>(message, JsonOptions)
            ?? throw new InvalidOperationException("Queue message did not contain a valid order.");

        if (string.IsNullOrWhiteSpace(order.Id))
        {
            throw new InvalidOperationException("Order id is required.");
        }

        if (order.Quantity <= 0)
        {
            throw new InvalidOperationException("Order quantity must be greater than zero.");
        }

        if (order.UnitPrice < 0)
        {
            throw new InvalidOperationException("Order unit price cannot be negative.");
        }

        return new ProcessedJob(
            order.Id,
            order.CustomerId,
            order.Product,
            order.Quantity,
            order.UnitPrice,
            order.Total,
            order.Total >= 100m ? "priority" : "standard",
            DateTimeOffset.UtcNow);
    }

    public string ProcessToJson(string message)
    {
        var result = Process(message);
        return JsonSerializer.Serialize(result, JsonOptions);
    }
}

public sealed record ProcessedJob(
    string OrderId,
    int CustomerId,
    string Product,
    int Quantity,
    decimal UnitPrice,
    decimal Total,
    string FulfillmentLane,
    DateTimeOffset ProcessedAtUtc);
