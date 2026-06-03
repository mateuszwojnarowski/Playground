# Navigating and debugging in Azure

This guide shows where to look in the Azure portal when running the course solution in Azure, and how to connect portal views, logs, telemetry, and service state during debugging.

The course targets **Azure Functions v4**, **.NET 10**, and the **isolated worker** model. For telemetry setup, first read [Application Insights and logging](app-insights-logging.md).

## Guided tour of the Azure portal

Start from the **Resource group** created for the course deployment. Treat it as the map of the solution.

### Resource group

| Portal area | What to check | Why it matters |
|---|---|---|
| Overview | All deployed resources, region, tags | Confirms you are in the right environment |
| Deployments | Bicep deployment history and errors | Finds failed or partial infrastructure deployments |
| Access control (IAM) | Role assignments for users, apps, managed identities | Explains 403s and trigger connection failures |
| Activity log | Resource changes, restarts, configuration edits | Useful for configuration drift and incident timelines |
| Cost analysis | Spend by resource | Helps spot noisy telemetry or runaway compute |

If a value looks different from local course conventions, compare it with [`CONVENTIONS.md`](../CONVENTIONS.md), the Bicep templates in [`../deploy/bicep/`](../deploy/bicep/), and the [GitHub Actions deployment tutorial](../deploy/github-actions-tutorial.md).

### Function App

Open the Function App resource first for runtime issues.

| Blade | Use it for | Notes |
|---|---|---|
| Overview | App status, URL, plan, runtime, restart, deployment slot | Confirm the app is running and on the expected plan |
| Functions | List discovered functions | If a function is missing, suspect build/deploy output or extension/package issues |
| Functions -> Function -> Monitor / Invocations | Individual invocation status and logs | Best starting point for one failed run |
| Log stream | Near-real-time host and application logs | Useful immediately after triggering a function |
| Configuration / Environment variables | App settings and connection names | Check `AzureWebJobsStorage`, `ServiceBusConnection`, `SqlConnectionString`, `CosmosDbConnection`, `APPLICATIONINSIGHTS_CONNECTION_STRING` |
| Identity | System-assigned or user-assigned managed identity | Needed for identity-based connections and Azure RBAC |
| Diagnose and solve problems | Guided detectors for availability, performance, configuration | Good for cold starts, crashes, HTTP 5xx, and platform issues |
| Deployment Center | Deployment source and recent deployment status | Confirms the app version was deployed |
| Scale out / App Service plan | Instance count and scaling behaviour | Relevant for throughput and cold starts |

For .NET isolated worker apps, remember that the Functions host and your worker process are separate. A host error may appear before your code starts; an application error usually appears as worker logs, traces, exceptions, or invocation failure details.

### Service Bus namespace

Use Service Bus blades for modules 05, 06, 07, and 12.

| Blade | Use it for | Typical check |
|---|---|---|
| Overview | Namespace status, endpoint, pricing tier | Is the namespace available? |
| Queues | Queue list and message counts | `orders`, `orders-out`, `enrich-in` exist and have expected counts |
| Topics | Topic and subscription list | `order-events`, `audit`, `fulfilment` exist |
| Metrics | Incoming/outgoing messages, active/dead-letter counts, throttling | Detect stuck consumers or bursts |
| Service Bus Explorer | Peek, receive, send, and dead-letter inspect messages | Reproduce and inspect course scenarios |
| Shared access policies | SAS connection strings if used | Check only when the app uses connection strings |
| Access control (IAM) | RBAC for managed identity | Required for identity-based Service Bus access |

Service Bus Explorer is especially useful for a failed workflow:

1. Open the queue or subscription.
2. Check **Active messages** and **Dead-letter messages**.
3. Peek a message without removing it.
4. Copy `MessageId`, `CorrelationId`, delivery count, dead-letter reason, and application properties.
5. Search those values in Application Insights.

### Storage account

The Function App always needs a storage account for host state, triggers, leases, and scale coordination.

