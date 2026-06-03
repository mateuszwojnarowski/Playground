# Local Infrastructure

Everything you need to run the course locally: **Azurite** (storage), the
**Service Bus emulator** (+ its SQL Edge backend), **SQL Server**, and the
**Cosmos DB emulator**.

## Start the stack
```bash
cd AzureFunctionsFundamentals/infra-local
docker compose up -d
```

Wait ~1 minute for all containers to become healthy, then seed data:
```bash
./scripts/seed-sql.sh        # SQL Server -> LearningDb.dbo.Customers
./scripts/seed-azurite.sh    # blob containers + storage queue (needs Azure CLI)
./scripts/seed-cosmos.sh     # guidance for Cosmos containers (see upload doc)
```

The Service Bus queues/topics are created automatically from
`servicebus-emulator/config.json`.

## Connection strings (local only — safe to commit)

| Resource | Env var | Value |
|---|---|---|
| Storage (Azurite) | `AzureWebJobsStorage` | `UseDevelopmentStorage=true` |
| Service Bus | `ServiceBusConnection` | `Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;` |
| SQL Server | `SqlConnectionString` | `Server=localhost,1433;Database=LearningDb;User Id=sa;******;TrustServerCertificate=True;` |
| Cosmos DB | `CosmosDbConnection` | `AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==;` |

These exact names are referenced by every module's `local.settings.json`.
See `../CONVENTIONS.md` for the entity topology (queue/topic/container names).

## Stop & reset
```bash
docker compose down          # stop
docker compose down -v        # stop and wipe all data volumes
```

## Notes
- The Service Bus emulator needs the `sqledge` service; both accept their EULA
  via `ACCEPT_EULA=Y` in `docker-compose.yml`.
- The Cosmos DB emulator presents a self-signed TLS certificate. Local clients
  use `TrustServerCertificate`/emulator settings; see
  `../docs/upload-to-emulators.md` for importing the cert if needed.
