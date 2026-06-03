# Module 07 - Service Bus pipeline

## Scenario
Build a processing pipeline that receives an order from Service Bus, transforms it, and sends the transformed message to the next queue. Service Bus uses competing consumers and at-least-once delivery, so handlers must be idempotent: repeated delivery of the same order id should produce the same logical result and not rely on side effects inside the function adapter.

## Acceptance criteria
- [ ] The example function is triggered by the `orders` queue.
- [ ] The example function writes a transformed message to `orders-out` with a Service Bus output binding.
- [ ] The exercise function consumes `Order` messages from `enrich-in`.
- [ ] `OrderTransformer` normalises data, stamps processing metadata, and chooses a route by rules.
- [ ] The exercise publishes transformed messages to `orders-out`.
- [ ] Business logic is unit-tested without the Functions runtime or live emulators.

## Run locally
Start the local stack from the course root:

```bash
cd AzureFunctionsFundamentals/infra-local
docker compose up -d
```

Run the exercise function:

```bash
cd ../modules/07-servicebus-pipeline/exercise
func start
```

Send a message to `enrich-in`:

```bash
az servicebus message send \
  --connection-string "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;" \
  --queue-name enrich-in \
  --body '{"id":"order-700","customerId":1,"product":"  laptop stand  ","quantity":2,"unitPrice":75.50}'
```

Observe the result on `orders-out` with Service Bus Explorer or your preferred receiver using the same emulator connection string.

## Tests

```bash
cd AzureFunctionsFundamentals/modules/07-servicebus-pipeline/exercise/tests
dotnet test
```
