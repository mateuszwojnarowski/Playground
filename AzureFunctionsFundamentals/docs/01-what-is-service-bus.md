# What is Azure Service Bus?

Azure Service Bus is an enterprise messaging broker. It lets systems communicate by exchanging messages rather than calling each other directly.

In Azure Functions, Service Bus is commonly used to trigger background processing, buffer bursts of work, integrate systems, and build reliable publish/subscribe workflows.

## Enterprise messaging

A direct HTTP call couples two systems in time:

```text
System A  ---- HTTP request ---->  System B
System A waits while System B is available and responsive
```

Messaging decouples them:

```text
System A  ---- message ---->  Service Bus  ---- message ---->  System B
Producer can finish before consumer starts work
```

This gives you:

- asynchronous processing,
- buffering during traffic spikes,
- retries when consumers fail,
- competing consumers for scale-out,
- publish/subscribe fan-out,
- dead-letter queues for messages that cannot be processed.

## Queues

A **queue** is a one-to-one messaging pattern: each message is processed by one consumer.

```text
Producer(s)
   |
   v
+-------------------+
| Queue: orders     |
| msg1 msg2 msg3    |
+-------------------+
   |       |      |
   v       v      v
Worker  Worker  Worker
(one worker receives each message)
```

Queues are ideal for work items such as “process this order”, “send this email”, or “generate this report”.

## Topics and subscriptions

A **topic** is a publish/subscribe pattern. Producers send to the topic. Consumers receive from subscriptions.

```text
Producer
   |
   v
+-----------------------+
| Topic: order-events   |
+-----------------------+
   |                 |
   v                 v
Subscription: audit  Subscription: fulfilment
   |                 |
   v                 v
Audit function       Fulfilment function
```

Each subscription has its own message copy and can be processed independently.

### Subscription filters

Subscriptions can use filters so that each subscriber receives only relevant messages.

```text
Topic: order-events
   |
   +--> audit subscription       filter: all messages
   |
   +--> fulfilment subscription filter: eventType = 'OrderSubmitted'
```

Filters may be simple boolean expressions over message properties, correlation filters, or SQL-like filters depending on your design. The local course topology creates the subscriptions; modules can then demonstrate how consumers use them.

## Local topology for this course

The local Service Bus emulator is configured by [`infra-local/servicebus-emulator/config.json`](../infra-local/servicebus-emulator/config.json). Names must match [`CONVENTIONS.md`](../CONVENTIONS.md).

| Item | Local name |
|---|---|
| Namespace | `sbemulatorns` |
| Connection setting | `ServiceBusConnection` |
| Connection string | `Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;` |
| Queues | `orders`, `orders-out`, `enrich-in` |
| Topic | `order-events` |
| Subscriptions | `audit`, `fulfilment` |

```text
Queues:
  orders       -> queue-triggered order processing
  orders-out   -> downstream queue output examples
  enrich-in    -> enrichment pipeline input

Topic:
  order-events
      ├── audit
      └── fulfilment
```

The configured queue and subscription defaults include a one-hour message TTL, one-minute lock duration, and maximum delivery count of five.

## Core concepts

### Competing consumers

Multiple consumers can read from the same queue. Service Bus gives each message to one consumer at a time.

```text
orders queue
  msg1 -> Function instance A
  msg2 -> Function instance B
  msg3 -> Function instance C
```

This is how queue-triggered Functions scale out while preserving one successful processing outcome per message.

### Load levelling

Queues absorb bursts. Producers can enqueue quickly while consumers process at a safe rate.

```text
Traffic burst -> queue depth rises -> consumers drain backlog over time
```

This protects downstream systems such as SQL Server, Cosmos DB, payment gateways, and third-party APIs.

### Decoupling

The producer does not need to know which service will process the message, where it is hosted, or whether it is temporarily offline. It only needs the broker contract: queue or topic name, message shape, and message properties.

### Sessions

Sessions group related messages so they are processed in order by one consumer at a time. A session is useful when messages for the same business entity must be handled sequentially.

