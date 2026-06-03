using AzureFunctionsFundamentals.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureFunctionsFundamentals.Modules.TimerTrigger.Exercise;

public sealed class DailyCleanupFunction(OrderCleanupService cleanupService, ILogger<DailyCleanupFunction> logger)
{
    [Function("DailyOrderCleanup")]
    public void Run([TimerTrigger("0 0 2 * * *")] TimerInfo timerInfo)
    {
        var sampleOrders = new[]
        {
            new OrderSnapshot(new Order { Id = "order-100", CustomerId = 1, Product = "Mouse", Quantity = 1, UnitPrice = 25m }, DateTimeOffset.UtcNow.AddDays(-45)),
            new OrderSnapshot(new Order { Id = "order-101", CustomerId = 2, Product = "Monitor", Quantity = 1, UnitPrice = 199m }, DateTimeOffset.UtcNow.AddDays(-5))
        };

        var summary = cleanupService.SummarizeStaleOrders(sampleOrders, TimeSpan.FromDays(30));

        logger.LogInformation(
            "Daily cleanup found {Count} stale orders before {Cutoff:O} totaling {Total:C}. Next run: {NextRun:O}",
            summary.StaleOrderCount,
            summary.Cutoff,
            summary.StaleOrderTotal,
            timerInfo.ScheduleStatus?.Next);
    }
}
