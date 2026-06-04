using AzureFunctionsFundamentals.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureFunctionsFundamentals.Modules.TimerTrigger.Exercise;

public sealed class DailyCleanupFunction(OrderCleanupService cleanupService, ILogger<DailyCleanupFunction> logger)
{
    // TODO: Implement the DailyOrderCleanup function.
    // Hints:
    // - Trigger: Use [TimerTrigger] attribute with cron expression "0 0 2 * * *" (runs daily at 2:00 AM).
    // - Parameter: Needs a TimerInfo parameter.
    // - Logic: Define sample order snapshots, invoke cleanupService.SummarizeStaleOrders, and log the summary details.
    [Function("DailyOrderCleanup")]
    public void Run([TimerTrigger("0 0 2 * * *")] TimerInfo timerInfo)
    {
        throw new NotImplementedException("TODO: Implement the Timer-triggered function according to the exercise guidelines.");
    }
}
