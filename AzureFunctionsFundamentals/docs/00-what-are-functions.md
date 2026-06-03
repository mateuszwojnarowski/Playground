# What are Azure Functions?

Azure Functions are Microsoft's Functions-as-a-Service (FaaS) platform. They let you run small pieces of code in response to events without managing the servers that host that code.

This course uses **Azure Functions v4**, **.NET 10**, and the **isolated worker** model.

## Serverless and FaaS

**Serverless** does not mean “no servers”. It means the cloud platform owns server provisioning, patching, capacity management, and much of the runtime hosting. You focus on:

- the event that should start work,
- the code that performs the work,
- the services the code reads from or writes to,
- the operational behaviour: retries, logging, security, and scale.

**Functions-as-a-Service** is a serverless style where the deployable unit is a function: a small handler with a trigger.

```text
Event source              Function runtime              Your code
-----------               ----------------              ---------
HTTP request      --->    starts the function     --->   validate and respond
Queue message     --->    passes message data     --->   process one item
Timer schedule    --->    wakes on schedule       --->   run maintenance work
Blob created      --->    passes blob details     --->   transform a file
```

The model is **event-driven**. Instead of an application polling constantly, the platform invokes your function when something interesting happens.

## What Azure Functions provide

Azure Functions provide:

- a hosted Functions runtime,
- trigger and binding integrations for Azure services,
- local development through Azure Functions Core Tools (`func`),
- scaling based on incoming events,
- built-in retry and logging integration,
- deployment options from local tooling, CI/CD, or infrastructure-as-code.

A function is usually best when one event maps to one focused unit of work.

## Triggers and bindings

Every Azure Function has exactly one **trigger**. The trigger decides when the function runs.

A function can also use **bindings**. Bindings declaratively connect the function to another service.

- **Input binding**: reads data for the function.
- **Output binding**: writes data after the function runs.

In .NET isolated worker projects, triggers and bindings are normally declared with attributes in C# code. The Functions Worker SDK generates the underlying `function.json` metadata at build time.

### Common triggers

| Trigger | Starts when | Typical use | Course module |
|---|---|---|---|
| HTTP | An HTTP request arrives | APIs, webhooks, health checks | [HTTP trigger](../modules/01-http-trigger/) |
| Timer | A CRON schedule fires | Cleanup, polling, reports | [Timer trigger](../modules/02-timer-trigger/) |
| Storage Queue | A message appears in an Azure Storage queue | Simple asynchronous background work | [Storage Queue trigger](../modules/03-storage-queue-trigger/) |
| Blob | A blob is created or changed | File ingestion, image/document processing | [Blob trigger](../modules/04-blob-trigger/) |
| Service Bus Queue | A message appears in a Service Bus queue | Reliable enterprise work queues | [Service Bus queue](../modules/05-servicebus-queue/) |
| Service Bus Topic | A message reaches a topic subscription | Publish/subscribe workflows | [Service Bus topic](../modules/06-servicebus-topic/) |
| Cosmos DB | A change appears in the change feed | Reacting to document changes | [Cosmos DB trigger](../modules/10-cosmosdb-trigger/) |

### Binding example in words

```text
Storage Queue trigger: incoming-jobs
        |
        v
Function receives one queue message
        |
        +-- reads/writes application data
        |
        +-- output binding or SDK sends a result elsewhere
```

Bindings reduce boilerplate, but they are not magic. You still need to understand the service semantics: message locks, retries, leases, partition keys, and permissions.

## .NET 10 isolated worker model

This course uses the **isolated worker** model:

```text
Functions host process  <---- gRPC ---->  .NET isolated worker process
loads triggers/bindings                  runs your .NET 10 application code
```

The older **in-process** model ran function code inside the Functions host process. That made some integrations convenient, but it tightly coupled your application to the host runtime.

| Area | Isolated worker (.NET 10) | Old in-process model |
|---|---|---|
| Process boundary | Your code runs in a separate .NET worker process | Your code runs inside the Functions host process |
| .NET versioning | Better alignment with modern .NET releases | Tied more closely to host-supported runtime versions |
| Dependency injection | Uses normal .NET hosting patterns | Uses Functions-specific startup patterns |
| Middleware and configuration | Similar to modern ASP.NET Core/Worker Service apps | More host-specific |
| Direction of travel | Recommended path for current .NET Functions | Legacy model for older apps |

For .NET 10, isolated worker is the right model for new work. It gives clearer ownership of your application process, modern .NET hosting APIs, and a cleaner migration path as .NET evolves.

A minimal isolated worker entry point looks like this:

```csharp
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);
builder.ConfigureFunctionsWebApplication();
builder.Build().Run();
```

For non-HTTP modules, the course still follows the same isolated worker conventions from [`CONVENTIONS.md`](../CONVENTIONS.md).

## Hosting plans

Azure Functions can run on several hosting plans. The plan controls scale behaviour, billing, networking options, and cold-start characteristics.

