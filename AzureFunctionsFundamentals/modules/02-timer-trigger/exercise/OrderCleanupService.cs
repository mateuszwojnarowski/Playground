using AzureFunctionsFundamentals.Shared;

namespace AzureFunctionsFundamentals.Modules.TimerTrigger.Exercise;

public sealed record OrderSnapshot(Order Order, DateTimeOffset LastUpdatedAt);

public sealed record CleanupSummary(DateTimeOffset Cutoff, int StaleOrderCount, decimal StaleOrderTotal, IReadOnlyList<string> StaleOrderIds);

public sealed class OrderCleanupService(TimeProvider timeProvider)
{
    // TODO: Implement stale-order summarization.
    // - Calculate the cutoff from the injected clock and staleAfter.
    // - Select stale orders, count them, total their values, and return their ids in the required order.
    // - Match the behavior described in README.md for this module.
    public CleanupSummary SummarizeStaleOrders(IEnumerable<OrderSnapshot> orders, TimeSpan staleAfter)
    {
        throw new NotImplementedException("TODO: implement this method.");
    }
}
