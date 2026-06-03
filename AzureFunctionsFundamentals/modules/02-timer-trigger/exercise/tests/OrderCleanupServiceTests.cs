using AzureFunctionsFundamentals.Modules.TimerTrigger.Exercise;
using AzureFunctionsFundamentals.Shared;
using Xunit;

namespace AzureFunctionsFundamentals.Modules.TimerTrigger.Exercise.Tests;

public sealed class OrderCleanupServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 3, 15, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void SummarizeStaleOrders_ReturnsOnlyOrdersAtOrBeforeCutoff()
    {
        var service = new OrderCleanupService(new FakeTimeProvider(Now));
        var orders = new[]
        {
            Snapshot("recent", 10, 1, 12m, Now.AddDays(-3)),
            Snapshot("stale-oldest", 20, 2, 15m, Now.AddDays(-45)),
            Snapshot("stale-at-cutoff", 30, 1, 30m, Now.AddDays(-30))
        };

        var summary = service.SummarizeStaleOrders(orders, TimeSpan.FromDays(30));

        Assert.Equal(Now.AddDays(-30), summary.Cutoff);
        Assert.Equal(2, summary.StaleOrderCount);
        Assert.Equal(60m, summary.StaleOrderTotal);
        Assert.Equal(["stale-oldest", "stale-at-cutoff"], summary.StaleOrderIds);
    }

    [Fact]
    public void SummarizeStaleOrders_WhenNoneAreStale_ReturnsEmptySummary()
    {
        var service = new OrderCleanupService(new FakeTimeProvider(Now));
        var orders = new[]
        {
            Snapshot("recent-1", 10, 1, 12m, Now.AddDays(-1)),
            Snapshot("recent-2", 20, 3, 8m, Now.AddDays(-7))
        };

        var summary = service.SummarizeStaleOrders(orders, TimeSpan.FromDays(30));

        Assert.Equal(Now.AddDays(-30), summary.Cutoff);
        Assert.Equal(0, summary.StaleOrderCount);
        Assert.Equal(0m, summary.StaleOrderTotal);
        Assert.Empty(summary.StaleOrderIds);
    }

    private static OrderSnapshot Snapshot(string id, int customerId, int quantity, decimal unitPrice, DateTimeOffset lastUpdatedAt)
    {
        return new OrderSnapshot(
            new Order
            {
                Id = id,
                CustomerId = customerId,
                Product = "Widget",
                Quantity = quantity,
                UnitPrice = unitPrice
            },
            lastUpdatedAt);
    }

    private sealed class FakeTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