| Plan | Best for | Scaling | Cold starts | Pricing intuition | Trade-offs |
|---|---|---|---|---|---|
| Consumption | Spiky, event-driven workloads with idle periods | Automatic scale out from zero | Possible after idle periods | Pay per execution, execution time, and memory | Simple and cheap when idle; fewer advanced hosting controls |
| Flex Consumption | Modern serverless workloads needing better scaling controls and networking options | Automatic, with configurable per-function scaling behaviour | Reduced compared with classic Consumption, depending on configuration | Serverless billing with more control | Newer plan; validate regional/features support for your needs |
| Premium | Production workloads needing low latency, VNET integration, or longer execution | Automatic scale, with pre-warmed instances | Avoidable with always-ready/pre-warmed instances | Pay for provisioned premium capacity | More predictable, but costs continue while capacity is allocated |
| Dedicated/App Service Plan | Apps that should share existing App Service capacity | Manual or App Service autoscale | No scale-to-zero cold start if instances stay warm | Pay for the App Service plan regardless of executions | Good when you already run always-on workloads; less serverless economically |

### Cold starts

A **cold start** happens when the platform needs to allocate or initialise a worker before your function can run. Cold starts are most visible when an app has scaled to zero or has been idle.

You can reduce their impact by:

- choosing Premium or Dedicated when latency must be predictable,
- keeping dependencies lean,
- avoiding expensive startup work,
- using async initialisation carefully,
- measuring startup time rather than guessing.

### Scaling

Azure Functions scale by adding more worker instances and processing more events concurrently. The exact rules depend on the trigger and plan.

```text
Queue depth increases
        |
        v
Scale controller observes backlog
        |
        v
More function instances start
        |
        v
Messages are processed in parallel
```

Scaling is powerful, but it can amplify downstream pressure. Protect databases and APIs with sensible concurrency settings, queues, retries, and back-pressure patterns.

## Pricing intuition

A useful mental model:

- If the workload is idle most of the time, serverless can be very cost-effective.
- If the workload runs constantly at high volume, a provisioned plan may be more predictable.
- If low latency is mandatory, you may pay for warm capacity.
- If each event fans out to expensive downstream calls, the cloud bill may be dominated by those services rather than Functions itself.

Always include storage, Service Bus, Cosmos DB, Application Insights, networking, and data transfer in cost estimates.

## When not to use Functions

Azure Functions are not the best fit for every workload. Consider another hosting model when you need:

- a long-running stateful process,
- very low and consistent latency without paying for warm capacity,
- complex in-memory coordination between requests,
- full control over the web server process,
- heavy CPU/GPU work better suited to containers, batch, or specialised compute,
- an application that is naturally a large monolith rather than event-driven units.

Durable Functions can handle many orchestration scenarios, but this fundamentals course focuses on core triggers and bindings.

## Key files in a Functions project

### `host.json`

`host.json` configures the Functions host for the app. It is committed to source control and applies across functions in the project.

Common areas include:

- logging,
- extension settings,
- retry and concurrency behaviour,
- route prefix for HTTP functions.

### `local.settings.json`

`local.settings.json` is for local development settings and secrets. It is normally not published to Azure.

In this course, modules use these setting names from the local infrastructure:

| Setting | Local value or purpose |
|---|---|
| `AzureWebJobsStorage` | `UseDevelopmentStorage=true` for Azurite |
| `ServiceBusConnection` | Local Service Bus emulator connection string |
| `SqlConnectionString` | SQL Server `LearningDb` connection string |
| `CosmosDbConnection` | Cosmos DB emulator connection string |

### `function.json`

`function.json` is metadata consumed by the Functions host. In .NET isolated worker projects, you normally do not write it by hand. The worker SDK generates it during build from your C# attributes.

```text
C# function attributes
        |
        v
Microsoft.Azure.Functions.Worker.Sdk
        |
        v
generated function.json metadata
        |
        v
Functions host loads trigger and binding definitions
```

## Continue through the course

Start with local setup, then work through the modules in order:

- [Local setup](local-setup.md)
- [Uploading to the emulators](upload-to-emulators.md)
- [HTTP trigger](../modules/01-http-trigger/)
- [Timer trigger](../modules/02-timer-trigger/)
- [Storage Queue trigger](../modules/03-storage-queue-trigger/)
- [Blob trigger](../modules/04-blob-trigger/)
- [Service Bus queue](../modules/05-servicebus-queue/)
- [Service Bus topic](../modules/06-servicebus-topic/)
- [Service Bus pipeline](../modules/07-servicebus-pipeline/)
- [SQL read](../modules/08-sql-read/)
- [Auth: OIDC / OAuth2.0](../modules/09-auth-oidc-oauth2/)
- [Cosmos DB trigger](../modules/10-cosmosdb-trigger/)
- [Cosmos DB read/write](../modules/11-cosmosdb-readwrite/)
- [Cosmos DB + Service Bus pipeline](../modules/12-cosmosdb-servicebus-pipeline/)
