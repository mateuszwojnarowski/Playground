# Module 11: Cosmos DB read/write

## Concept
Functions can access Cosmos DB with bindings for simple input/output cases, or with the `CosmosClient` SDK when code needs query options, paging, retry-aware repositories, or richer data access. Partition keys matter: a point read by `id` plus partition key is usually cheapest, while cross-partition queries cost more RUs. Queries that filter by the partition key (`customerId`) stay scoped and predictable.

All SDK queries in this module use `QueryDefinition.WithParameter`; never concatenate user input into query text.

## Scenario
The exercise exposes an HTTP API. `POST /api/orders` creates or updates an order document in `LearningDb/orders`. `GET /api/orders/{customerId}` returns orders for one customer by partition key. Data access is behind `IOrderRepository`; tests use a fake repository instead of Cosmos.

## Acceptance criteria
- [ ] Function projects target `net10.0`, Functions v4 isolated worker.
- [ ] SDK repository uses parameterised Cosmos queries.
- [ ] POST upserts an order.
- [ ] GET queries by `customerId` partition key.
- [ ] Unit tests do not use a live emulator.

## Run locally
```bash
cd AzureFunctionsFundamentals/infra-local && docker compose up -d
# create LearningDb/orders with partition key /customerId per docs/upload-to-emulators.md
cd ../modules/11-cosmosdb-readwrite/exercise
func start
```

Create/update:
```bash
curl -i http://localhost:7071/api/orders \
  -H "Content-Type: application/json" \
  -d '{"id":"order-100","customerId":42,"product":"Keyboard","quantity":2,"unitPrice":19.5}'
```

Query by partition:
```bash
curl -i http://localhost:7071/api/orders/42
```

## Tests
Run `dotnet test AzureFunctionsFundamentals/modules/11-cosmosdb-readwrite/exercise/tests/CosmosDbReadWriteExercise.Tests.csproj`.
