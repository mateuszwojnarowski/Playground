# Application Insights and logging

Application Insights is Azure Monitor's application performance management (APM) service. For Azure Functions it answers operational questions such as:

- Did this invocation run, fail, retry, or time out?
- Which dependency call made the function slow?
- Which queue, Service Bus, SQL, or Cosmos operation produced an exception?
- Are failures isolated to one function, customer, message, partition, or deployment?

This course uses **Azure Functions v4**, **.NET 10**, and the **isolated worker** model. The examples below follow the pinned package versions in [`CONVENTIONS.md`](../CONVENTIONS.md).

## What Application Insights collects

| Signal | What it tells you | Typical Functions example |
|---|---|---|
| Requests | A top-level operation handled by the app | HTTP-triggered function request, or a function invocation represented as an operation |
| Traces | Log messages from `ILogger` and the Functions host | `Order processed`, validation warnings, retry information |
| Exceptions | Unhandled and tracked exceptions | SQL timeout, Cosmos conflict, invalid payload |
| Dependencies | Outgoing calls to other systems | SQL, Service Bus, Azure Storage, Cosmos DB, HTTP APIs |
| Metrics | Numeric measurements over time | Queue depth, processing duration, items processed |
| Distributed traces | Correlated operations across services | HTTP request -> Service Bus message -> queue-triggered function -> Cosmos write |
| Live Metrics | Near-real-time health | Current request rate, failures, CPU, memory, sampled traces |

Application Insights stores telemetry in a Log Analytics workspace. You inspect it through portal blades such as **Application Map**, **Performance**, **Failures**, **Transactions**, **Live Metrics**, and **Logs** using KQL.

## Wire Application Insights into a .NET 10 isolated worker app

Add the exact packages from the course conventions:

| Package | Version | Purpose |
|---|---:|---|
| `Microsoft.Azure.Functions.Worker.ApplicationInsights` | `2.50.0` | Functions isolated-worker integration and correlation |
| `Microsoft.ApplicationInsights.WorkerService` | `3.1.2` | Application Insights telemetry for .NET worker services |

Example project references:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Azure.Functions.Worker" Version="2.52.0" />
  <PackageReference Include="Microsoft.Azure.Functions.Worker.Sdk" Version="2.0.7" />
  <PackageReference Include="Microsoft.Azure.Functions.Worker.ApplicationInsights" Version="2.50.0" />
  <PackageReference Include="Microsoft.ApplicationInsights.WorkerService" Version="3.1.2" />
</ItemGroup>
```

Set the app setting in Azure and in local settings when you want local telemetry to go to the same resource:

| Setting | Value |
|---|---|
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | The connection string from the Application Insights resource |

Prefer the connection string over the older instrumentation key setting.

### Complete `Program.cs` snippet

Use `FunctionsApplication.CreateBuilder` consistently with the course. HTTP modules normally call `ConfigureFunctionsWebApplication`; non-HTTP-only modules can use worker defaults, but the Application Insights calls are the same.

```csharp
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Logging.Services.Configure<LoggerFilterOptions>(options =>
{
    LoggerFilterRule? applicationInsightsRule = options.Rules.FirstOrDefault(rule =>
        rule.ProviderName ==
        "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");

    if (applicationInsightsRule is not null)
    {
        options.Rules.Remove(applicationInsightsRule);
    }
});

