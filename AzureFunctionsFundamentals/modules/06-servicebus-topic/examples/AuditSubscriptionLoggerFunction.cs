using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ServiceBusTopicExample;

public sealed class AuditSubscriptionLoggerFunction
{
    private readonly ILogger<AuditSubscriptionLoggerFunction> _logger;

    public AuditSubscriptionLoggerFunction(ILogger<AuditSubscriptionLoggerFunction> logger)
    {
        _logger = logger;
    }

    [Function(nameof(AuditSubscriptionLoggerFunction))]
    public void Run([ServiceBusTrigger("order-events", "audit", Connection = "ServiceBusConnection")] string message)
    {
        _logger.LogInformation("Audit subscription received order event: {Message}", message);
    }
}
