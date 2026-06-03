# Module 10: Cosmos DB trigger

## Concept
A Cosmos DB change feed trigger runs when documents in a container are inserted or updated. It is useful for event-sourcing style workflows, materialised views, cache/projection updates, and replication because downstream code reacts to durable database changes instead of polling.

The Functions `CosmosDBTrigger` attribute points at database `LearningDb`, monitored container `orders`, and a lease container. This module uses `LeaseContainerName = "orders-leases"` and `CreateLeaseContainerIfNotExists = true`; leases coordinate progress so multiple function instances do not process the same feed range.

## Scenario
The exercise reacts to new or updated order documents in `orders` and writes an audit projection into the `audit` container with a Cosmos output binding. Projection code lives in `AuditProjector` so it can be tested without the Functions runtime or Cosmos emulator.

## Acceptance criteria
- [ ] Change feed trigger monitors `LearningDb/orders`.
- [ ] Lease container is `orders-leases`.
- [ ] Audit projection is written to `LearningDb/audit`.
- [ ] Business logic is unit-tested without a live emulator.

## Run locally
1. Start infrastructure: `cd AzureFunctionsFundamentals/infra-local && docker compose up -d`.
2. Create Cosmos containers as described in `docs/upload-to-emulators.md`: `orders` (`/customerId`), `orders-leases` (`/id`), and `audit` (`/customerId`).
3. Run the exercise project: `cd AzureFunctionsFundamentals/modules/10-cosmosdb-trigger/exercise && func start`.
4. Insert or update an order document in `LearningDb/orders`; the function logs the batch and writes an audit document.

## Tests
Run `dotnet test AzureFunctionsFundamentals/modules/10-cosmosdb-trigger/exercise/tests/CosmosDbTriggerExercise.Tests.csproj`.