builder.Build().Run();
```

Why remove the logging rule? The Application Insights Worker Service SDK adds a default provider filter that only sends `Warning` and above. In an isolated Functions app, that can make `ILogger` `Information` logs appear locally but not in Application Insights. Removing that provider-specific rule lets normal logging configuration decide what flows.

## `host.json` logging and sampling

`host.json` configures the Functions host. It controls host/runtime logs, trigger behaviour, and built-in Application Insights sampling. It does **not** replace normal .NET worker logging configuration.

A practical starting point:

```json
{
  "version": "2.0",
  "logging": {
    "logLevel": {
      "default": "Information",
      "Function": "Information",
      "Host.Results": "Information",
      "Host.Aggregator": "Information",
      "Microsoft": "Warning",
      "Azure.Core": "Warning"
    },
    "applicationInsights": {
      "samplingSettings": {
        "isEnabled": true,
        "maxTelemetryItemsPerSecond": 20,
        "excludedTypes": "Request;Exception"
      },
      "enableLiveMetricsFilters": true
    }
  }
}
```

| Setting | Guidance |
|---|---|
| `default` | Use `Information` while learning; raise to `Warning` for noisy production paths if needed. |
| `Function` | Controls function-category logs produced by the host. |
| `Microsoft`, `Azure.Core` | Keep framework SDK logs at `Warning` unless diagnosing client internals. |
| `samplingSettings.isEnabled` | Keep enabled for normal workloads to control ingestion cost. |
| `excludedTypes` | Do not sample exceptions; many teams also exclude requests to preserve accurate counts. |
| `enableLiveMetricsFilters` | Keeps Live Metrics filtering aligned with configured filters. |

### Isolated-worker logging caveat

In the isolated model there are two processes:

```text
Functions host process  <---- gRPC ---->  .NET isolated worker process
host.json logging                        your ILogger<T> and DI services
```

`host.json` influences host logs and some categories, but your worker process also uses .NET logging filters. To keep `ILogger<T>` logs flowing to Application Insights:

1. Add `AddApplicationInsightsTelemetryWorkerService()`.
2. Add `ConfigureFunctionsApplicationInsights()`.
3. Set `APPLICATIONINSIGHTS_CONNECTION_STRING`.
4. Remove the default `ApplicationInsightsLoggerProvider` warning-only rule if you expect `Information` logs.
5. Keep `host.json` and any `appsettings.json` or code-based filters consistent.

## Structured logging with `ILogger<T>`

Structured logging records named properties, not just formatted strings. Use message templates with placeholders:

```csharp
logger.LogInformation(
    "Processing order {OrderId} for customer {CustomerId} from {Source}",
    order.Id,
    order.CustomerId,
    "service-bus");
```

Do **not** build log strings with interpolation when values should be queryable:

```csharp
// Avoid for operational logs because OrderId is no longer a separate property.
logger.LogInformation($"Processing order {order.Id}");
```

### Log levels

| Level | Use for | Example |
|---|---|---|
| `Trace` | Very detailed diagnostics, usually disabled | Per-record parser detail |
| `Debug` | Developer diagnostics | Branch decisions during troubleshooting |
| `Information` | Business milestones and normal progress | Message accepted, item processed |
| `Warning` | Unexpected but recoverable condition | Retry scheduled, optional data missing |
| `Error` | Operation failed | Dependency failure after retries |
| `Critical` | App or environment is unusable | Cannot start due to missing required configuration |

### Scopes and custom properties

Scopes attach properties to every log written within the scope. They are useful for message IDs, order IDs, tenant IDs, and correlation IDs.

```csharp
using var scope = logger.BeginScope(new Dictionary<string, object>
{
    ["OrderId"] = order.Id,
    ["CustomerId"] = order.CustomerId,
    ["MessageId"] = messageId
});

logger.LogInformation("Started order processing");
logger.LogInformation("Finished order processing");
```

Application Insights stores these values in `customDimensions`, where you can query them with KQL.

Best practices:

- Log stable identifiers, not entire payloads.
- Avoid secrets, tokens, connection strings, and personal data.
- Use consistent property names: `OrderId`, `CustomerId`, `MessageId`, `InvocationId`.
- Include the reason and the next action in warnings and errors.
- Use `LogError(exception, ...)` so the exception is linked to the trace.

## Custom telemetry with `TelemetryClient`

Most telemetry should come from `ILogger`, dependency auto-collection, and the Functions integration. Use `TelemetryClient` when you need explicit business events, metrics, or dependencies that auto-collection cannot see.

Inject it like any other service:

```csharp
using Microsoft.ApplicationInsights;

public sealed class OrderProcessor
{
    private readonly TelemetryClient telemetry;

    public OrderProcessor(TelemetryClient telemetry)
    {
        this.telemetry = telemetry;
    }
}
```

### Track events

```csharp
telemetry.TrackEvent("OrderAccepted", new Dictionary<string, string>
{
    ["OrderId"] = order.Id,
    ["CustomerId"] = order.CustomerId,
    ["Source"] = "ServiceBus"
});
```

Use events for domain milestones: `OrderAccepted`, `OrderRejected`, `CustomerLoaded`, `ProductEnriched`.

### Track metrics

```csharp
telemetry.TrackMetric("OrderValue", order.Total);
telemetry.TrackMetric("ItemsProcessed", itemCount);
```

Use metrics for numeric values you want to aggregate over time. Keep metric cardinality low; do not create a different metric name per customer or order.

### Track dependencies

Many dependencies are auto-collected, but explicit dependency telemetry is useful when a library or emulator does not emit enough detail.

```csharp
var start = DateTimeOffset.UtcNow;
var success = false;

