using AzureFunctionsFundamentals.Modules.HttpTrigger.Exercise;
using AzureFunctionsFundamentals.Shared;
using Xunit;

namespace AzureFunctionsFundamentals.Modules.HttpTrigger.Exercise.Tests;

public sealed class OrderServiceTests
{
    private readonly OrderService service = new(new OrderValidator());

    [Fact]
    public void Create_WithValidOrder_ReturnsCreatedOrder()
    {
        var order = new Order
        {
            Id = "order-123",
            CustomerId = 42,
            Product = "  Keyboard  ",
            Quantity = 2,
            UnitPrice = 49.99m
        };

        var result = service.Create(order);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.NotNull(result.Order);
        Assert.Equal("order-123", result.Order.Id);
        Assert.Equal("Keyboard", result.Order.Product);
        Assert.Equal(99.98m, result.Order.Total);
    }

    [Fact]
    public void Create_WithMissingPayload_ReturnsPayloadError()
    {
        var result = service.Create(null);

        Assert.False(result.IsValid);
        Assert.Null(result.Order);
        Assert.Equal(["Order payload is required."], result.Errors);
    }

    [Fact]
    public void Create_WithInvalidOrder_ReturnsAllValidationErrors()
    {
        var order = new Order
        {
            CustomerId = 0,
            Product = " ",
            Quantity = -1,
            UnitPrice = 0m
        };

        var result = service.Create(order);

        Assert.False(result.IsValid);
        Assert.Null(result.Order);
        Assert.Equal(
            [
                "CustomerId must be greater than zero.",
                "Product is required.",
                "Quantity must be greater than zero.",
                "UnitPrice must be greater than zero."
            ],
            result.Errors);
    }
}
