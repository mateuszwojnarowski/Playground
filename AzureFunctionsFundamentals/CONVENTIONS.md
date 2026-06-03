# Build Conventions & Pinned Versions (READ FIRST)

This file is the single source of truth for every project in
`AzureFunctionsFundamentals`. All modules MUST follow these conventions so the
solution builds and tests consistently.

## Target framework & model
- **.NET 10** (`<TargetFramework>net10.0</TargetFramework>`)
- **Azure Functions v4**, **isolated worker** model (`dotnet-isolated`)
- `<Nullable>enable</Nullable>` and `<ImplicitUsings>enable</ImplicitUsings>`
- Test framework: **xUnit**

## Pinned NuGet package versions
Use exactly these versions everywhere they appear:

| Package | Version |
|---|---|
| Microsoft.Azure.Functions.Worker | 2.52.0 |
| Microsoft.Azure.Functions.Worker.Sdk | 2.0.7 |
| Microsoft.Azure.Functions.Worker.Extensions.Http.AspNetCore | 2.1.0 |
| Microsoft.Azure.Functions.Worker.Extensions.ServiceBus | 5.24.0 |
| Microsoft.Azure.Functions.Worker.Extensions.CosmosDB | 4.16.1 |
| Microsoft.Azure.Functions.Worker.Extensions.Storage.Queues | 5.5.4 |
| Microsoft.Azure.Functions.Worker.Extensions.Storage.Blobs | 6.8.1 |
| Microsoft.Azure.Functions.Worker.Extensions.Timer | 4.3.1 |
| Microsoft.Azure.Functions.Worker.ApplicationInsights | 2.50.0 |
| Microsoft.ApplicationInsights.WorkerService | 3.1.2 |
| Microsoft.Data.SqlClient | 7.0.1 |
| Microsoft.Azure.Cosmos | 3.61.0 |
| Microsoft.NET.Test.Sdk | 17.12.0 |
| xunit | 2.9.2 |
| xunit.runner.visualstudio | 2.8.2 |

## Standard Functions project (.csproj) shape
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <AzureFunctionsVersion>v4</AzureFunctionsVersion>
    <OutputType>Exe</OutputType>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Azure.Functions.Worker" Version="2.52.0" />
    <PackageReference Include="Microsoft.Azure.Functions.Worker.Sdk" Version="2.0.7" />
    <!-- add the trigger/binding extension packages this module needs -->
  </ItemGroup>
  <ItemGroup>
    <None Update="host.json" CopyToOutputDirectory="PreserveNewest" />
    <None Update="local.settings.json" CopyToOutputDirectory="PreserveNewest" CopyToPublishDirectory="Never" />
  </ItemGroup>
</Project>
```

## Standard test project (.csproj) shape
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\<FunctionProject>.csproj" />
  </ItemGroup>
</Project>
```

## Program.cs (isolated worker) baseline
```csharp
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);
builder.ConfigureFunctionsWebApplication(); // only needed for HTTP (ASP.NET Core integration)
builder.Build().Run();
```
For non-HTTP-only apps you may use `builder.ConfigureFunctionsWorkerDefaults()` style;
prefer `FunctionsApplication.CreateBuilder` for all modules for consistency.

## Testability rule
Functions are thin adapters. Put real logic in a plain injectable service/handler
class (e.g. `OrderProcessor`) and unit-test that class directly. Do not require the
Functions runtime or live emulators in unit tests.

## Local infrastructure names (defined by infra-local — DO NOT rename)
- **Storage / Azurite** connection string env name: `AzureWebJobsStorage` =
  `UseDevelopmentStorage=true`
- **Service Bus** connection string env name: `ServiceBusConnection`
  - Queues: `orders`, `orders-out`, `enrich-in`
  - Topic: `order-events` with subscriptions: `audit`, `fulfilment`
- **SQL Server** connection string env name: `SqlConnectionString`
  - DB: `LearningDb`, table `dbo.Customers(Id INT, Name NVARCHAR, Tier NVARCHAR)`
- **Cosmos DB** connection string env name: `CosmosDbConnection`
  - Database: `LearningDb`
  - Containers: `orders` (pk `/customerId`), `orders-leases` (pk `/id`),
    `audit` (pk `/customerId`), `products` (pk `/category`)
- **Storage queues** used: `incoming-jobs`; **Blob containers**: `uploads`, `processed`

See `infra-local/README.md` for connection string values.
