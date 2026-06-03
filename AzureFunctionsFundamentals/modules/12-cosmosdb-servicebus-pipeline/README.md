# Module 12: Cosmos DB + Service Bus pipeline

## Concept
Service Bus and Cosmos DB often work together: a queue decouples producers from consumers, while Cosmos stores documents used for enrichment and projections. A Service Bus-triggered Function can receive an order message, query Cosmos for reference data, build a richer document, and write that document back to Cosmos.

## Scenario
The exercise listens on queue `enrich-in`, deserializes an `Order`, uses repository interfaces to look up product and customer data from Cosmos, builds an `EnrichedOrder`, and writes it through a Cosmos output binding. Enrichment is implemented in `CosmosOrderEnricher` and unit-tested with fakes.

## Acceptance criteria
- [ ] Service Bus trigger uses `ServiceBusConnection` and queue `enrich-in`.
- [ ] Cosmos SDK repositories use parameterised `QueryDefinition.WithParameter`.
- [ ] Enrichment logic is independent from the Functions runtime.
- [ ] Output binding writes an enriched document to Cosmos.

## Run locally
1. Start infrastructure: `cd AzureFunctionsFundamentals/infra-local && docker compose up -d`.
2. Create Cosmos containers per `docs/upload-to-emulators.md`: `orders`, `audit`, and `products`.
3. Run `cd AzureFunctionsFundamentals/modules/12-cosmosdb-servicebus-pipeline/exercise && func start`.
4. Send an order JSON message to Service Bus queue `enrich-in`; the function writes an enriched document to `orders`.

## Tests
Run `dotnet test AzureFunctionsFundamentals/modules/12-cosmosdb-servicebus-pipeline/exercise/tests/CosmosDbServiceBusPipelineExercise.Tests.csproj`.
