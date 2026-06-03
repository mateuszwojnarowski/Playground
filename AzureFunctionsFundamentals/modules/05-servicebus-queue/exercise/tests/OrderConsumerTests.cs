using AzureFunctionsFundamentals.Modules.ServiceBusQueue;
using AzureFunctionsFundamentals.Shared;
using Xunit;

namespace ServiceBusQueueExercise.Tests;

public sealed class OrderConsumerTests
{
    private readonly OrderConsumer _consumer = new(new OrderValidator());

    [Fact]
    public async Task ProcessAsync_ValidOrder_ReturnsProcessedOrder()
    {
        var order = new Order
        {
            Id = "order-123",
            CustomerId = 7,
            Product = "Headphones",
            Quantity = 2,
            UnitPrice = 25m
        };

        ProcessedOrder processed = await _consumer.ProcessAsync(order);

        Assert.Equal("order-123", processed.OrderId);
        Assert.Equal(7, processed.CustomerId);
        Assert.Equal(50m, processed.Total);
    }

    [Fact]
    public async Task ProcessAsync_InvalidBusinessRules_ThrowsForDeadLetterRetryFlow()
    {
        var order = new Order
        {
            Id = "order-456",
            CustomerId = 0,
            Product = "",
            Quantity = 0,
            UnitPrice = -1m
        };

        OrderValidationException exception = await Assert.ThrowsAsync<OrderValidationException>(() => _consumer.ProcessAsync(order));

        Assert.Contains("Customer id must be greater than zero.", exception.Errors);
        Assert.Contains("Product is required.", exception.Errors);
        Assert.Contains("Quantity must be greater than zero.", exception.Errors);
        Assert.Contains("Unit price must be greater than zero.", exception.Errors);
    }
}
