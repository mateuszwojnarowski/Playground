using System.Text.Json;
using AzureFunctionsFundamentals.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace QueueTriggerExample;

public sealed class QueueMessageLogger
{
    private readonly ILogger<QueueMessageLogger> _logger;

    public QueueMessageLogger(ILogger<QueueMessageLogger> logger)
    {
        _logger = logger;
    }

    [Function(nameof(QueueMessageLogger))]
    public void Run(
        [QueueTrigger("incoming-jobs", Connection = "AzureWebJobsStorage")] string message)
    {
        _logger.LogInformation("Received queue message: {Message}", message);

        try
        {
            var order = JsonSerializer.Deserialize<Order>(message, JsonOptions.Default);
            if (order is not null)
            {
                _logger.LogInformation(
                    "Parsed order {OrderId}: {Quantity} x {Product} = {Total:C}",
                    order.Id,
                    order.Quantity,
                    order.Product,
                    order.Total);
            }
        }
        catch (JsonException)
        {
            _logger.LogInformation("Message was not an Order JSON document; logged as plain text.");
        }
    }
}

internal static class JsonOptions
{
    public static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web);
}