| Blade | Use it for | Course relevance |
|---|---|---|
| Overview | Account status, region, replication | Confirms the account exists and is healthy |
| Containers | Blob containers and host artefacts | Blob module containers such as `uploads`, `processed` |
| Queues | Storage queue messages | Queue module queue `incoming-jobs` |
| Tables / File shares | Host/runtime artefacts depending on configuration | Usually not edited manually |
| Access keys | Connection string diagnostics | Used when `AzureWebJobsStorage` is key-based |
| Access control (IAM) | Identity-based storage permissions | Needed when not using account keys |
| Metrics | Transactions, latency, availability | Diagnose storage-trigger and host storage issues |
| Networking | Firewall and private endpoint rules | Explains connectivity failures |

Do not delete the Functions host containers or files while the app is running unless you understand the runtime impact.

### SQL database

Use SQL blades for module 08 and any extension exercises.

| Blade | Use it for | Typical check |
|---|---|---|
| Overview | Server name, database status, connection strings | Confirms the target database |
| Query editor | Quick read-only checks | Verify `dbo.Customers` data for the course |
| Networking | Firewall, private endpoints, public access | Common cause of cloud-only failures |
| Microsoft Entra ID / Identity | Authentication configuration | Needed for token-based SQL access |
| Metrics | DTU/vCore, CPU, sessions, deadlocks | Spots overload or blocking |
| Diagnostic settings | Sends SQL logs to Log Analytics | Useful for deeper production diagnostics |

Connection string errors often appear as dependency failures in Application Insights and as exceptions in invocation logs.

### Cosmos DB account

Use Cosmos DB blades for modules 10, 11, and 12.

| Blade | Use it for | Course relevance |
|---|---|---|
| Data Explorer | Databases, containers, documents, queries | `LearningDb`, `orders`, `orders-leases`, `audit`, `products` |
| Metrics | RU consumption, 429s, latency, availability | Diagnose throttling and partition hot spots |
| Keys | Connection string diagnostics | Used if the app is key-based |
| Access control (IAM) | Identity-based access | Required for managed identity patterns |
| Networking | Firewall/private endpoint rules | Explains cloud connectivity failures |
| Replicate data globally | Region configuration | Useful when discussing latency and failover |

For Cosmos DB triggers, the lease container matters. If the function is not triggering, inspect both the source container and the leases container.

### Application Insights

Use Application Insights for the end-to-end view.

| Blade | Use it for |
|---|---|
| Overview | Failure rate, response time, traffic, availability |
| Application Map | Dependencies and service topology |
| Live Metrics | Near-real-time traffic and failures |
| Transaction search | One operation across requests, traces, exceptions, dependencies |
| Failures | Failed operations, exception grouping, dependency failures |
| Performance | Slow operations and dependency latency |
| Logs | KQL queries over all telemetry |
| Usage and estimated costs | Telemetry volume and cost control |
| Alerts | Operational notifications |

See [Application Insights and logging](app-insights-logging.md) for setup, `Program.cs`, sampling, structured logging, and KQL examples.

## Debugging a live Function

Use a repeatable flow. Do not start by changing code.

```text
Trigger/event -> Function invocation -> Logs/traces -> Dependencies -> Source service state -> Configuration/identity
```

### 1. Find the invocation

In the Function App:

1. Open **Functions**.
2. Select the function.
3. Open **Monitor** or **Invocations**.
4. Choose the failed or slow invocation.
5. Note the invocation ID, timestamp, duration, result, and log lines.

In Application Insights, search by operation ID or custom dimensions. If you logged `InvocationId`, `MessageId`, or `OrderId`, use those values.

```kusto
traces
| where timestamp > ago(6h)
| where tostring(customDimensions.InvocationId) == "PUT-INVOCATION-ID-HERE"
| project timestamp, severityLevel, message, operation_Id, customDimensions
| order by timestamp asc
```

If you only have an operation ID:

```kusto
let operationId = "PUT-OPERATION-ID-HERE";
union requests, traces, exceptions, dependencies, customEvents
| where operation_Id == operationId
| project timestamp, itemType, name, message, type, resultCode, success, duration, customDimensions
| order by timestamp asc
```

### 2. Read function execution logs

Look for:

- trigger binding errors before your function method starts,
- missing configuration values,
- authentication or authorisation failures,
- dependency failures,
- retry and delivery count messages,
- exception type and outer message,
- whether the function completed, abandoned, retried, or dead-lettered the message.

For isolated worker apps, a missing package or binding extension can prevent the function from appearing in the Functions list. A runtime exception in your code usually appears after the invocation starts.

