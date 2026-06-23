// =============================================================================
// ConsumerGroupDemo.cs - INTERMEDIATE LEVEL: Consumer Groups & Load Balancing
// =============================================================================
//
// WHAT IS A CONSUMER GROUP?
//   A consumer group is a set of consumers that COOPERATE to consume a topic.
//   Kafka distributes the topic's partitions among the consumers in the group,
//   so each partition is consumed by exactly ONE consumer at a time.
//
//   This provides:
//   1. HORIZONTAL SCALING: Add more consumers to increase throughput
//   2. FAULT TOLERANCE: If a consumer dies, its partitions are redistributed
//   3. LOAD BALANCING: Work is automatically balanced across consumers
//
// THE GOLDEN RULE:
//   Number of consumers that can work in parallel = Number of partitions
//   - 6 partitions + 3 consumers = 2 partitions per consumer
//   - 6 partitions + 6 consumers = 1 partition per consumer (max parallelism)
//   - 6 partitions + 8 consumers = 6 active, 2 IDLE (extra consumers are standby)
//   - Having MORE consumers than partitions wastes resources but provides failover
//
// REBALANCING:
//   When consumers join or leave the group, Kafka REBALANCES the partition
//   assignment. During rebalance, consumption PAUSES briefly (the "stop the world"
//   moment). Modern Kafka supports "cooperative incremental rebalancing" which
//   minimizes this pause.
//
// VISUALIZING CONSUMER GROUPS:
//
//   Topic "orders" (6 partitions) consumed by "shipping-service" group:
//
//   SCENARIO A: 2 consumers
//   Consumer-1: reads Partition 0, 1, 2
//   Consumer-2: reads Partition 3, 4, 5
//
//   SCENARIO B: 3 consumers (rebalance happened!)
//   Consumer-1: reads Partition 0, 1
//   Consumer-2: reads Partition 2, 3
//   Consumer-3: reads Partition 4, 5
//
//   SCENARIO C: Consumer-2 crashes (rebalance!)
//   Consumer-1: reads Partition 0, 1, 2, 3
//   Consumer-3: reads Partition 4, 5
//
// PARTITION ASSIGNMENT STRATEGIES:
//   - Range (default): Assigns consecutive partitions to each consumer
//   - RoundRobin: Alternates partitions across consumers
//   - Sticky: Minimizes partition movement during rebalance
//   - CooperativeSticky: Like Sticky but without full-stop rebalancing

using Confluent.Kafka;

namespace Kafka101.Intermediate;

/// <summary>
/// Demonstrates consumer group behavior and load balancing.
/// Shows how multiple consumers cooperate to process a topic.
/// </summary>
public class ConsumerGroupDemo
{
    private readonly string _bootstrapServers;
    private readonly string _topicName;
    private readonly string _groupId;

    public ConsumerGroupDemo(string bootstrapServers, string topicName = "intermediate-orders",
        string groupId = "intermediate-consumer-group")
    {
        _bootstrapServers = bootstrapServers;
        _topicName = topicName;
        _groupId = groupId;
    }

    /// <summary>
    /// Creates a configured consumer for this group.
    /// Extracted to a factory method so we can create multiple instances easily.
    /// </summary>
    private IConsumer<string, string> CreateConsumer(string consumerId)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _bootstrapServers,
            GroupId = _groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,

            // DISABLE auto-commit for explicit control (recommended for production)
            EnableAutoCommit = false,

            // Session timeout: If broker doesn't receive heartbeat within this time,
            // it considers the consumer dead and triggers rebalance.
            // Default: 45000ms. Reduce for faster failure detection.
            SessionTimeoutMs = 30000,

            // Max time between polls before the consumer is considered dead.
            // If your processing takes longer than this, increase it!
            // Or better: move slow processing to a separate thread.
            MaxPollIntervalMs = 300000, // 5 minutes

            // PARTITION ASSIGNMENT STRATEGY:
            // CooperativeSticky minimizes partition movement during rebalance.
            // Partitions not being reassigned continue flowing during rebalance!
            PartitionAssignmentStrategy = PartitionAssignmentStrategy.CooperativeSticky,