try
{
    await repository.SaveAsync(order, cancellationToken);
    success = true;
}
finally
{
    telemetry.TrackDependency(
        dependencyTypeName: "SQL",
        dependencyName: "SaveOrder",
        data: "dbo.Orders",
        startTime: start,
        duration: DateTimeOffset.UtcNow - start,
        success: success);
}
```

Dependency naming guidance:

| Dependency | Type | Name examples | Useful data |
|---|---|---|---|
| SQL | `SQL` | `ReadCustomer`, `SaveOrder` | database/table/procedure name, never raw secrets |
| Service Bus | `Azure Service Bus` | `Send orders-out`, `Complete orders` | queue/topic/subscription, message ID |
| Cosmos DB | `Azure Cosmos DB` | `Read products`, `Upsert audit` | database/container, partition key shape, status code |
| HTTP | `Http` | `GET product-api` | method and route, not full sensitive query strings |

## Correlation and operation IDs

Application Insights groups related telemetry by operation. In KQL this appears as `operation_Id` and `operation_ParentId`.

For HTTP-triggered functions, correlation usually starts from the incoming HTTP trace context. For queue and Service Bus pipelines, propagate a correlation ID in message application properties so downstream functions can log it and, where possible, continue the distributed trace.

Recommended message properties:

| Property | Meaning |
|---|---|
| `CorrelationId` | End-to-end business correlation ID |
| `OrderId` | Domain identifier for examples in this course |
| `SourceInvocationId` | Function invocation that emitted the message |
| `traceparent` | W3C trace context when manually propagating distributed tracing |

In logs, include both the Functions invocation ID and the business correlation ID. They serve different purposes: invocation ID finds one run; correlation ID follows the workflow.

## Sampling and cost control

Telemetry has a cost: ingestion, retention, queries, and alert volume. Sampling keeps representative telemetry while reducing volume.

| Approach | How it works | Use when |
|---|---|---|
| Adaptive sampling | SDK adjusts sampling rate to stay near a target volume | Default for most apps with variable traffic |
| Fixed-rate sampling | You choose a fixed percentage | You need predictable sampling behaviour across services |
| No sampling | Every telemetry item is kept | Short diagnostics windows, low-volume apps, compliance-driven traces |

Cost-control practices:

- Keep adaptive sampling enabled for normal workloads.
- Exclude `Exception` from sampling so rare failures are preserved.
- Consider excluding `Request` if accurate request counts matter more than ingestion cost.
- Reduce noisy `Debug` and framework categories before lowering useful application logs.
- Prefer metric aggregation over logging one trace per loop item.
- Use short-term diagnostic changes, then revert them.
- Create alerts on rates and aggregates, not every single trace.

## Querying with KQL

Open the Application Insights resource, then **Logs**. These examples use common tables: `requests`, `traces`, `exceptions`, `dependencies`, and `customEvents`.

### Failures in the last hour

```kusto
requests
| where timestamp > ago(1h)
| where success == false
| project timestamp, name, resultCode, duration, operation_Id, cloud_RoleName
| order by timestamp desc
```

For non-HTTP triggers, failures may be easier to find in exceptions:

```kusto
exceptions
| where timestamp > ago(1h)
| project timestamp, type, outerMessage, operation_Id, cloud_RoleName, customDimensions
| order by timestamp desc
```

### Error and warning traces

```kusto
traces
| where timestamp > ago(1h)
| where severityLevel >= 2
| project timestamp, severityLevel, message, operation_Id, customDimensions
| order by timestamp desc
```

Severity levels are numeric: verbose `0`, information `1`, warning `2`, error `3`, critical `4`.

### Slowest dependencies

```kusto
dependencies
| where timestamp > ago(24h)
| summarize calls = count(), failures = countif(success == false), p95 = percentile(duration, 95), maxDuration = max(duration) by type, target, name
| order by p95 desc
```

### A specific invocation by operation ID

```kusto
let operationId = "PUT-OPERATION-ID-HERE";
union requests, traces, exceptions, dependencies, customEvents
| where operation_Id == operationId
| project timestamp, itemType, name, message, type, resultCode, success, duration, customDimensions
| order by timestamp asc
```

### Find an order or message ID

```kusto
traces
| where timestamp > ago(24h)
| where tostring(customDimensions.OrderId) == "12345"
   or tostring(customDimensions.MessageId) == "abc"
