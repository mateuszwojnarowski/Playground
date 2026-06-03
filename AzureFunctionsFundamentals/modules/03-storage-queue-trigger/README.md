# Module 03: Storage Queue Trigger

## Concept

Azure Storage Queue triggers run a function when a message appears on a queue. They are useful for lightweight background work, buffering bursts, and decoupling producers from processors.

Messages are often JSON documents. The Functions host handles queue polling, visibility timeouts, retries, and poison queues. When a message repeatedly fails, it is moved to a poison queue named `<queue-name>-poison` so it can be inspected without blocking new work.

## Scenario

The exercise processes an `Order` JSON message from the `incoming-jobs` queue, calculates a processing result, and writes a JSON result document to the `processed` blob container with a blob output binding.

## Acceptance criteria

- [ ] Queue trigger listens to `incoming-jobs` using `AzureWebJobsStorage`.
- [ ] Messages are JSON `Order` documents.
- [ ] Business logic lives in `JobProcessor`, not in the Function method.
- [ ] The Function writes a result document to the `processed` blob container.
- [ ] Unit tests cover the processor without the Functions runtime or Azurite.

## Run locally

Start Azurite from `infra-local`, then run the exercise project:

```bash
cd AzureFunctionsFundamentals/modules/03-storage-queue-trigger/exercise
dotnet run
```

Enqueue a message against Azurite:

```bash
az storage message put \
  --queue-name incoming-jobs \
  --content '{"id":"order-1001","customerId":42,"product":"Keyboard","quantity":2,"unitPrice":49.99}' \
  --connection-string 'UseDevelopmentStorage=true'
```

If your tooling base64-encodes queue messages, the trigger still receives the decoded JSON payload through the Functions host.

## Tests

```bash
cd AzureFunctionsFundamentals/modules/03-storage-queue-trigger/exercise/tests
dotnet test
```