```text
SessionId = customer-123
  msg1 -> msg2 -> msg3 processed in order
```

The course emulator queues are configured with `RequiresSession: false`, so the early modules can focus on the basic receive/send model.

### Dead-letter queues

Each queue and subscription has a dead-letter queue (DLQ). Messages move there when they cannot be delivered or processed successfully, for example after too many failed delivery attempts.

```text
orders queue -> processing fails repeatedly -> orders/$DeadLetterQueue
```

A DLQ is not a rubbish bin. It is an operational signal. Inspect it, fix the cause, and decide whether to replay or discard messages.

### Peek-lock vs receive-and-delete

Service Bus supports two receive modes.

| Mode | Behaviour | Use when |
|---|---|---|
| Peek-lock | Receiver locks the message, processes it, then completes it. If processing fails, the lock expires and the message can be retried. | Most business workloads |
| Receive-and-delete | Broker removes the message as soon as it is delivered. If the consumer crashes, the message is lost. | Telemetry-like data where occasional loss is acceptable |

Azure Functions Service Bus triggers use reliable message settlement patterns. If your function fails, the runtime can abandon or retry the message according to configuration and broker state.

### Duplicate detection

Duplicate detection lets Service Bus discard messages with the same `MessageId` during a configured window. It helps when producers may retry sends after uncertain failures.

The local course entities set `RequiresDuplicateDetection: false`, so examples should not rely on broker-side duplicate suppression unless you change the topology deliberately.

### Message TTL

Time-to-live (TTL) controls how long a message may remain valid. Expired messages are no longer delivered. Depending on entity settings, expired messages can be dead-lettered.

The local Service Bus emulator entities use `DefaultMessageTimeToLive: PT1H`.

## Service Bus vs Storage Queues

Both Service Bus queues and Azure Storage queues can trigger Azure Functions, but they serve different needs.

| Capability | Service Bus | Storage Queues |
|---|---|---|
| Primary role | Enterprise broker | Simple, large-scale queue storage |
| Topics/subscriptions | Yes | No |
| Subscription filters | Yes | No |
| Sessions/order grouping | Yes | No native equivalent |
| Duplicate detection | Yes | No broker-level duplicate detection |
| Dead-letter queue | Built in | No automatic DLQ; design your own poison handling |
| Transactions | Broker features available | Simpler storage semantics |
| Message size and features | Rich broker message model with properties | Simpler message model |
| Operational complexity | More features to understand | Easier to start with |
| Local emulator in this course | Service Bus emulator | Azurite |

Choose **Storage Queues** when you need a simple queue for straightforward background work and do not need broker features.

Choose **Service Bus** when you need enterprise messaging: pub/sub, subscription filters, sessions, richer dead-letter handling, duplicate detection, or clearer integration contracts between systems.

## Real-world scenarios

### Order processing

```text
Checkout API -> orders queue -> order processor -> orders-out queue
```

The API returns quickly after accepting the order. A Function processes the order asynchronously. If processing fails, Service Bus retries and eventually dead-letters the message for investigation.

### System integration

```text
Order system -> order-events topic
                  ├── audit subscription
                  └── fulfilment subscription
```

The order system publishes once. Audit and fulfilment services evolve independently.

### Retries and back-pressure

```text
Producer sends faster than database can handle
        |
        v
Queue depth increases
        |
        v
Functions process with controlled concurrency
```

The queue protects the database from sudden spikes. You can tune function concurrency, lock duration, and downstream retry policies.

### Enrichment pipeline

```text
enrich-in queue -> Function reads product/customer data -> Cosmos DB or orders-out
```

A pipeline can add data from SQL Server, Cosmos DB, or external APIs without blocking the producer.

## How Azure Functions fit

A Service Bus-triggered Function is a message handler hosted by the Functions runtime.

```text
Service Bus entity
        |
        v
Functions Service Bus trigger
        |
        v
Thin function adapter
        |
        v
Testable application service/handler
```

For this course, keep function classes thin and put business logic in plain services that can be unit tested without live emulators.
