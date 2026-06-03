# Module 06 - Service Bus topics and subscriptions

## Scenario

The order platform publishes one `Order` event to the `order-events` topic. Service Bus fans that event out to independent subscriptions so each downstream process can react in its own way. In this module the `audit` subscriber records an audit entry, while the `fulfilment` subscriber decides whether the order is ready for shipping.

Topics/subscriptions are a better fit than queues when one published event must drive multiple independent consumers. Subscription filters can narrow which messages a subscriber receives, while each subscription still has its own delivery state, retry policy, and dead-letter queue.

## Acceptance criteria

- The example project logs messages from the `audit` subscription on the `order-events` topic.
- The exercise has separate `audit` and `fulfilment` Service Bus topic-triggered functions.
- Both subscribers consume the same `AzureFunctionsFundamentals.Shared.Order` event.
- `AuditHandler` and `FulfilmentHandler` contain the business behavior and are unit-tested without Functions or Service Bus.
- Tests prove the same event drives different audit and fulfilment outcomes.

## Run locally

Start the local infrastructure first:

```bash
cd AzureFunctionsFundamentals/infra-local
docker compose up -d
```

Run the exercise Functions project:

```bash
cd AzureFunctionsFundamentals/modules/06-servicebus-topic/exercise
dotnet run
```

Publish an order event to the topic:

```csharp
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using AzureFunctionsFundamentals.Shared;

const string connectionString = "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";

await using var client = new ServiceBusClient(connectionString);
ServiceBusSender sender = client.CreateSender("order-events");

var order = new Order
{
    CustomerId = 42,
    Product = "Standing Desk",
    Quantity = 1,
    UnitPrice = 399m
};

await sender.SendMessageAsync(new ServiceBusMessage(JsonSerializer.Serialize(order)));
```

## Tests

```bash
cd AzureFunctionsFundamentals/modules/06-servicebus-topic/exercise/tests
dotnet test
```
