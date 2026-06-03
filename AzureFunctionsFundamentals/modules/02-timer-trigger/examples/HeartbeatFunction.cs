using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureFunctionsFundamentals.Modules.TimerTrigger.Examples;

public sealed class HeartbeatFunction(ILogger<HeartbeatFunction> logger)
{
    [Function("Heartbeat")]
    public void Run([TimerTrigger("0 */5 * * * *")] TimerInfo timerInfo)
    {
        logger.LogInformation("Timer heartbeat fired at {Timestamp:O}. Next run: {NextRun:O}", DateTimeOffset.UtcNow, timerInfo.ScheduleStatus?.Next);
    }
}