            ClientId = $"kafka101-consumer-{consumerId}"
        };

        var consumer = new ConsumerBuilder<string, string>(config)
            // -----------------------------------------------------------------------
            // REBALANCE CALLBACKS:
            // These let you react to partition assignment changes.
            //
            // OnPartitionsAssigned: Called when partitions are given TO this consumer
            //   Use case: Initialize state for these partitions (e.g., load cache)
            //   You CAN override the offset here:
            //   context.SyncOverride(new[] { new TopicPartitionOffset(tp, Offset.Beginning) });
            //
            // OnPartitionsRevoked: Called when partitions are taken FROM this consumer
            //   Use case: Commit offsets, flush caches, clean up state
            //   CRITICAL: Commit BEFORE returning from this callback!
            //   If you don't commit here, you risk reprocessing messages.
            //
            // OnPartitionsLost: Called on abrupt partition loss (crash scenarios)
            //   Different from Revoked - you can't commit here (partition is gone)
            //   Use case: Clean up state without committing
            // -----------------------------------------------------------------------
            .SetPartitionsAssignedHandler((c, partitions) =>
            {
                Console.WriteLine($"\n🔄 [{consumerId}] Partitions ASSIGNED: " +
                    string.Join(", ", partitions.Select(p => $"P{p.Partition.Value}")));
                Console.WriteLine($"   Group: {_groupId}");
            })
            .SetPartitionsRevokedHandler((c, partitions) =>
            {
                Console.WriteLine($"\n🔄 [{consumerId}] Partitions REVOKED: " +
                    string.Join(", ", partitions.Select(p => $"P{p.Partition.Value}@{p.Offset}")));

                // COMMIT BEFORE REBALANCE:
                // This ensures no messages are reprocessed when partitions
                // are assigned to another consumer.
                try
                {
                    c.Commit(partitions);
                    Console.WriteLine($"   ✅ [{consumerId}] Committed offsets before revocation");
                }
                catch (KafkaException ex)
                {
                    Console.WriteLine($"   ⚠️  [{consumerId}] Could not commit: {ex.Message}");
                }
            })
            .SetPartitionsLostHandler((c, partitions) =>
            {
                Console.WriteLine($"\n💀 [{consumerId}] Partitions LOST (consumer was considered dead): " +
                    string.Join(", ", partitions.Select(p => $"P{p.Partition.Value}")));
                // Don't try to commit here - these partitions may already be owned by another consumer
            })
            .Build();

        return consumer;
    }

    /// <summary>
    /// Runs a single consumer instance within the group.
    /// In production, you'd run multiple instances across different machines.
    ///
    /// SIMULATING MULTIPLE CONSUMERS IN ONE PROCESS:
    /// This demo runs multiple consumers in separate threads with the same GroupId.
    /// In reality, each consumer would be a separate process or container.
    /// </summary>
    public void RunConsumer(string consumerId, CancellationToken cancellationToken, int maxMessages = 20)
    {
        using var consumer = CreateConsumer(consumerId);
        consumer.Subscribe(_topicName);

        Console.WriteLine($"🚀 [{consumerId}] Starting - Group: {_groupId} | Topic: {_topicName}");

        var messageCount = 0;

        try
        {
            while (!cancellationToken.IsCancellationRequested && messageCount < maxMessages)
            {
                try
                {
                    var result = consumer.Consume(TimeSpan.FromSeconds(2));

                    if (result == null || result.IsPartitionEOF)
                        continue;

                    messageCount++;

                    // Simulate processing time (real apps would do actual work here)
                    var processingTime = new Random().Next(10, 50);
                    Thread.Sleep(processingTime);

                    Console.WriteLine($"📨 [{consumerId}] Message {messageCount}:");
                    Console.WriteLine($"   Partition: {result.Partition} | Offset: {result.Offset}");
                    Console.WriteLine($"   Key: {result.Message.Key}");
                    Console.WriteLine($"   Processing: {processingTime}ms");

                    // -----------------------------------------------------------
                    // MANUAL COMMIT AFTER PROCESSING:
                    // Commit the offset ONLY after successfully processing.
                    // This ensures "at-least-once" delivery - if we crash
                    // before committing, the message is reprocessed on restart.
                    //
                    // StoreOffset vs Commit:
                    // - StoreOffset: Marks locally but doesn't send to Kafka yet
                    //   (used with EnableAutoOffsetStore=false for fine control)
                    // - Commit: Immediately sends offset to Kafka broker
                    //   (synchronous, adds latency - consider CommitAsync)
                    // -----------------------------------------------------------
                    consumer.Commit(result);
                }
                catch (ConsumeException ex) when (!ex.Error.IsFatal)
                {
                    Console.WriteLine($"⚠️  [{consumerId}] Non-fatal error: {ex.Error.Reason}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"🛑 [{consumerId}] Shutdown initiated");
        }
        finally
        {
            consumer.Close();
            Console.WriteLine($"✅ [{consumerId}] Closed gracefully. Total messages: {messageCount}");
        }
    }

    /// <summary>
    /// Runs multiple consumers concurrently to demonstrate load balancing.
    ///
    /// IMPORTANT NOTE ON THREAD SAFETY:
    /// IConsumer is NOT thread-safe. Each consumer must run on its own thread.
    /// Never share a consumer instance across threads.
    /// </summary>
    public static async Task RunDemoAsync(string bootstrapServers)
    {
        var demo = new ConsumerGroupDemo(bootstrapServers);

        Console.WriteLine("=== INTERMEDIATE: Consumer Group Demo ===");
        Console.WriteLine("Simulating 3 consumers in the same group reading the same topic.");
        Console.WriteLine("Watch how partitions are distributed and messages load-balanced.\n");

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

        // Start 3 consumers in parallel threads (each with its own IConsumer instance)
        var tasks = new[]
        {
            Task.Run(() => demo.RunConsumer("Consumer-1", cts.Token, maxMessages: 10)),
            Task.Run(() => demo.RunConsumer("Consumer-2", cts.Token, maxMessages: 10)),
            Task.Run(() => demo.RunConsumer("Consumer-3", cts.Token, maxMessages: 10))
        };

        // Wait for all consumers to finish or cancellation
        await Task.WhenAll(tasks);

        Console.WriteLine("\n✅ Consumer Group demo complete!");
        Console.WriteLine("💡 Each partition was consumed by exactly ONE consumer.");
        Console.WriteLine("   If you run with fewer consumers, each handles more partitions.");
    }
}
