// =============================================================================
// SimpleProducer.cs - BEGINNER LEVEL: Your first Kafka Producer
// =============================================================================
//
// WHAT IS A KAFKA PRODUCER?
//   A producer is an application that PUBLISHES (writes) messages to Kafka.
//   Think of it like a writer posting articles to a news feed - it sends data
//   and Kafka stores it reliably for consumers to read later.
//
// CORE CONCEPTS COVERED:
//   1. ProducerConfig - How to configure a Kafka producer
//   2. ProduceAsync vs Produce - Async (recommended) vs fire-and-forget
//   3. DeliveryResult - Confirmation that the message was stored
//   4. Topics - Named channels where messages are published
//   5. Error handling - What to do when things go wrong
//
// PREREQUISITES:
//   - Kafka must be running locally (see docker-compose.yml)
//   - Default broker: localhost:9092
//
// HOW KAFKA STORES MESSAGES:
//   Messages are stored in TOPICS. A topic is like a database table or a
//   folder - it holds related messages. Topics are divided into PARTITIONS
//   which allow parallel reads and writes. Each message gets an OFFSET
//   (sequential ID) within its partition.
//
//   Topic: "orders"
//   ┌─────────────────────────────────────────────┐
//   │ Partition 0: [msg0] [msg1] [msg2] [msg3]    │
//   │ Partition 1: [msg0] [msg1] [msg2]           │
//   │ Partition 2: [msg0] [msg1] [msg3] [msg4]    │
//   └─────────────────────────────────────────────┘

using Confluent.Kafka;

namespace Kafka101.Beginner;

/// <summary>
/// Demonstrates basic Kafka message production.
/// Start here if you're new to Kafka!
/// </summary>
public class SimpleProducer
{
    // -------------------------------------------------------------------------
    // CONFIGURATION:
    // The ProducerConfig holds all settings for your producer.
    // At minimum, you need BootstrapServers - the address(es) of your Kafka broker(s).
    //
    // A "broker" is a Kafka server. In production you'd have multiple brokers
    // for high availability (e.g., "broker1:9092,broker2:9092,broker3:9092").
    // -------------------------------------------------------------------------
    private readonly ProducerConfig _config;

    // The topic name where we'll publish messages.
    // BEST PRACTICE: Use lowercase, hyphen-separated names (kebab-case).
    // Examples: "user-signups", "order-events", "sensor-readings"
    private readonly string _topicName;

