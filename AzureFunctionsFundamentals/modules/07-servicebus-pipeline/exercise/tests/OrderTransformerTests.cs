using AzureFunctionsFundamentals.Modules.ServiceBusPipeline;
using AzureFunctionsFundamentals.Shared;
using Xunit;

namespace ServiceBusPipelineExercise.Tests;

public sealed class OrderTransformerTests
{
    private static readonly DateTimeOffset FixedNow = new(2026, 1, 2, 3, 4, 5, TimeSpan.Zero);
    private readonly OrderTransformer _transformer = new(new FakeClock(FixedNow));

    [Fact]
    public void Transform_NormalizesProductAndAddsProcessingMetadata()
    {
        var order = new Order
        {
            Id = "order-701",
            CustomerId = 12,
            Product = "  laptop   stand  ",
            Quantity = 2,
            UnitPrice = 75m
        };

        TransformedOrder transformed = _transformer.Transform(order);

        Assert.Equal("order-701", transformed.Id);
        Assert.Equal("laptop stand", transformed.Product);
        Assert.Equal(150m, transformed.Total);
        Assert.Equal("standard", transformed.Route);
        Assert.Equal(FixedNow, transformed.ProcessingStamp);
        Assert.Equal("order:order-701", transformed.IdempotencyKey);
    }

    [Fact]
    public void Transform_RoutesHighValueOrdersToPriority()
    {
        var order = new Order
        {
            Id = "order-702",
            CustomerId = 8,
            Product = "Server",
            Quantity = 1,
            UnitPrice = 1_250m
        };

        TransformedOrder transformed = _transformer.Transform(order);

        Assert.Equal("priority", transformed.Route);
    }

    [Fact]
    public void Transform_RoutesBulkOrdersToPriority()
    {
        var order = new Order
        {
            Id = "order-703",
            CustomerId = 8,
            Product = "Cable",
            Quantity = 10,
            UnitPrice = 5m
        };

        TransformedOrder transformed = _transformer.Transform(order);

        Assert.Equal("priority", transformed.Route);
    }

    private sealed class FakeClock : IProcessingClock
    {
        public FakeClock(DateTimeOffset utcNow)
        {
            UtcNow = utcNow;
        }

        public DateTimeOffset UtcNow { get; }
    }
}
