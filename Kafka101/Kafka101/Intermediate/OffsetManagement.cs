// =============================================================================
// OffsetManagement.cs - INTERMEDIATE LEVEL: Manual Offset Control
// =============================================================================
//
// WHAT IS AN OFFSET?
//   An offset is a sequential ID assigned to each message within a partition.
//   It's like a line number in a file - offset 0 is the first message,
//   offset 1 is the second, etc.
//
//   Topic "orders", Partition 0:
//   ┌────────────────────────────────────────────────────────┐
//   │ [Offset:0] [Offset:1] [Offset:2] [Offset:3] [Offset:4]│
//   │  "ORD-1"   "ORD-2"   "ORD-3"   "ORD-4"   "ORD-5"    │
//   └────────────────────────────────────────────────────────┘
//         ↑
//   Consumer committed at offset 2, so next Consume() returns offset 3
//
// WHY MANUAL OFFSET MANAGEMENT?
//   Kafka stores the "committed offset" for each (consumer group, topic, partition).
//   This tells Kafka: "Group X has successfully processed up to offset N."
//
//   On restart, the consumer resumes from (committed offset + 1).
//
//   With AUTO-COMMIT (interval-based):
//   - Kafka commits every N ms regardless of processing state
//   - Risk: Commit happens BEFORE processing completes → message loss
//   - Risk: Crash AFTER processing but BEFORE auto-commit → duplicate processing
//
//   With MANUAL COMMIT (controlled):
//   - You commit ONLY after successful processing
//   - Guarantees: If committed = definitely processed
//   - This is "at-least-once" delivery semantics
//
// DELIVERY SEMANTICS EXPLAINED:
//   1. AT-MOST-ONCE:  Commit BEFORE processing
//                     Messages might be lost if crash during processing
//                     Use when: loss is acceptable (metrics, logs)
//
//   2. AT-LEAST-ONCE: Commit AFTER processing (this file demonstrates this)
//                     Messages might be duplicated if crash after processing
//                     but before commit. Make your processing idempotent!
//                     Use when: duplicates are OK or can be deduplicated
//
//   3. EXACTLY-ONCE:  Requires Kafka transactions + idempotent consumers
//                     Most complex, most correct
//                     Use when: financial transactions, critical state changes
//                     See Advanced/TransactionalProducer.cs for this pattern

using Confluent.Kafka;

namespace Kafka101.Intermediate;

/// <summary>
/// Demonstrates manual offset management patterns for reliable message processing.
/// This is one of the most important topics for production Kafka applications.
/// </summary>
public class OffsetManagement
{
    private readonly ConsumerConfig _config;
    private readonly string _topicName;

