using AzureFunctionsFundamentals.Shared;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;

namespace AzureFunctionsFundamentals.Modules.AppInsights.Exercise;

public sealed class OrderProcessingResult
{
    public bool IsSuccess { get; init; }
    public string? Error { get; init; }
    public static OrderProcessingResult Success() => new() { IsSuccess = true };
    public static OrderProcessingResult Failure(string error) => new() { IsSuccess = false, Error = error };
}

/// <summary>
/// Processes orders with proper Application Insights telemetry.
/// Your task is to implement this service following the logging best practices
/// described in docs/app-insights-logging.md and the acceptance criteria in README.md.
/// </summary>
public sealed class OrderTelemetryService
{
    private readonly ILogger<OrderTelemetryService> _logger;
    private readonly TelemetryClient _telemetry;

    public OrderTelemetryService(ILogger<OrderTelemetryService> logger, TelemetryClient telemetry)
    {
        _logger = logger;
        _telemetry = telemetry;
    }

    // TODO: Implement ProcessAsync.
    // Requirements (see README.md for full acceptance criteria):
    // 1. Use logger.BeginScope with OrderId and CustomerId before logging.
    // 2. Log at Information level that processing has started, including OrderId.
    // 3. If the order is null or CustomerId <= 0 or Product is empty, return Failure(...) and log a Warning.
    // 4. Log at Information level that processing completed successfully.
    // 5. Track a custom event "OrderProcessed" with properties: OrderId, CustomerId, Product, Total (as string).
    // 6. Track a metric "OrderValue" with the numeric value of order.Total.
    // 7. If an exception is thrown internally, catch it, log it with LogError(exception, ...) and return Failure.
    // 8. Never log raw payloads, PII, secrets, or connection strings.
    public Task<OrderProcessingResult> ProcessAsync(Order? order, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("TODO: implement ProcessAsync.");
    }

    // TODO: Implement TrackOrderDependency.
    // Requirements:
    // 1. Measure how long the async operation takes using DateTimeOffset.UtcNow before and after.
    // 2. Call telemetry.TrackDependency("InternalService", operationName, data, startTime, duration, success).
    // 3. Re-throw any exceptions that occur during the operation.
    public async Task TrackOrderDependency(string operationName, string data, Func<Task> operation)
    {
        throw new NotImplementedException("TODO: implement TrackOrderDependency.");
    }
}
