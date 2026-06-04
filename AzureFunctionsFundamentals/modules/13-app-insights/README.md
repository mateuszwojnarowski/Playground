# Module 13 — App Insights logging

This module focuses on **structured logging** and **Application Insights telemetry** in Azure Functions. You will practice how to emit useful operational logs, attach stable identifiers with scopes, and publish custom events and metrics that make troubleshooting easier in Azure Monitor.

Read [`../../docs/app-insights-logging.md`](../../docs/app-insights-logging.md) before starting. It explains the wiring, logging levels, scopes, dependency tracking, and telemetry practices used in this exercise.

## What you will learn
- How to wire Application Insights into a .NET 10 isolated Azure Functions app.
- How to use `ILogger<T>` with structured placeholders instead of interpolated operational log strings.
- How to attach `OrderId` and `CustomerId` to a processing block with `logger.BeginScope()`.
- How to publish custom events and metrics with `TelemetryClient`.
- How to log warnings and exceptions without leaking raw payloads, PII, or secrets.

## Acceptance criteria
- [ ] Add App Insights packages and wire up in Program.cs
- [ ] Inject `ILogger<T>` and use structured placeholders (not string interpolation)
- [ ] Use correct log levels: Information for normal flow, Warning for recoverable issues, Error for failures
- [ ] Use `logger.BeginScope()` to attach OrderId and CustomerId to all logs in a processing block
- [ ] Track a custom event with `TelemetryClient.TrackEvent("OrderProcessed", ...)`
- [ ] Track an order value metric with `TelemetryClient.TrackMetric("OrderValue", ...)`
- [ ] Log exceptions with `logger.LogError(exception, ...)`
- [ ] Never log raw payloads, PII, or connection strings

## Your task
Implement `exercise/OrderTelemetryService.cs`.

You need to:
1. Validate incoming orders and return `OrderProcessingResult.Failure(...)` for invalid input.
2. Start a logging scope that includes `OrderId` and `CustomerId`.
3. Emit start/completion logs at the correct level.
4. Emit a warning when the order is invalid.
5. Track the `OrderProcessed` custom event with the required properties.
6. Track the `OrderValue` metric.
7. Track a dependency operation with `TrackOrderDependency`.
8. Catch unexpected exceptions, log them with `LogError(exception, ...)`, and return a failure result.

## Run instructions
### Example project
```bash
dotnet run --project modules/13-app-insights/examples/AppInsightsExamples.csproj
```

### Exercise project
Run the tests first. They are expected to start **red** until you implement the service.

```bash
dotnet test modules/13-app-insights/exercise/tests/AppInsightsExercise.Tests.csproj
```

After the tests are failing for the expected reasons, implement `OrderTelemetryService` until they pass.
