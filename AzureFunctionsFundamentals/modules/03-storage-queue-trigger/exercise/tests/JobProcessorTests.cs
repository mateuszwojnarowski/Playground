using System.Text.Json;
using StorageQueueExercise;
using Xunit;

namespace StorageQueueExercise.Tests;

public sealed class JobProcessorTests
{
    [Fact]
    public void Process_ComputesTotalAndPriorityLane()
    {
        var processor = new JobProcessor();
        var message = """
            {
              "id": "order-1001",
              "customerId": 42,
              "product": "Keyboard",
              "quantity": 3,
              "unitPrice": 50.00
            }
            """;

        var result = processor.Process(message);

        Assert.Equal("order-1001", result.OrderId);
        Assert.Equal(150.00m, result.Total);
        Assert.Equal("priority", result.FulfillmentLane);
    }

    [Fact]
    public void ProcessToJson_ReturnsResultDocument()
    {
        var processor = new JobProcessor();
        var json = processor.ProcessToJson("""
            { "id": "order-1002", "customerId": 7, "product": "Mouse", "quantity": 1, "unitPrice": 25.50 }
            """);

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal("order-1002", root.GetProperty("orderId").GetString());
        Assert.Equal(25.50m, root.GetProperty("total").GetDecimal());
        Assert.Equal("standard", root.GetProperty("fulfillmentLane").GetString());
    }

    [Fact]
    public void Process_RejectsInvalidQuantity()
    {
        var processor = new JobProcessor();

        var exception = Assert.Throws<InvalidOperationException>(() => processor.Process("""
            { "id": "bad-order", "customerId": 1, "product": "Mouse", "quantity": 0, "unitPrice": 25.50 }
            """));

        Assert.Contains("quantity", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}
