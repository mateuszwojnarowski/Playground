using System.Text.Json;
using AzureFunctionsFundamentals.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace QueueTriggerExample;

public sealed class QueueMessageLogger(ILogger<QueueMessageLogger> logger)
{
    [Function(nameof(QueueMessageLogger))]
    public void Run(
        [QueueTrigger("incoming-jobs", Connection = "AzureWebJobsStorage")] string message)
    {
        logger.LogInformation("Received queue message with {CharacterCount} characters.", message.Length);

        try
        {
            var order = JsonSerializer.Deserialize<Order>(message, JsonOptions.Default);
            if (order is not null)
            {
                logger.LogInformation(
                    "Parsed order {OrderId} for customer {CustomerId} with total {Total}.",
                    order.Id,
                    order.CustomerId,
                    order.Total);
            }
        }
        catch (JsonException)
        {
            logger.LogWarning("Queue message was not a valid Order JSON document.");
        }
    }
}

internal static class JsonOptions
{
    public static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web);
}
