using AzureFunctionsFundamentals.Shared;

namespace AzureFunctionsFundamentals.Modules.TimerTrigger.Exercise;

public sealed record OrderSnapshot(Order Order, DateTimeOffset LastUpdatedAt);

public sealed record CleanupSummary(DateTimeOffset Cutoff, int StaleOrderCount, decimal StaleOrderTotal, IReadOnlyList<string> StaleOrderIds);

public sealed class OrderCleanupService(TimeProvider timeProvider)
{
    public CleanupSummary SummarizeStaleOrders(IEnumerable<OrderSnapshot> orders, TimeSpan staleAfter)
    {
        var cutoff = timeProvider.GetUtcNow().Subtract(staleAfter);
        var staleOrders = orders
            .Where(order => order.LastUpdatedAt <= cutoff)
            .OrderBy(order => order.LastUpdatedAt)
            .ToList();

        return new CleanupSummary(
            cutoff,
            staleOrders.Count,
            staleOrders.Sum(order => order.Order.Total),
            staleOrders.Select(order => order.Order.Id).ToArray());
    }
}
