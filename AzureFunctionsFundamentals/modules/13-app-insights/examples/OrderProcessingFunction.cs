using System.Globalization;
using AzureFunctionsFundamentals.Shared;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureFunctionsFundamentals.Modules.AppInsights.Examples;

/// <summary>
/// Example HTTP function that demonstrates structured logging, scopes,
/// custom telemetry, and explicit dependency tracking with Application Insights.
/// </summary>
public sealed class OrderProcessingFunction(ILogger<OrderProcessingFunction> logger, TelemetryClient telemetry)
{
    [Function("ProcessOrderExample")]
    public async Task<IActionResult> ProcessOrderAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "telemetry/orders")] HttpRequest request,
        CancellationToken cancellationToken)
    {
        Order? order = await request.ReadFromJsonAsync<Order>(cancellationToken);
        if (order is null)
        {
            logger.LogWarning("Order processing request did not contain a valid payload.");
            return new BadRequestObjectResult(new { error = "A valid order payload is required." });
        }

        string orderId = string.IsNullOrWhiteSpace(order.Id) ? Guid.NewGuid().ToString() : order.Id;
        using var scope = logger.BeginScope(new Dictionary<string, object>
        {
            ["OrderId"] = orderId,
            ["CustomerId"] = order.CustomerId
        });

        logger.LogInformation("Processing order {OrderId} for customer {CustomerId}.", orderId, order.CustomerId);

        try
        {
            var dependencyStart = DateTimeOffset.UtcNow;
            var dependencySuccess = false;
            try
            {
                await Task.Delay(TimeSpan.FromMilliseconds(25), cancellationToken);
                dependencySuccess = true;
            }
            finally
            {
                telemetry.TrackDependency(
                    "InternalService",
                    "ReserveInventory",
                    "inventory-check",
                    dependencyStart,
                    DateTimeOffset.UtcNow - dependencyStart,
                    dependencySuccess);
            }

            var normalizedOrder = order with { Id = orderId, Product = order.Product.Trim() };
            telemetry.TrackEvent("OrderAccepted", new Dictionary<string, string>
            {
                ["OrderId"] = normalizedOrder.Id,
                ["CustomerId"] = normalizedOrder.CustomerId.ToString(),
                ["Product"] = normalizedOrder.Product,
                ["Total"] = normalizedOrder.Total.ToString(CultureInfo.InvariantCulture)
            });
            telemetry.TrackMetric("OrderValue", (double)normalizedOrder.Total);

            logger.LogInformation("Completed order {OrderId} with total {OrderTotal}.", normalizedOrder.Id, normalizedOrder.Total);
            return new OkObjectResult(new
            {
                message = "Order processed.",
                orderId = normalizedOrder.Id,
                total = normalizedOrder.Total
            });
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to process order {OrderId}.", orderId);
            return new ObjectResult(new { error = "Order processing failed." }) { StatusCode = StatusCodes.Status500InternalServerError };
        }
    }
}
