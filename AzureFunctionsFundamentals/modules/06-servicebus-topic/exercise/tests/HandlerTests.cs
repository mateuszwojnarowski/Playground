using AzureFunctionsFundamentals.Modules.ServiceBusTopic;
using AzureFunctionsFundamentals.Shared;
using Xunit;

namespace ServiceBusTopicExercise.Tests;

public sealed class HandlerTests
{
    [Fact]
    public void AuditHandler_Record_CreatesAuditEntryForOrderEvent()
    {
        var handler = new AuditHandler();
        var observedAt = new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero);
        var order = new Order
        {
            Id = "order-789",
            CustomerId = 99,
            Product = "Monitor",
            Quantity = 1,
            UnitPrice = 225m
        };

        AuditEntry entry = handler.Record(order, observedAt);

        Assert.Equal("order-789", entry.OrderId);
        Assert.Equal(99, entry.CustomerId);
        Assert.Equal(observedAt, entry.ObservedAt);
        Assert.Contains("audit subscription", entry.Description);
    }

    [Fact]
    public void FulfilmentHandler_Decide_ApprovesStandardShippingForNormalOrder()
    {
        var handler = new FulfilmentHandler();
        var order = new Order
        {
            Id = "order-789",
            CustomerId = 99,
            Product = "Monitor",
            Quantity = 1,
            UnitPrice = 225m
        };

        FulfilmentDecision decision = handler.Decide(order);

        Assert.Equal("order-789", decision.OrderId);
        Assert.True(decision.ShouldShip);
        Assert.Equal("Standard shipping approved.", decision.Reason);
    }

    [Fact]
    public void FulfilmentHandler_Decide_UsesDifferentBehaviourForSameEvent()
    {
        var handler = new FulfilmentHandler();
        var order = new Order
        {
            Id = "order-999",
            CustomerId = 42,
            Product = "Server",
            Quantity = 2,
            UnitPrice = 300m
        };

        FulfilmentDecision decision = handler.Decide(order);

        Assert.True(decision.ShouldShip);
        Assert.Equal("Priority shipping required for high-value order.", decision.Reason);
    }

    [Fact]
    public void FulfilmentHandler_Decide_DoesNotShipInvalidOrderDetails()
    {
        var handler = new FulfilmentHandler();
        var order = new Order
        {
            Id = "order-1000",
            CustomerId = 42,
            Product = "",
            Quantity = 0,
            UnitPrice = 100m
        };

        FulfilmentDecision decision = handler.Decide(order);

        Assert.False(decision.ShouldShip);
        Assert.Equal("Order is missing product or quantity details.", decision.Reason);
    }
}