### 3. Stream logs

Portal log stream:

1. Open the Function App.
2. Select **Log stream**.
3. Trigger the function.
4. Watch host and worker logs in near real time.

Azure Functions Core Tools:

```bash
func azure functionapp logstream <FUNCTION_APP_NAME> --resource-group <RESOURCE_GROUP_NAME>
```

Azure CLI fallback:

```bash
az webapp log tail \
  --name <FUNCTION_APP_NAME> \
  --resource-group <RESOURCE_GROUP_NAME>
```

If streaming is empty, check that application logging is enabled for the app, the app is running, and you are connected to the correct Function App and slot.

### 4. Use end-to-end transaction view

In Application Insights:

1. Open **Transaction search**.
2. Filter by time range, operation name, result, or custom property.
3. Open an operation.
4. Review the timeline: request/invocation, traces, dependencies, exceptions.
5. Use the dependency item to jump to SQL, Service Bus, Cosmos, Storage, or HTTP details.

Use **Failures** when you know something failed but do not know where. Use **Performance** when the function succeeds but is too slow.

### 5. Correlate a failed message to its invocation

For Service Bus or Storage Queue failures:

1. Open the queue/subscription in the portal.
2. Peek active or dead-letter messages.
3. Copy `MessageId`, `CorrelationId`, and relevant application properties such as `OrderId`.
4. Search Application Insights.

```kusto
traces
| where timestamp > ago(24h)
| where tostring(customDimensions.MessageId) == "PUT-MESSAGE-ID-HERE"
   or tostring(customDimensions.CorrelationId) == "PUT-CORRELATION-ID-HERE"
| project timestamp, severityLevel, message, operation_Id, customDimensions
| order by timestamp asc
```

Then inspect the full operation:

```kusto
let op = toscalar(
    traces
    | where timestamp > ago(24h)
    | where tostring(customDimensions.MessageId) == "PUT-MESSAGE-ID-HERE"
    | top 1 by timestamp desc
    | project operation_Id
);
union requests, traces, exceptions, dependencies, customEvents
| where operation_Id == op
| project timestamp, itemType, name, message, type, resultCode, success, duration, customDimensions
| order by timestamp asc
```

## Common failure scenarios and triage

### Function not triggering

| Possible cause | Where to check | Fix direction |
|---|---|---|
| Function not deployed or discovered | Function App -> Functions list; deployment logs | Rebuild and redeploy the correct project; check package references and generated functions metadata |
| Wrong connection setting name | Configuration / Environment variables | Match names from conventions: `AzureWebJobsStorage`, `ServiceBusConnection`, `SqlConnectionString`, `CosmosDbConnection` |
| Missing connection value | Configuration; App Insights exceptions | Add the app setting and restart |
| Managed identity lacks role | Function App -> Identity; target resource -> IAM | Grant the required data-plane role at the right scope |
| Source resource missing | Service Bus queues/topics, Storage queues, Cosmos containers | Deploy or create the expected queue/topic/container |
| Networking blocked | Resource networking blades | Allow Function App outbound path, private endpoint, or firewall rule |
| Trigger disabled | Function App function settings / app settings | Re-enable the function or remove disabling app setting |

Useful commands:

```bash
az functionapp function list \
  --name <FUNCTION_APP_NAME> \
  --resource-group <RESOURCE_GROUP_NAME> \
  --query "[].name" -o table

az functionapp config appsettings list \
  --name <FUNCTION_APP_NAME> \
  --resource-group <RESOURCE_GROUP_NAME> \
  --query "[].name" -o table
```

### Messages dead-lettering

| Symptom | Triage |
|---|---|
| Dead-letter count rising | Open Service Bus Explorer and inspect dead-letter reason/description |
| Delivery count reaches max | Search logs by `MessageId`; find repeated exception |
| Poison payload | Validate schema; log validation warning; decide whether to dead-letter intentionally |
| Lock lost | Check function duration against lock duration; reduce work per message or renew/adjust lock where appropriate |
| Downstream dependency failure | Inspect dependency failures in Application Insights |

KQL:

