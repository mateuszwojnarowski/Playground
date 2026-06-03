# Uploading data to the local emulators

This guide shows how to put test data into the local emulators used by the course.

Start the stack first:

```bash
cd AzureFunctionsFundamentals/infra-local
docker compose up -d
```

The local topology is defined in [`CONVENTIONS.md`](../CONVENTIONS.md) and [`infra-local/README.md`](../infra-local/README.md).

## Azurite: blobs and storage queues

Azurite provides local Blob, Queue, and Table storage. In Functions settings the course uses:

```text
AzureWebJobsStorage=UseDevelopmentStorage=true
```

For Azure CLI commands, use the full Azurite development connection string used by `infra-local/scripts/seed-azurite.sh`:

```bash
CONN='DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1'
```

### Create and list blob containers

The course blob containers are `uploads` and `processed`.

```bash
az storage container create --name uploads --connection-string "$CONN"
az storage container create --name processed --connection-string "$CONN"
az storage container list --connection-string "$CONN" --query '[].name' --output table
```

### Upload a blob

From `AzureFunctionsFundamentals/infra-local`, create a small local file inside the course folder, then upload it:

```bash
mkdir -p ../.local-data
printf 'hello from Azurite\n' > ../.local-data/sample.txt

az storage blob upload \
  --container-name uploads \
  --name sample.txt \
  --file ../.local-data/sample.txt \
  --connection-string "$CONN" \
  --overwrite true
```

List blobs:

```bash
az storage blob list \
  --container-name uploads \
  --connection-string "$CONN" \
  --query '[].name' \
  --output table
```

### Create a storage queue and enqueue a message

The course storage queue is `incoming-jobs`.

```bash
az storage queue create --name incoming-jobs --connection-string "$CONN"

az storage message put \
  --queue-name incoming-jobs \
  --content '{"jobId":"job-001","fileName":"sample.txt"}' \
  --connection-string "$CONN"
```

Peek messages:

```bash
az storage message peek \
  --queue-name incoming-jobs \
  --connection-string "$CONN" \
  --num-messages 5
```

### Use Azure Storage Explorer

Azure Storage Explorer can connect to Azurite visually.

1. Start the local stack.
2. Open Azure Storage Explorer.
3. Choose the local emulator/Azurite connection option, or attach using the connection string above.
4. Browse the development storage account.
5. Create containers `uploads` and `processed` if needed.
6. Create queue `incoming-jobs` if needed.
7. Upload blobs or add queue messages through the UI.

## Service Bus emulator: queues and topics

The Service Bus emulator entities are created automatically from:

```text
AzureFunctionsFundamentals/infra-local/servicebus-emulator/config.json
```

Local namespace and entities:

```text
Namespace: sbemulatorns
Queues:    orders, orders-out, enrich-in
Topic:     order-events
Subs:      audit, fulfilment
```

Connection setting:

```text
ServiceBusConnection=Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;
```

### About Azure CLI and the emulator

The `az servicebus` commands are primarily Azure Resource Manager management commands for real Azure namespaces. They are not the best way to send data-plane messages to the local Service Bus emulator, and management operations such as creating queues should come from `config.json` instead.

For local message sending, prefer:

- a tiny .NET sender using `Azure.Messaging.ServiceBus`,
- a Service Bus Explorer tool that supports custom connection strings,
- the course modules themselves when they contain sender examples.

### Send a message with a tiny .NET snippet

From a scratch console app or LINQPad-style runner, reference `Azure.Messaging.ServiceBus` and use the emulator connection string.

```csharp
using Azure.Messaging.ServiceBus;

var connectionString = "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";

await using var client = new ServiceBusClient(connectionString);

ServiceBusSender queueSender = client.CreateSender("orders");
await queueSender.SendMessageAsync(new ServiceBusMessage("{\"orderId\":\"o-1001\",\"customerId\":\"c-001\"}")
{
    MessageId = "o-1001",
    ContentType = "application/json",
    Subject = "OrderSubmitted"
});

ServiceBusSender topicSender = client.CreateSender("order-events");
var topicMessage = new ServiceBusMessage("{\"orderId\":\"o-1001\",\"eventType\":\"OrderSubmitted\"}")
{
    MessageId = "evt-o-1001",
    ContentType = "application/json",
    Subject = "OrderSubmitted"
};
topicMessage.ApplicationProperties["eventType"] = "OrderSubmitted";
await topicSender.SendMessageAsync(topicMessage);
```

