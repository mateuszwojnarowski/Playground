# Azure Functions Fundamentals (.NET 10)

A hands-on course for learning **Azure Functions** end to end: concepts, every
major trigger, local emulators, real-world exercises with **acceptance criteria**
and **unit tests**, plus deployment (**Bicep** + **GitHub Actions**),
observability (**Application Insights**), and **token-based auth (OIDC/OAuth2.0)**.

Everything targets **.NET 10** and the **isolated worker** model on Functions v4.

## How to use this course
1. Read the preamble docs in `docs/` (what Functions are, what Service Bus is).
2. Start the local stack — see [`infra-local/README.md`](infra-local/README.md).
3. Work through the modules in order. Each module has:
   - a `README.md` explaining the concept and *why* it matters,
   - an `examples/` project — a minimal runnable sample,
   - an `exercise/` — a real-world scenario with **acceptance criteria** and a
     **unit test** project (tests start red, pass when you implement the logic).
4. When you're ready for the cloud, follow `deploy/` and the observability docs.

## Prerequisites
- .NET 10 SDK
- Docker (for the local emulators)
- Azure Functions Core Tools v4 (`func`)
- Azure CLI (`az`) — for seeding emulators and deploying

## Module index
| # | Module | Trigger / Topic |
|---|---|---|
| — | [What are Azure Functions?](docs/00-what-are-functions.md) | Preamble |
| — | [What is Azure Service Bus?](docs/01-what-is-service-bus.md) | Preamble |
| 01 | [HTTP trigger](modules/01-http-trigger/) | HTTP |
| 02 | [Timer trigger](modules/02-timer-trigger/) | Timer / schedule |
| 03 | [Storage Queue trigger](modules/03-storage-queue-trigger/) | Azure Storage Queue |
| 04 | [Blob trigger](modules/04-blob-trigger/) | Blob storage |
| 05 | [Service Bus queue](modules/05-servicebus-queue/) | SB queue receive |
| 06 | [Service Bus topic](modules/06-servicebus-topic/) | SB topic/subscription |
| 07 | [Service Bus pipeline](modules/07-servicebus-pipeline/) | SB receive → send |
| 08 | [SQL read](modules/08-sql-read/) | SB-triggered + SqlConnection |
| 09 | [Auth: OIDC / OAuth2.0](modules/09-auth-oidc-oauth2/) | Token-based auth |
| 10 | [Cosmos DB trigger](modules/10-cosmosdb-trigger/) | Cosmos change feed |
| 11 | [Cosmos DB read/write](modules/11-cosmosdb-readwrite/) | Cosmos SDK |
| 12 | [Cosmos DB + Service Bus pipeline](modules/12-cosmosdb-servicebus-pipeline/) | SB → Cosmos enrich |

## Operations & deployment
- [Local setup](docs/local-setup.md)
- [Uploading to the emulators](docs/upload-to-emulators.md)
- [Bicep deployment tutorial](deploy/bicep-tutorial.md)
- [GitHub Actions deployment tutorial](deploy/github-actions-tutorial.md)
- [Application Insights & logging](docs/app-insights-logging.md)
- [Navigating & debugging in Azure](docs/azure-navigation-debug.md)

## Build & test everything
```bash
cd AzureFunctionsFundamentals
dotnet build AzureFunctionsFundamentals.sln
dotnet test  AzureFunctionsFundamentals.sln
```

See [`CONVENTIONS.md`](CONVENTIONS.md) for pinned package versions and project
conventions used across every module.