```kusto
traces
| where timestamp > ago(24h)
| where message has_any ("dead-letter", "DeliveryCount", "lock lost", "abandon")
| project timestamp, severityLevel, message, operation_Id, customDimensions
| order by timestamp desc
```

### Cold starts

Cold starts are normal when a serverless app has scaled to zero or needs a new worker.

| Check | Portal location |
|---|---|
| Hosting plan | Function App -> Overview |
| Instance activity and restarts | Diagnose and solve problems |
| First request after idle | Application Insights -> Performance / Logs |
| Package size and startup work | Deployment artefact and application startup logs |

Triage actions:

- Avoid heavy work in `Program.cs` startup.
- Move expensive initialisation into lazy services where safe.
- Use an appropriate hosting plan for latency-sensitive apps.
- Keep dependencies and configuration loading simple.

### Configuration drift

Configuration drift means the cloud app no longer matches source-controlled infrastructure or expected course settings.

Check:

```bash
az functionapp config appsettings list \
  --name <FUNCTION_APP_NAME> \
  --resource-group <RESOURCE_GROUP_NAME> \
  --output table

az deployment group list \
  --resource-group <RESOURCE_GROUP_NAME> \
  --query "[].{name:name,state:properties.provisioningState,time:properties.timestamp}" \
  --output table
```

Look in the resource group's **Activity log** for manual edits. Prefer redeploying infrastructure from Bicep rather than hand-fixing many settings.

### 401/403 on protected endpoints

This ties back to [module 09: Auth OIDC / OAuth2.0](../modules/09-auth-oidc-oauth2/).

| Status | Meaning | Triage |
|---|---|---|
| 401 | Caller is not authenticated | Missing bearer token, expired token, wrong issuer, token not sent to the Function App |
| 403 | Caller is authenticated but not allowed | Wrong audience/scope/role, app authorisation logic rejected the caller, missing Azure RBAC role |

Check:

- HTTP request in Application Insights `requests`.
- Authentication and authorisation traces from your function.
- Token issuer, audience, expiry, scopes, and roles.
- Easy Auth/App Service Authentication settings if enabled.
- API Management or front-door layer if one sits in front.

Never paste real access tokens into logs or tickets. Decode only in safe tooling and redact sensitive values.

### Cosmos DB 429 / RU throttling

A 429 means Cosmos DB is throttling because requested RU/s exceeded provisioned or autoscale capacity for that container or partition range.

| Where | What to inspect |
|---|---|
| Cosmos DB -> Metrics | Normalised RU consumption, 429 count, server-side latency |
| Application Insights -> dependencies | Cosmos dependency result code `429`, duration, operation name |
| Function logs | Request charge, retry-after, partition key, document count |
| Data Explorer | Partition key design and queried data shape |

KQL:

```kusto
dependencies
| where timestamp > ago(24h)
| where type has "Cosmos" or target has "documents.azure.com"
| where resultCode == "429"
| summarize count(), p95 = percentile(duration, 95) by bin(timestamp, 5m), name, target
| order by timestamp desc
```

Fix direction:

- Reduce query fan-out and request charge.
- Use point reads when possible.
- Batch carefully and honour retries.
- Increase RU/s or autoscale limits when workload justifies it.
- Revisit partition key design for hot partitions.

### SQL connectivity and firewall

| Symptom | Likely cause | Check |
|---|---|---|
| Login timeout | Firewall/private networking | SQL server -> Networking; Function App outbound path |
| Login failed | Wrong credentials or identity | App settings, managed identity, SQL users/roles |
| Cannot open database | Wrong database name or permissions | Connection string and SQL role assignments |
| Intermittent slow queries | Query or resource pressure | SQL metrics, Query Performance Insight, App Insights dependencies |

KQL:

```kusto
dependencies
| where timestamp > ago(24h)
| where type == "SQL"
| project timestamp, name, target, resultCode, success, duration, operation_Id, data
| order by timestamp desc
```

Azure CLI firewall example:

```bash
az sql server firewall-rule list \
  --resource-group <RESOURCE_GROUP_NAME> \
  --server <SQL_SERVER_NAME> \
  --output table
```

## Remote debugging notes

Remote debugging can be useful for short-lived diagnostics in non-production environments, but it should not be your main cloud debugging method.

