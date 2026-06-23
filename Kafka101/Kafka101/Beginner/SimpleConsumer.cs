// =============================================================================
// SimpleConsumer.cs - BEGINNER LEVEL: Your first Kafka Consumer
// =============================================================================
//
// WHAT IS A KAFKA CONSUMER?
//   A consumer reads (SUBSCRIBES to) messages from Kafka topics.
//   Unlike traditional queues (RabbitMQ, SQS), Kafka messages are NOT deleted
//   after being read! They persist for a configurable retention period
//   (default: 7 days). This means:
//   - Multiple consumers can read the same messages independently
//   - You can replay events by resetting your offset
//   - New consumers can process historical data
//
// CORE CONCEPTS COVERED:
//   1. ConsumerConfig - How to configure a consumer
//   2. GroupId - Consumer group identity (critical concept!)
//   3. Subscribe vs Assign - Two ways to consume partitions
//   4. Poll loop - How consumers continuously check for new messages
//   5. Offsets - Tracking which messages have been processed
//   6. AutoOffsetReset - What to do when there's no saved offset
//
// THE POLL LOOP (MOST IMPORTANT CONCEPT):
//   Kafka consumers are "pull-based" - they actively request messages from
//   the broker rather than messages being pushed to them. This gives consumers
//   control over their own pace (backpressure handling).
//
//   while (true) {
//     var msg = consumer.Consume(timeout);  ← Pull next message
//     processMessage(msg);                  ← Handle it
//     consumer.Commit();                    ← Mark as processed
//   }

using Confluent.Kafka;

namespace Kafka101.Beginner;

/// <summary>
/// Demonstrates basic Kafka message consumption.
/// Read this after understanding SimpleProducer!
/// </summary>
public class SimpleConsumer
{
    private readonly ConsumerConfig _config;
    private readonly string _topicName;

