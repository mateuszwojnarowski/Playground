using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ServiceBusTopicExample;

public sealed class AuditSubscriptionLoggerFunction(ILogger<AuditSubscriptionLoggerFunction> logger)
{
    [Function(nameof(AuditSubscriptionLoggerFunction))]
    public void Run([ServiceBusTrigger("order-events", "audit", Connection = "ServiceBusConnection")] string message)
    {
        logger.LogInformation("Audit subscription received an event with {CharacterCount} characters.", message.Length);
    }
}