| Topic | Guidance |
|---|---|
| Production | Prefer logs, metrics, traces, snapshots, and safe reproduction. Do not attach debuggers to production unless there is an approved incident process. |
| Linux Function Apps | Remote debugging support is more limited than Windows App Service scenarios. Check current portal and IDE support for your hosting plan. |
| Consumption plan | Instances are ephemeral and may scale out; attaching to one instance may not catch the next invocation. |
| Isolated worker | You debug your .NET worker process, not the Functions host. Startup and binding errors may happen before your debugger attaches. |
| Security | Debugging can expose secrets and memory contents. Restrict access and turn it off afterwards. |

Prefer this order:

1. Reproduce locally with emulators and the same app settings shape.
2. Add structured logs or custom telemetry.
3. Use App Insights transaction, failures, performance, and KQL.
4. Use staging slot or non-production remote debugging only when telemetry is insufficient.

## Where local and cloud differ

| Area | Local | Azure |
|---|---|---|
| Storage | Azurite and local queues/blobs | Azure Storage account and host storage |
| Service Bus | Emulator using course topology | Real namespace with RBAC/SAS, metrics, DLQs |
| SQL | Local SQL container from infra-local | Azure SQL firewall, identity, DTU/vCore limits |
| Cosmos | Local emulator or configured account | RU/s, partitioning, regional latency, firewall |
| Identity | Developer credentials or local secrets | Managed identity, app registration, RBAC |
| Configuration | `local.settings.json` | Function App environment variables |
| Telemetry | Console logs unless AI connection string is set | Application Insights and Log Analytics |
| Scale | Usually one local worker | Dynamic scale-out and concurrent executions |
| Cold start | Local process already running | Possible after idle or scale-out |

If a bug only occurs in Azure, suspect identity, networking, configuration, scale/concurrency, or service limits before assuming the function logic differs.

## Useful CLI commands

Set variables first:

```bash
RESOURCE_GROUP="<RESOURCE_GROUP_NAME>"
FUNCTION_APP="<FUNCTION_APP_NAME>"
```

List functions:

```bash
az functionapp function list \
  --resource-group "$RESOURCE_GROUP" \
  --name "$FUNCTION_APP" \
  --query "[].{name:name,disabled:config.disabled}" \
  --output table
```

Show app settings names without printing secret values:

```bash
az functionapp config appsettings list \
  --resource-group "$RESOURCE_GROUP" \
  --name "$FUNCTION_APP" \
  --query "[].name" \
  --output table
```

Restart after configuration changes:

```bash
az functionapp restart \
  --resource-group "$RESOURCE_GROUP" \
  --name "$FUNCTION_APP"
```

Stream logs:

```bash
func azure functionapp logstream "$FUNCTION_APP" --resource-group "$RESOURCE_GROUP"
```

Tail platform logs:

```bash
az webapp log tail \
  --resource-group "$RESOURCE_GROUP" \
  --name "$FUNCTION_APP"
```

Check recent deployments:

```bash
az deployment group list \
  --resource-group "$RESOURCE_GROUP" \
  --query "[].{name:name,state:properties.provisioningState,time:properties.timestamp}" \
  --output table
```

## Practical incident worksheet

| Question | Where to answer it |
|---|---|
| Is the app deployed and running? | Function App Overview, Deployment Center |
| Is the function discovered? | Function App -> Functions |
| Did the trigger fire? | Invocations, Log stream, source service metrics |
| What was the failing invocation? | Function Monitor, App Insights transaction search |
| What exception occurred? | Invocation logs, App Insights exceptions |
| Which dependency failed? | App Insights dependencies, Performance, Failures |
| Is the message stuck or dead-lettered? | Service Bus Explorer or Storage queue portal |
| Is configuration correct? | Function App Configuration and source-controlled deployment |
| Is identity authorised? | Function App Identity and target resource IAM |
| Is the service throttling or blocked? | Cosmos/SQL/Storage/Service Bus metrics and networking |

## Related documentation

- [Application Insights and logging](app-insights-logging.md)
- [GitHub Actions deployment tutorial](../deploy/github-actions-tutorial.md)
- Bicep infrastructure templates in [`../deploy/bicep/`](../deploy/bicep/)
- [Module 09: Auth OIDC / OAuth2.0](../modules/09-auth-oidc-oauth2/)