    /// <summary>
    /// Initializes the SimpleProducer with broker connection details.
    /// </summary>
    /// <param name="bootstrapServers">
    /// Comma-separated list of Kafka broker addresses.
    /// Example: "localhost:9092" or "broker1:9092,broker2:9092"
    /// </param>
    /// <param name="topicName">The Kafka topic to publish messages to.</param>
    public SimpleProducer(string bootstrapServers, string topicName)
    {
        _topicName = topicName;

        // -----------------------------------------------------------------------
        // ProducerConfig EXPLAINED:
        //
        // BootstrapServers: Initial connection point(s) to the Kafka cluster.
        //   These are used for initial discovery only - Kafka then provides
        //   the full cluster metadata. You don't need to list ALL brokers.
        //
        // Acks (Acknowledgment mode):
        //   - Acks.None (0): Producer doesn't wait for broker confirmation.
        //     FASTEST but messages can be LOST if the broker crashes.
        //     Use for: metrics, logs where occasional loss is acceptable.
        //
        //   - Acks.Leader (1): Wait for the partition leader to write to disk.
        //     BALANCED - good speed, messages survive leader crashes.
        //     This is the DEFAULT.
        //
        //   - Acks.All (-1): Wait for ALL in-sync replicas to confirm.
        //     SAFEST - no data loss even if multiple brokers fail.
        //     Use for: financial transactions, order confirmations.
        //
        // MessageTimeoutMs: How long to wait before giving up on delivery.
        //   Default is 300000ms (5 minutes). Reduce for time-sensitive use cases.
        // -----------------------------------------------------------------------
        _config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,

            // Use Acks.All for guaranteed delivery (the safest option)
            Acks = Acks.All,

            // Give up after 30 seconds if message can't be delivered
            MessageTimeoutMs = 30000,

            // OPTIONAL: ClientId helps you identify your producer in Kafka logs
            // and monitoring tools like Kafka UI or Confluent Control Center.
            ClientId = "kafka101-simple-producer"
        };
    }

    /// <summary>
    /// Sends a single text message to Kafka asynchronously.
    ///
    /// WHY ASYNC?
    /// Async sending is preferred because it doesn't block your thread while
    /// waiting for Kafka's acknowledgment. Your application can handle other
    /// work while the network round-trip completes.
    ///
    /// THE FLOW:
    /// 1. Producer serializes the message (string → bytes)
    /// 2. Producer sends bytes to the Kafka broker
    /// 3. Broker writes to disk and replicates (if Acks.All)
    /// 4. Broker sends acknowledgment back
    /// 5. DeliveryResult is returned with confirmation details
    /// </summary>
    /// <param name="message">The text message to send.</param>
    public async Task SendMessageAsync(string message)
    {
        // -----------------------------------------------------------------------
        // IProducer<TKey, TValue>:
        //   The producer is generic - TKey is the message key type, TValue is
        //   the message value type. Here we use <Null, string> because:
        //   - Null key = no key, Kafka uses round-robin partition assignment
        //   - string value = our message content
        //
        //   In Intermediate level, we'll use <string, string> keys for ordering.
        //   In Advanced level, we'll use custom serializers for complex objects.
        //
        // USING STATEMENT:
        //   Always use `using` to dispose the producer when done.
        //   Disposing ensures all buffered messages are flushed to Kafka before
        //   the producer is cleaned up. Forgetting this = potential message loss!
        // -----------------------------------------------------------------------
        using var producer = new ProducerBuilder<Null, string>(_config).Build();

        try
        {
            Console.WriteLine($"\n📤 Sending message: \"{message}\"");
            Console.WriteLine($"   Topic: {_topicName}");

            // -------------------------------------------------------------------
            // Message<TKey, TValue>:
            //   Wraps the actual data you want to send.
            //   - Key: For ordering guarantees (null here = no specific partition)
            //   - Value: The actual message content
            //   - Headers (optional): Metadata like correlation IDs, content-type
            //   - Timestamp (optional): Defaults to current time if not set
            // -------------------------------------------------------------------
            var kafkaMessage = new Message<Null, string>
            {
                Value = message
                // Key is implicitly null for <Null, string> producer
            };

            // -------------------------------------------------------------------
            // ProduceAsync - THE RECOMMENDED WAY:
            //   Returns a Task<DeliveryResult> that completes when Kafka
            //   has acknowledged receipt of the message.
            //
            //   DeliveryResult contains:
            //   - Topic: Where the message was stored
            //   - Partition: Which partition received it
            //   - Offset: The message's position in the partition (starts at 0)
            //   - Status: MessageStatus.Persisted = safely stored!
            //
            // ALTERNATIVE - Produce (fire-and-forget):
            //   producer.Produce(topicName, message, deliveryHandler);
            //   This is non-blocking but you handle the result via a callback.
            //   See SendMultipleMessagesAsync for that pattern.
            // -------------------------------------------------------------------
            var deliveryResult = await producer.ProduceAsync(_topicName, kafkaMessage);

            // Success! Print the confirmed storage location
            Console.WriteLine($"✅ Message delivered successfully!");
            Console.WriteLine($"   Topic:     {deliveryResult.Topic}");
            Console.WriteLine($"   Partition: {deliveryResult.Partition}");
            Console.WriteLine($"   Offset:    {deliveryResult.Offset}");
            Console.WriteLine($"   Status:    {deliveryResult.Status}");
            Console.WriteLine($"   Timestamp: {deliveryResult.Timestamp.UtcDateTime:HH:mm:ss.fff} UTC");
        }
        catch (ProduceException<Null, string> ex)
        {
            // -------------------------------------------------------------------
            // ProduceException: Thrown when the broker rejects the message.
            // Common reasons:
            // - TOPIC_AUTHORIZATION_FAILED: No permission to write to this topic
            // - MESSAGE_TOO_LARGE: Message exceeds broker's max.message.bytes
            // - LEADER_NOT_AVAILABLE: Broker is still electing a new leader
            // - UNKNOWN_TOPIC_OR_PART: Topic doesn't exist (and auto-creation is off)
            //
            // In production, you'd want to:
            // 1. Log the error with the full message for debugging
            // 2. Retry transient errors (network, leader election)
            // 3. Dead-letter permanent errors (message too large, auth)
            // See Advanced/ErrorHandlingConsumer.cs for retry patterns.
            // -------------------------------------------------------------------
            Console.WriteLine($"❌ Failed to deliver message: {ex.Error.Reason}");
            Console.WriteLine($"   Error Code: {ex.Error.Code}");
            throw;
        }
    }

    /// <summary>
    /// Sends multiple messages using the fire-and-forget pattern with a delivery callback.
    ///
    /// WHEN TO USE THIS PATTERN:
    /// When you need maximum throughput and can handle delivery confirmations
    /// asynchronously via a callback. The producer batches messages internally
    /// and sends them in bulk, which is much more efficient than awaiting each one.
    ///
    /// THROUGHPUT COMPARISON:
    /// - ProduceAsync (one-by-one): ~5,000-10,000 msg/sec
    /// - Produce with callback (batched): ~100,000-500,000 msg/sec
    ///
    /// The tradeoff: you lose the simple sequential flow of async/await.
    /// </summary>
    /// <param name="messages">Collection of messages to send in bulk.</param>
    public async Task SendMultipleMessagesAsync(IEnumerable<string> messages)
    {
        using var producer = new ProducerBuilder<Null, string>(_config).Build();

        var messageList = messages.ToList();
        var deliveredCount = 0;
        var failedCount = 0;

        Console.WriteLine($"\n📤 Sending {messageList.Count} messages to '{_topicName}'...");

        foreach (var message in messageList)
        {
            // -------------------------------------------------------------------
            // Produce (non-awaited / fire-and-forget):
            //   Enqueues the message in the producer's internal buffer.
            //   Kafka's internal sender thread handles the actual network call.
            //   The deliveryHandler callback is invoked when Kafka responds.
            //
            //   INTERNAL BATCHING:
            //   Kafka's producer buffers messages and sends them in batches
            //   (configurable via batch.size and linger.ms). This dramatically
            //   improves throughput compared to sending one-by-one.
            // -------------------------------------------------------------------
            producer.Produce(
                _topicName,
                new Message<Null, string> { Value = message },
                deliveryReport =>
                {
                    // This callback runs on a background thread!
                    // Don't do slow operations here - just log and move on.
                    if (deliveryReport.Error.Code == ErrorCode.NoError)
                    {
                        Interlocked.Increment(ref deliveredCount);
                    }
                    else
                    {
                        Interlocked.Increment(ref failedCount);
                        Console.WriteLine($"⚠️  Delivery failed: {deliveryReport.Error.Reason}");
                    }
                });
        }

        // -----------------------------------------------------------------------
        // FLUSH IS CRITICAL!
        // After fire-and-forget produces, always call Flush() before exiting.
        // Flush() blocks until all buffered messages have been sent and
        // delivery reports received.
        //
        // WITHOUT FLUSH: Messages still in the buffer are LOST when the
        // producer is disposed! This is the #1 beginner mistake with Kafka.
        //
        // The TimeSpan parameter is the maximum time to wait.
        // In production, handle the case where Flush times out gracefully.
        // -----------------------------------------------------------------------
        producer.Flush(TimeSpan.FromSeconds(30));

        Console.WriteLine($"✅ Batch complete: {deliveredCount} delivered, {failedCount} failed");

        await Task.CompletedTask; // Satisfy async signature
    }

    /// <summary>
    /// Demonstrates synchronous message sending - useful in non-async contexts.
    ///
    /// NOTE: Prefer async patterns in modern .NET applications.
    /// Synchronous sending blocks the calling thread until Kafka responds,
    /// which wastes resources in high-throughput scenarios.
    ///
    /// WHEN SYNC IS OK:
    /// - CLI tools and scripts where simplicity beats performance
    /// - Test code verifying producer behavior
    /// - Low-volume administrative operations
    /// </summary>
    public void SendMessageSync(string message)
    {
        using var producer = new ProducerBuilder<Null, string>(_config).Build();

        // GetAwaiter().GetResult() synchronously waits for the async operation.
        // CAUTION: This can cause deadlocks in UI applications (WinForms, WPF)
        // or ASP.NET contexts. Only use in console apps or background services.
        var result = producer.ProduceAsync(_topicName, new Message<Null, string>
        {
            Value = message
        }).GetAwaiter().GetResult();

        Console.WriteLine($"✅ [Sync] Message sent to {result.Topic}:{result.Partition}@{result.Offset}");
    }

    /// <summary>
    /// Entry point demonstrating all producer patterns.
    /// </summary>
    public static async Task RunDemoAsync(string bootstrapServers)
    {
        const string topicName = "beginner-demo";
        var producer = new SimpleProducer(bootstrapServers, topicName);

        Console.WriteLine("=== BEGINNER: Simple Producer Demo ===");
        Console.WriteLine($"Topic: {topicName}");
        Console.WriteLine("This demo shows how to send messages to Kafka.\n");

        // Demo 1: Send a single message asynchronously
        Console.WriteLine("--- Demo 1: Single Async Message ---");
        await producer.SendMessageAsync("Hello, Kafka! This is my first message.");

        // Demo 2: Send multiple messages with batching
        Console.WriteLine("\n--- Demo 2: Batch Messages (Fire-and-Forget) ---");
        var batch = Enumerable.Range(1, 5)
            .Select(i => $"Batch message #{i} - {DateTime.UtcNow:HH:mm:ss.fff}");
        await producer.SendMultipleMessagesAsync(batch);

        // Demo 3: Synchronous sending
        Console.WriteLine("\n--- Demo 3: Synchronous Send ---");
        producer.SendMessageSync("Sync message - good for scripts and tests");

        Console.WriteLine("\n✅ Simple Producer demo complete!");
        Console.WriteLine("💡 Now run the SimpleConsumer to read these messages.");
    }
}