    /// <summary>
    /// Initializes the consumer with connection and identity settings.
    /// </summary>
    /// <param name="bootstrapServers">Kafka broker address(es).</param>
    /// <param name="topicName">Topic to subscribe to.</param>
    /// <param name="groupId">
    /// Consumer group identifier. This is the most important consumer setting!
    /// See comments below for full explanation.
    /// </param>
    public SimpleConsumer(string bootstrapServers, string topicName, string groupId = "beginner-consumer-group")
    {
        _topicName = topicName;

        // -----------------------------------------------------------------------
        // ConsumerConfig EXPLAINED:
        //
        // GroupId (CRITICAL):
        //   Every consumer MUST belong to a group. The group ID determines:
        //   1. LOAD BALANCING: Kafka distributes partitions among consumers
        //      in the same group. If your topic has 6 partitions and you run
        //      3 consumers with the same GroupId, each gets 2 partitions.
        //
        //   2. OFFSET TRACKING: Kafka tracks how far each GROUP has read.
        //      If your consumer crashes and restarts, it picks up where
        //      it left off (not from the beginning) - per group!
        //
        //   3. ISOLATION: Two consumer apps with DIFFERENT GroupIds each
        //      receive ALL messages independently (like separate subscribers
        //      to a newsletter). This enables fan-out patterns.
        //
        //   Visual example:
        //   Topic "orders" with 3 partitions:
        //   Group "shipping-service":  [P0→Consumer1] [P1→Consumer2] [P2→Consumer3]
        //   Group "billing-service":   [P0→ConsumerA] [P1→ConsumerA] [P2→ConsumerB]
        //   (billing-service gets ALL messages but with only 2 consumers)
        //
        // AutoOffsetReset:
        //   When a consumer group has NO saved offset (first run, or offset expired):
        //   - AutoOffsetReset.Earliest: Start from the OLDEST message in the topic
        //     Use for: First-time consumer that needs to process all history
        //   - AutoOffsetReset.Latest: Start from NEW messages only (ignore history)
        //     Use for: Real-time monitoring that only cares about current data
        //   - AutoOffsetReset.Error: Throw an exception if no offset found
        //
        // EnableAutoCommit:
        //   - true (DEFAULT): Kafka automatically commits offsets every 5 seconds
        //     Simple but risky: you might commit BEFORE processing completes!
        //     This can cause message loss if your app crashes after commit
        //     but before finishing processing.
        //   - false (RECOMMENDED for reliability): You manually commit offsets
        //     after SUCCESSFULLY processing each message. More code, but safe.
        //     See Intermediate/OffsetManagement.cs for the manual commit pattern.
        // -----------------------------------------------------------------------
        _config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,

            // Consumer group identity - shared across all instances of your service
            GroupId = groupId,

            // Start from the beginning when first subscribing
            AutoOffsetReset = AutoOffsetReset.Earliest,

            // For beginners, auto-commit is fine. We'll disable it in Intermediate level.
            EnableAutoCommit = true,

            // How often to auto-commit offsets (default: 5000ms)
            AutoCommitIntervalMs = 5000,

            // Identify your consumer in logs and monitoring dashboards
            ClientId = "kafka101-simple-consumer"
        };
    }

    /// <summary>
    /// Consumes messages from Kafka until the cancellation token is cancelled.
    ///
    /// WHY CANCELLATION TOKEN?
    /// The consumer runs in an infinite loop (the "poll loop"). To stop it
    /// gracefully, we use CancellationToken. In a real app, you'd cancel this
    /// when the application is shutting down (SIGTERM, Ctrl+C).
    ///
    /// GRACEFUL SHUTDOWN IS IMPORTANT:
    /// Simply killing the process without closing the consumer leaves it in
    /// the consumer group until the session timeout (default: 10 seconds).
    /// During this time, its partitions are "orphaned" - not being consumed.
    /// Proper Close() calls trigger immediate partition rebalancing.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token to signal when to stop consuming.
    /// Use CancellationTokenSource and cancel it to stop the consumer.
    /// </param>
    /// <param name="maxMessages">
    /// Maximum number of messages to consume (for demo purposes).
    /// In production, you'd consume indefinitely until shutdown.
    /// </param>
    public void ConsumeMessages(CancellationToken cancellationToken, int maxMessages = int.MaxValue)
    {
        // -----------------------------------------------------------------------
        // IConsumer<TKey, TValue>:
        //   Like the producer, the consumer is generic.
        //   <Null, string> matches what SimpleProducer sends (no key, string value).
        //
        // ConsumerBuilder:
        //   Builds the consumer with your config. In advanced scenarios, you can
        //   add custom deserializers here:
        //   .SetValueDeserializer(new MyCustomDeserializer())
        // -----------------------------------------------------------------------
        using var consumer = new ConsumerBuilder<Null, string>(_config).Build();

        try
        {
            // -------------------------------------------------------------------
            // Subscribe vs Assign:
            //
            // Subscribe (used here):
            //   - Tells Kafka "I want messages from this topic"
            //   - Kafka automatically assigns partitions to your consumer
            //   - When other consumers join/leave the group, Kafka REBALANCES
            //     (redistributes partitions). Your consumer gets a callback.
            //   - Recommended for most use cases.
            //
            // Assign (manual partition assignment):
            //   - You explicitly specify which partition(s) to read
            //   - Example: consumer.Assign(new TopicPartition("topic", 0));
            //   - No automatic rebalancing - you control which partition you read
            //   - Used for: Replay scenarios, exactly-once architectures,
            //     when you need deterministic partition assignment
            // -------------------------------------------------------------------
            consumer.Subscribe(_topicName);
            Console.WriteLine($"✅ Subscribed to topic: {_topicName}");
            Console.WriteLine($"   Group ID: {_config.GroupId}");
            Console.WriteLine($"   Waiting for messages... (Ctrl+C to stop)\n");

            var consumedCount = 0;

            // THE POLL LOOP - the heart of every Kafka consumer
            while (!cancellationToken.IsCancellationRequested && consumedCount < maxMessages)
            {
                try
                {
                    // ---------------------------------------------------------------
                    // Consume(timeout) - Pull next message:
                    //   The timeout specifies how long to WAIT if no messages are
                    //   immediately available. It does NOT mean "stop after X seconds".
                    //
                    //   If no message arrives within the timeout, returns null
                    //   (or throws OperationCanceledException if token is cancelled).
                    //
                    //   TIMEOUT CHOICE:
                    //   - Short timeout (100ms): More responsive to cancellation
                    //     but slightly more CPU usage from frequent polling
                    //   - Long timeout (5000ms): Less CPU but slower shutdown response
                    //   - Typically 1000ms is a good balance
                    //
                    //   HEARTBEATING:
                    //   Even when no messages arrive, Consume() sends heartbeats
                    //   to the broker to prove the consumer is alive. If you stop
                    //   calling Consume(), the broker thinks you're dead and
                    //   triggers a rebalance! Never block the poll loop for long.
                    // ---------------------------------------------------------------
                    var consumeResult = consumer.Consume(cancellationToken);

                    // If no message and timeout expired, consumeResult.IsPartitionEOF is true
                    if (consumeResult.IsPartitionEOF)
                    {
                        Console.WriteLine($"   ⏸ Reached end of partition {consumeResult.Partition}. Waiting...");
                        continue;
                    }

                    // Process the message
                    consumedCount++;
                    Console.WriteLine($"📨 Message #{consumedCount} received:");
                    Console.WriteLine($"   Value:     {consumeResult.Message.Value}");
                    Console.WriteLine($"   Topic:     {consumeResult.Topic}");
                    Console.WriteLine($"   Partition: {consumeResult.Partition}");
                    Console.WriteLine($"   Offset:    {consumeResult.Offset}");
                    Console.WriteLine($"   Timestamp: {consumeResult.Message.Timestamp.UtcDateTime:HH:mm:ss.fff} UTC");

                    // -----------------------------------------------------------
                    // AUTO-COMMIT vs MANUAL COMMIT:
                    // With EnableAutoCommit=true (our current config), Kafka
                    // automatically records progress every 5 seconds.
                    //
                    // This means: if you process 1000 messages but the auto-commit
                    // hasn't fired yet, and your app crashes, you'll re-process
                    // those 1000 messages on restart. That's "at-least-once" delivery.
                    //
                    // For EXACTLY-ONCE delivery, see Intermediate/OffsetManagement.cs
                    // where we commit manually after each successful processing.
                    // -----------------------------------------------------------
                }
                catch (ConsumeException ex)
                {
                    // -----------------------------------------------------------
                    // ConsumeException: Non-fatal consumer errors.
                    // In most cases, you should log and continue the loop.
                    //
                    // Fatal errors (ex.Error.IsFatal) require recreating the consumer.
                    // -----------------------------------------------------------
                    Console.WriteLine($"⚠️  Consume error: {ex.Error.Reason}");
                    if (ex.Error.IsFatal)
                    {
                        Console.WriteLine("💀 Fatal error - consumer cannot continue!");
                        break;
                    }
                    // Continue the loop for non-fatal errors
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown - cancellation token was triggered
            Console.WriteLine("\n🛑 Shutdown signal received.");
        }
        finally
        {
            // -------------------------------------------------------------------
            // CLOSE IS MANDATORY (not just Dispose):
            // consumer.Close() performs a GRACEFUL shutdown:
            // 1. Commits any pending auto-commit offsets
            // 2. Sends "LeaveGroup" request to the broker
            // 3. Triggers immediate rebalancing so other consumers
            //    can pick up the partitions right away
            //
            // Without Close(), the group waits for session.timeout.ms (10 sec)
            // before realizing the consumer is gone.
            //
            // The using statement calls Dispose() but NOT Close()!
            // Always call Close() explicitly before Dispose().
            // -------------------------------------------------------------------
            consumer.Close();
            Console.WriteLine("✅ Consumer closed gracefully.");
        }
    }

    /// <summary>
    /// Entry point demonstrating basic consumption.
    /// </summary>
    public static void RunDemo(string bootstrapServers)
    {
        const string topicName = "beginner-demo";
        var consumer = new SimpleConsumer(bootstrapServers, topicName);

        Console.WriteLine("=== BEGINNER: Simple Consumer Demo ===");
        Console.WriteLine($"Topic: {topicName}");
        Console.WriteLine("Reading messages produced by SimpleProducer...\n");

        // CancellationTokenSource allows external cancellation
        using var cts = new CancellationTokenSource();

        // Register Ctrl+C handler for graceful shutdown
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true; // Prevent immediate process kill
            cts.Cancel();    // Signal our consumer to stop
            Console.WriteLine("\n🛑 Ctrl+C pressed - initiating graceful shutdown...");
        };

        // Consume up to 10 messages (or until Ctrl+C)
        consumer.ConsumeMessages(cts.Token, maxMessages: 10);

        Console.WriteLine("\n✅ Simple Consumer demo complete!");
    }
}