Send to other queues by changing the sender name to `orders-out` or `enrich-in`.

### Use Service Bus Explorer tooling

If your Service Bus Explorer tool supports custom connection strings:

1. Connect with `Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;`.
2. Browse queues `orders`, `orders-out`, and `enrich-in`.
3. Browse topic `order-events` and subscriptions `audit`, `fulfilment`.
4. Send JSON messages to a queue or to the topic.
5. Peek or receive messages from queues and subscriptions.

If a tool assumes a real Azure namespace or uses ARM management APIs only, it may not work with the emulator.

## Cosmos DB emulator

The Cosmos DB emulator runs at:

```text
https://localhost:8081/_explorer/index.html
```

Connection setting:

```text
CosmosDbConnection=AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==;
```

### Trust the emulator certificate

The emulator uses a self-signed certificate. For browser-based setup:

1. Open `https://localhost:8081/_explorer/index.html`.
2. Accept the browser warning for local development if prompted.
3. If your OS requires it, export the certificate from the browser and trust it in your user or machine certificate store.

For SDK clients, use emulator-friendly development settings where appropriate. Do not disable TLS validation for production Azure Cosmos DB accounts.

### Create the database and containers

Open Data Explorer and create database:

```text
Database: LearningDb
```

Create these containers exactly:

| Container | Partition key |
|---|---|
| `orders` | `/customerId` |
| `orders-leases` | `/id` |
| `audit` | `/customerId` |
| `products` | `/category` |

Use 400 RU/s throughput for local practice, matching `infra-local/cosmosdb/init.json`.

### Import seed data

The seed file is:

```text
AzureFunctionsFundamentals/infra-local/cosmosdb/init.json
```

It defines sample product documents for the `products` container:

```json
{
  "id": "p-001",
  "category": "books",
  "name": "Clean Architecture",
  "price": 38.50
}
```

Data Explorer does not import this topology file as-is. Use it as the source of truth:

1. Create `LearningDb`.
2. Create the four containers with the partition keys above.
3. In the `products` container, add the product documents from the `seed.products` array in `init.json`.

When you add each product manually, preserve the `id` and `category` fields because `category` is the partition key.

## SQL Server

The SQL Server container is exposed on port `1433`. The seed script applies:

```text
AzureFunctionsFundamentals/infra-local/sql/init.sql
```

It creates:

```text
Database: LearningDb
Table:    dbo.Customers(Id INT, Name NVARCHAR(200), Tier NVARCHAR(50))
```

Sample rows include Ada Lovelace, Alan Turing, Grace Hopper, Edsger Dijkstra, and Margaret Hamilton.

### Apply the SQL seed

From `AzureFunctionsFundamentals/infra-local`:

```bash
./scripts/seed-sql.sh
```

The script waits for SQL Server, copies `init.sql` into the container, and runs it with `sqlcmd`.

### Run ad-hoc queries with sqlcmd

Use `sqlcmd` inside the container:

```bash
docker exec aff-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P 'Your_password123!' \
  -C \
  -d LearningDb \
  -Q 'SELECT Id, Name, Tier FROM dbo.Customers ORDER BY Id;'
```

Or open an interactive prompt:

```bash
docker exec -it aff-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P 'Your_password123!' \
  -C \
  -d LearningDb
```

Then run:

```sql
SELECT COUNT(*) AS CustomerCount FROM dbo.Customers;
GO
```

## Quick verification checklist

| Emulator | Check |
|---|---|
| Azurite | `uploads`, `processed`, and `incoming-jobs` exist |
| Service Bus | `orders`, `orders-out`, `enrich-in`, and `order-events` exist from `config.json` |
| Cosmos DB | `LearningDb` has `orders`, `orders-leases`, `audit`, and `products` containers |
| SQL Server | `LearningDb.dbo.Customers` contains sample rows |