    public OffsetManagement(string bootstrapServers, string topicName = "intermediate-orders")
    {
        _topicName = topicName;

        _config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = "offset-management-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,

            // DISABLE auto-commit - we'll commit manually at strategic points
            EnableAutoCommit = false,

            // Also disable auto-store so we control EXACTLY when offsets are stored
            // (prevents Commit() from committing unprocessed messages)
            EnableAutoOffsetStore = false,

            ClientId = "kafka101-offset-manager"
        };
    }

    /// <summary>
    /// AT-LEAST-ONCE delivery pattern:
    /// Process first, then commit. If crash before commit, message replays.
    ///
    /// YOUR PROCESSING CODE MUST BE IDEMPOTENT!
    /// Idempotent = "safe to run multiple times with the same result"
    /// Examples:
    /// - "INSERT OR UPDATE" (upsert) instead of plain INSERT
    /// - Track processed message IDs in your database and skip duplicates
    /// - Use natural deduplication (e.g., setting a value instead of incrementing)
    /// </summary>
    public void ConsumeAtLeastOnce(CancellationToken cancellationToken, int maxMessages = 10)
    {
        using var consumer = new ConsumerBuilder<string, string>(_config).Build();
        consumer.Subscribe(_topicName);

        Console.WriteLine("📋 AT-LEAST-ONCE delivery mode");
        Console.WriteLine("   Commit AFTER processing. Safe but may duplicate on failure.\n");

        var processed = 0;

        try
        {
            while (!cancellationToken.IsCancellationRequested && processed < maxMessages)
            {
                var result = consumer.Consume(TimeSpan.FromSeconds(3));
                if (result == null || result.IsPartitionEOF) continue;

                try
                {
                    // STEP 1: PROCESS the message (your business logic here)
                    Console.WriteLine($"⚙️  Processing: {result.Message.Key} @ offset {result.Offset}");
                    ProcessMessage(result.Message.Value); // Your processing logic
                    processed++;

                    // STEP 2: ONLY COMMIT AFTER SUCCESSFUL PROCESSING
                    // If ProcessMessage() throws, we DON'T commit.
                    // On restart, we'll re-process this message.
                    // This is why your processing must be idempotent!

                    // StoreOffset marks the offset for commit without sending to broker yet
                    // The actual send happens on the next Commit() call
                    consumer.StoreOffset(result);
                    Console.WriteLine($"✅ Processed and stored offset {result.Offset}");

                    // Commit every 100 messages for efficiency (batch commit)
                    // More frequent commits = more overhead but faster recovery after crash
                    // Less frequent commits = more potential reprocessing on crash
                    if (processed % 10 == 0)
                    {
                        consumer.Commit();
                        Console.WriteLine($"📌 Committed batch at offset {result.Offset}");
                    }
                }
                catch (Exception ex)
                {
                    // Processing FAILED - DO NOT commit this offset
                    // The message will be reprocessed on next poll or restart
                    Console.WriteLine($"❌ Processing failed for offset {result.Offset}: {ex.Message}");
                    Console.WriteLine("   Not committing - will retry this message");

                    // In production, implement retry logic here:
                    // - Retry immediately (for transient errors: network, DB connection)
                    // - Send to Dead Letter Queue (for permanent errors: bad data)
                    // See Advanced/ErrorHandlingConsumer.cs for retry patterns
                }
            }

            // Final commit for any uncommitted offsets
            consumer.Commit();
            Console.WriteLine($"\n✅ Final commit. Processed {processed} messages.");
        }
        finally
        {
            consumer.Close();
        }
    }

    /// <summary>
    /// AT-MOST-ONCE delivery pattern:
    /// Commit BEFORE processing. Fastest but messages can be lost.
    ///
    /// WHEN TO USE AT-MOST-ONCE:
    /// - High-volume metrics/logging where occasional loss is acceptable
    /// - Real-time dashboards where stale data is preferable to delay
    /// - Events where the cost of reprocessing exceeds the cost of occasional loss
    /// - IoT sensor data where the next reading makes the lost one irrelevant
    /// </summary>
    public void ConsumeAtMostOnce(CancellationToken cancellationToken, int maxMessages = 10)
    {
        using var consumer = new ConsumerBuilder<string, string>(_config).Build();
        consumer.Subscribe(_topicName);

        Console.WriteLine("📋 AT-MOST-ONCE delivery mode");
        Console.WriteLine("   Commit BEFORE processing. Fast but may lose messages.\n");

        var processed = 0;

        try
        {
            while (!cancellationToken.IsCancellationRequested && processed < maxMessages)
            {
                var result = consumer.Consume(TimeSpan.FromSeconds(3));
                if (result == null || result.IsPartitionEOF) continue;

                // STEP 1: COMMIT FIRST (mark as "seen" even before processing)
                consumer.StoreOffset(result);
                consumer.Commit();
                Console.WriteLine($"📌 Committed offset {result.Offset} BEFORE processing");

                try
                {
                    // STEP 2: PROCESS (if this crashes, message is already committed = LOST)
                    ProcessMessage(result.Message.Value);
                    processed++;
                    Console.WriteLine($"✅ Processed: {result.Message.Key}");
                }
                catch (Exception ex)
                {
                    // Message was already committed! It won't be reprocessed.
                    // This is the risk of at-most-once.
                    Console.WriteLine($"❌ Processing failed (MESSAGE LOST): {ex.Message}");
                }
            }
        }
        finally
        {
            consumer.Close();
        }
    }

    /// <summary>
    /// Demonstrates SEEKING to a specific offset position.
    ///
    /// COMMON SEEK USE CASES:
    /// 1. REPLAY: "Re-process the last hour of data"
    ///    consumer.Seek(new TopicPartitionOffset(tp, earlierOffset));
    ///
    /// 2. SKIP: "Skip past a poison message we can't process"
    ///    consumer.Seek(new TopicPartitionOffset(tp, result.Offset + 1));
    ///
    /// 3. RESET TO BEGINNING: "Process all historical data"
    ///    consumer.Seek(new TopicPartitionOffset(tp, Offset.Beginning));
    ///
    /// 4. RESET TO END: "Only process new messages from now"
    ///    consumer.Seek(new TopicPartitionOffset(tp, Offset.End));
    ///
    /// 5. SEEK BY TIMESTAMP: "Process all messages since midnight"
    ///    var offsets = consumer.OffsetsForTimes(...);
    ///    consumer.Assign(offsets);
    /// </summary>
    public void DemonstrateSeek(CancellationToken cancellationToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _config.BootstrapServers,
            GroupId = "seek-demo-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();

        // Use Assign instead of Subscribe when you need manual partition control
        // (required for Seek operations)
        var partition = new TopicPartition(_topicName, new Partition(0));
        consumer.Assign(partition);

        // SEEK TO BEGINNING: Start from offset 0
        consumer.Seek(new TopicPartitionOffset(_topicName, 0, Offset.Beginning));
        Console.WriteLine($"\n🔍 Seeked to BEGINNING of {_topicName}, Partition 0");
        Console.WriteLine("   Reading first 3 messages from the start...\n");

        var count = 0;
        while (count < 3 && !cancellationToken.IsCancellationRequested)
        {
            var result = consumer.Consume(TimeSpan.FromSeconds(3));
            if (result == null || result.IsPartitionEOF)
            {
                Console.WriteLine("   (No more messages in partition 0)");
                break;
            }

            Console.WriteLine($"📨 [Replayed] Offset {result.Offset}: {result.Message.Key}");
            count++;
        }

        // Demonstrate seeking to a specific offset
        if (count > 0)
        {
            Console.WriteLine("\n🔍 Seeking to offset 0 again for another replay...");
            consumer.Seek(new TopicPartitionOffset(_topicName, 0, 0)); // Specific offset
        }

        consumer.Close();
    }

    /// <summary>
    /// Simulates business logic processing.
    /// In production this would be your actual domain processing.
    /// </summary>
    private static void ProcessMessage(string messageValue)
    {
        // Simulate occasional processing failure (10% failure rate for demo)
        if (new Random().Next(10) == 0)
            throw new InvalidOperationException("Simulated transient processing error");

        // Simulate processing time
        Thread.Sleep(new Random().Next(5, 20));
    }

    /// <summary>Entry point for offset management demonstrations.</summary>
    public static void RunDemo(string bootstrapServers)
    {
        var demo = new OffsetManagement(bootstrapServers);

        Console.WriteLine("=== INTERMEDIATE: Offset Management Demo ===\n");

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

        Console.WriteLine("--- Demo 1: At-Least-Once Delivery ---");
        demo.ConsumeAtLeastOnce(cts.Token, maxMessages: 5);

        if (!cts.Token.IsCancellationRequested)
        {
            Console.WriteLine("\n--- Demo 2: Seek / Replay ---");
            demo.DemonstrateSeek(cts.Token);
        }

        Console.WriteLine("\n✅ Offset Management demo complete!");
        Console.WriteLine("💡 Key insight: Commit strategy determines your delivery guarantee.");
    }
}
