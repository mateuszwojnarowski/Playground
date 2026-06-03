# Module 08 - SQL read from a Service Bus function

## Scenario
A Service-Bus-triggered function receives an order, opens a SQL connection, reads customer data from `LearningDb.dbo.Customers`, and enriches the outbound message. The data access code is isolated behind an interface so business logic can be tested without SQL Server or the Functions runtime.

## Acceptance criteria
- [ ] The example function is triggered by the `orders` queue and reads a customer by id.
- [ ] SQL access uses `Microsoft.Data.SqlClient` and async APIs.
- [ ] Queries are parameterised with `SqlParameter`.
- [ ] The exercise consumes `Order` messages from `enrich-in`.
- [ ] `OrderEnricher` depends on `ICustomerRepository` and returns an `EnrichedOrder`.
- [ ] The exercise publishes enriched messages to `orders-out`.
- [ ] Unit tests use fakes and do not hit a real database.

## Run locally

```bash
cd AzureFunctionsFundamentals/infra-local
docker compose up -d
./scripts/seed-sql.sh
```

Run the exercise function:

```bash
cd ../modules/08-sql-read/exercise
func start
```

Send a message to `enrich-in`:

```bash
az servicebus message send \
  --connection-string "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;" \
  --queue-name enrich-in \
  --body '{"id":"order-800","customerId":1,"product":"Keyboard","quantity":2,"unitPrice":49.99}'
```

Observe the enriched result on `orders-out` with Service Bus Explorer or any receiver connected to the emulator.

## Tests

```bash
cd AzureFunctionsFundamentals/modules/08-sql-read/exercise/tests
dotnet test
```

## Security
All SQL uses parameterised queries with `SqlParameter`. Never build SQL text by concatenating message values into the command string; parameters protect the query from SQL injection and preserve query plan reuse.
