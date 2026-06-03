# Local setup

This guide prepares your machine to run the Azure Functions Fundamentals course locally with .NET 10, Azure Functions Core Tools, Docker-based emulators, and the Azure CLI.

## What runs locally

The local stack is defined in [`infra-local`](../infra-local/):

```text
Docker Compose
  ├── Azurite                         ports 10000, 10001, 10002
  ├── Azure Service Bus emulator       ports 5672, 5300
  ├── SQL Edge backend for emulator    internal only
  ├── SQL Server                       port 1433
  └── Cosmos DB emulator               ports 8081, 10250-10255
```

Connection setting names used by modules:

| Setting | Local value |
|---|---|
| `AzureWebJobsStorage` | `UseDevelopmentStorage=true` |
| `ServiceBusConnection` | `Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;` |
| `SqlConnectionString` | `Server=localhost,1433;Database=LearningDb;User Id=sa;******;TrustServerCertificate=True;` |
| `CosmosDbConnection` | `AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==;` |

## 1. Install .NET 10 SDK

Install the .NET 10 SDK for your operating system from the official .NET downloads page.

Verify:

```bash
dotnet --version
```

You should see a `10.x` SDK version.

## 2. Install Azure Functions Core Tools v4

Install **Azure Functions Core Tools v4**. The command must provide the `func` executable.

Verify:

```bash
func --version
```

You should see a v4 version.

## 3. Install Docker

Install Docker Desktop or Docker Engine with Docker Compose support.

Verify:

```bash
docker --version
docker compose version
```

Docker must be running before you start the emulators.

## 4. Install Azure CLI

Install the Azure CLI. It is used by the local seed scripts and is also useful for deployment modules later in the course.

Verify:

```bash
az version
```

You do not need to be logged in to Azure for the local Azurite commands.

## 5. Start the local stack

From the repository root:

```bash
cd AzureFunctionsFundamentals/infra-local
docker compose up -d
```

Wait about one minute for the containers to become healthy.

Check status:

```bash
docker compose ps
```

## 6. Seed local data

Still in `AzureFunctionsFundamentals/infra-local`, run:

```bash
./scripts/seed-sql.sh
./scripts/seed-azurite.sh
./scripts/seed-cosmos.sh
```

What these do:

| Script | Purpose |
|---|---|
| `seed-sql.sh` | Applies `sql/init.sql` to create `LearningDb.dbo.Customers` and seed sample customers |
| `seed-azurite.sh` | Creates blob containers `uploads`, `processed`, and storage queue `incoming-jobs` |
| `seed-cosmos.sh` | Prints Cosmos DB emulator guidance; see [Uploading to the emulators](upload-to-emulators.md) for the full data setup |

Service Bus entities are created automatically from `servicebus-emulator/config.json` when the emulator starts:

```text
Namespace: sbemulatorns
Queues:    orders, orders-out, enrich-in
Topic:     order-events
Subs:      audit, fulfilment
```

## 7. Run a single module locally

Each module contains an example or exercise Functions project. Open the module you are working on and run `func start` from the Functions project folder, the folder containing its `.csproj`, `host.json`, and `local.settings.json`.

Example pattern:

```bash
cd AzureFunctionsFundamentals/modules/01-http-trigger/examples
func start
```

If a module has a different project folder layout, use the module README as the source of truth.

When the host starts successfully, it lists the discovered functions and trigger endpoints.

## 8. Run tests

Run all tests from the course root:

```bash
cd AzureFunctionsFundamentals
dotnet test AzureFunctionsFundamentals.sln
```

Run tests for one module by targeting that module's test project or solution path, for example:

```bash
dotnet test modules/01-http-trigger
```

The course convention is that function adapters stay thin and real logic is unit-tested in plain services. Unit tests should not require live emulators unless a module explicitly says so.

## Troubleshooting

### Docker containers are not healthy

Check container status and logs:

```bash
cd AzureFunctionsFundamentals/infra-local
docker compose ps
docker compose logs servicebus
docker compose logs cosmosdb
```

If you need a clean reset:

```bash
docker compose down -v
docker compose up -d
```

This deletes emulator data volumes.

### Port conflicts

The local stack uses:

| Service | Ports |
|---|---|
| Azurite blob/queue/table | `10000`, `10001`, `10002` |
| Service Bus emulator | `5672`, `5300` |
| SQL Server | `1433` |
| Cosmos DB emulator | `8081`, `10250`-`10255` |

If startup fails, another process may already be using one of these ports. Stop the conflicting process or change the compose file only for your local experiment.

### Cosmos DB emulator TLS certificate

The Cosmos DB emulator uses a self-signed TLS certificate at `https://localhost:8081/`. Browser or SDK clients may reject it until you trust the certificate.

Open:

```text
https://localhost:8081/_explorer/index.html
```

If your browser warns about the certificate, accept it for local development. For SDK tooling, import/trust the emulator certificate as described in [Uploading to the emulators](upload-to-emulators.md).

### Cosmos Data Explorer does not load

Wait a little longer after `docker compose up -d`; the Cosmos emulator can take more time than the other services. Then check:

```bash
docker compose logs cosmosdb
```

Also confirm that port `8081` is not already in use.

### Service Bus emulator does not start

The Service Bus emulator requires its SQL Edge backend. In this stack the `servicebus` container depends on `sqledge`.

Check both logs:

```bash
docker compose logs sqledge
docker compose logs servicebus
```

The compose file sets `ACCEPT_EULA=Y` for the SQL containers and Service Bus emulator. If you create your own compose file, you must accept the EULAs there too.

### `func start` cannot find settings

Run `func start` from the Functions project directory, not from the repository root. The project directory should contain `host.json` and `local.settings.json`.

Confirm that `local.settings.json` contains the expected setting names:

- `AzureWebJobsStorage`
- `ServiceBusConnection`
- `SqlConnectionString`
- `CosmosDbConnection`

### Azurite seeding fails

Make sure Docker is running, Azurite is up, and Azure CLI is installed:

```bash
docker compose ps azurite
az version
./scripts/seed-azurite.sh
```

### SQL login fails

The local SQL Server container uses the password configured in `docker-compose.yml` and the database `LearningDb`. The local connection string is:

```text
Server=localhost,1433;Database=LearningDb;User Id=sa;******;TrustServerCertificate=True;
```

If the database is missing, rerun:

```bash
./scripts/seed-sql.sh
```