| project timestamp, message, operation_Id, customDimensions
| order by timestamp asc
```

### Service Bus or queue retry clues

```kusto
traces
| where timestamp > ago(24h)
| where message has_any ("retry", "dead-letter", "abandon", "lock")
| project timestamp, severityLevel, message, operation_Id, customDimensions
| order by timestamp desc
```

### Cosmos 429 throttling

```kusto
dependencies
| where timestamp > ago(24h)
| where type has "Cosmos" or target has "documents.azure.com"
| where resultCode == "429"
| summarize throttles = count(), p95 = percentile(duration, 95) by bin(timestamp, 5m), target, name
| order by timestamp desc
```

### SQL failures

```kusto
dependencies
| where timestamp > ago(24h)
| where type == "SQL"
| where success == false
| project timestamp, name, target, resultCode, duration, operation_Id, data
| order by timestamp desc
```

## Live Metrics

Use **Live Metrics** during active testing or an incident. It shows near-real-time request rate, failure rate, dependency calls, CPU, memory, and sample traces without waiting for normal ingestion latency.

Use it to answer:

- Is the Function App receiving traffic now?
- Are failures happening right now or only in historical logs?
- Did a deployment immediately change request/dependency duration?
- Are logs being filtered too aggressively?

Live Metrics is not a replacement for KQL. Use KQL for durable investigation and reports.

## Comprehensive logging checklist for the course

### Every module

- [ ] Log one clear start message and one completion message for meaningful work.
- [ ] Include `InvocationId` when available.
- [ ] Include business IDs such as `OrderId`, `CustomerId`, `MessageId`, or `BlobName`.
- [ ] Use structured placeholders, not interpolated operational log strings.
- [ ] Log exceptions with `LogError(exception, ...)`.
- [ ] Do not log secrets, access tokens, full connection strings, or sensitive payloads.
- [ ] Add custom events for important business milestones.
- [ ] Add metrics for counts, durations, and business quantities that need aggregation.

### HTTP trigger module

- [ ] Log route, method, response status, and safe request identifiers.
- [ ] Log validation failures as `Warning`, not `Error`, if the function handled them correctly.
- [ ] For protected endpoints in module 09, log authentication outcome without logging tokens.
- [ ] Query `requests` for status code and latency trends.

### Timer trigger module

- [ ] Log schedule occurrence, whether the run is past due, and items processed.
- [ ] Track a metric for batch size or records touched.
- [ ] Log idempotency decisions so repeated timer executions are understandable.

### Storage Queue and Blob modules

- [ ] Log queue message ID, dequeue count, blob container/name, and safe content metadata.
- [ ] Warn when dequeue count indicates repeated failures.
- [ ] Track dependency calls to Storage if SDK auto-collection is insufficient.

### Service Bus queue, topic, and pipeline modules

- [ ] Log `MessageId`, `CorrelationId`, `DeliveryCount`, queue/topic/subscription, and settlement decision.
- [ ] Preserve correlation IDs when sending downstream messages.
- [ ] Warn before messages approach max delivery count.
- [ ] Record dead-letter reason and description when manually dead-lettering.

### SQL module

- [ ] Log the logical operation, not raw SQL with sensitive parameters.
- [ ] Track dependency duration and result for important queries.
- [ ] Distinguish connection/firewall errors from query or data errors.

### Cosmos DB modules

- [ ] Log database, container, partition-key value shape, status code, and request charge when available.
- [ ] Warn on 429 throttling and include retry-after details when safe.
- [ ] Track metrics for RU charge and documents processed.

## Related documentation

- [Navigating and debugging in Azure](azure-navigation-debug.md)
- [GitHub Actions deployment tutorial](../deploy/github-actions-tutorial.md)
- Bicep infrastructure templates in [`../deploy/bicep/`](../deploy/bicep/)
