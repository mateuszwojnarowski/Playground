using AzureFunctionsFundamentals.Modules.CosmosDbTrigger.Exercise;
using AzureFunctionsFundamentals.Shared;
using Xunit;

namespace CosmosDbTriggerExercise.Tests;

public sealed class AuditProjectorTests
{
    [Fact]
    public void Project_creates_audit_projection_from_order()
    {
        var now = new DateTimeOffset(2025, 1, 2, 3, 4, 5, TimeSpan.Zero);
        var projector = new AuditProjector(() => now);

        var projection = projector.Project(new Order
        {
            Id = "order-1",
            CustomerId = 42,
            Product = "  Keyboard  ",
            Quantity = 2,
            UnitPrice = 19.50m
        });

        Assert.Equal("order-1:audit", projection.Id);
        Assert.Equal("order-1", projection.OrderId);
        Assert.Equal(42, projection.CustomerId);
        Assert.Equal("Keyboard", projection.Product);
        Assert.Equal(2, projection.Quantity);
        Assert.Equal(39.00m, projection.Total);
        Assert.Equal(now, projection.LastSeenUtc);
    }

    [Fact]
    public void Project_rejects_orders_without_partition_key()
    {
        var projector = new AuditProjector();

        var ex = Assert.Throws<ArgumentException>(() => projector.Project(new Order { Id = "order-1", CustomerId = 0 }));

        Assert.Contains("CustomerId", ex.Message);
    }
}
