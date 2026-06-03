# Module 05 - Service Bus queue trigger

## Scenario

The order intake system receives JSON `Order` messages on the `orders` Service Bus queue. A queue-triggered Azure Function deserializes each message and delegates business decisions to injectable classes (`OrderConsumer` and `OrderValidator`) so the behavior can be unit-tested without the Functions runtime or the Service Bus emulator.

Azure Service Bus queues are useful when you need brokered messaging features that storage queues do not provide, including peek-lock delivery, `MaxDeliveryCount`, dead-letter queues, duplicate detection, sessions, and richer operational controls. In peek-lock mode the Functions trigger locks a message while the handler runs. If the handler succeeds, the message is completed. If the handler throws, the message is abandoned and becomes available again until `MaxDeliveryCount` is reached, at which point Service Bus moves it to the dead-letter queue. Throwing for invalid business rules is intentional in this exercise so poison orders are isolated for investigation.

## Acceptance criteria

- The example project logs messages consumed from the `orders` queue.
- The exercise consumes `AzureFunctionsFundamentals.Shared.Order` JSON from `orders`.
- Valid orders are accepted by `OrderConsumer`.
- Invalid business-rule failures throw `OrderValidationException`, allowing the Functions trigger to abandon the message and eventually dead-letter it.
- Unit tests cover valid processing and invalid orders without starting Functions or Service Bus.

## Run locally

Start the local infrastructure first:

```bash
cd AzureFunctionsFundamentals/infra-local
docker compose up -d
```

Run either Functions project:

```bash
cd AzureFunctionsFundamentals/modules/05-servicebus-queue/exercise
dotnet run
```

Send a test message to the emulator:

```csharp
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using AzureFunctionsFundamentals.Shared;

const string connectionString = "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";

await using var client = new ServiceBusClient(connectionString);
ServiceBusSender sender = client.CreateSender("orders");

var order = new Order
{
    CustomerId = 42,
    Product = "Keyboard",
    Quantity = 2,
    UnitPrice = 49.99m
};

await sender.SendMessageAsync(new ServiceBusMessage(JsonSerializer.Serialize(order)));
```

## Tests

```bash
cd AzureFunctionsFundamentals/modules/05-servicebus-queue/exercise/tests
dotnet test
```
