// =============================================================================
// PartitionedProducer.cs - INTERMEDIATE LEVEL: Partitions and Message Keys
// =============================================================================
//
// WHAT ARE PARTITIONS?
//   A Kafka topic is divided into PARTITIONS - ordered, immutable log segments.
//   Partitions are the fundamental unit of parallelism and scalability in Kafka.
//
//   Topic "orders" with 3 partitions:
//   ┌──────────────────────────────────────────────────────────────────┐
//   │ Partition 0: [offset:0 msg] → [offset:1 msg] → [offset:2 msg] → │
//   │ Partition 1: [offset:0 msg] → [offset:1 msg] → [offset:2 msg] → │
//   │ Partition 2: [offset:0 msg] → [offset:1 msg] → [offset:2 msg] → │
//   └──────────────────────────────────────────────────────────────────┘
//
// WHY PARTITIONS MATTER:
//   1. PARALLELISM: Multiple consumers can read different partitions simultaneously
//      (one consumer per partition max). More partitions = more parallelism.
//
//   2. SCALABILITY: Partitions can be on different brokers, distributing load.
//      A topic with 100 partitions across 10 brokers = 10 partitions each.
//
//   3. ORDERING: Messages within ONE partition are ordered (by offset).
//      Messages ACROSS partitions have NO guaranteed ordering.
//      This is why message keys matter - they pin related messages to
//      the same partition, preserving their relative order.
//
// HOW PARTITION ASSIGNMENT WORKS:
//   1. If key is provided: partition = hash(key) % numPartitions
//      Same key ALWAYS goes to the same partition (deterministic)
//   2. If no key (null): Kafka distributes round-robin across partitions
//      For high throughput without ordering requirements
//   3. Manual assignment: You explicitly specify the partition number
//
// REAL-WORLD USE CASE:
//   For order events, using OrderId as the key ensures:
//   - "OrderCreated", "OrderPaid", "OrderShipped" for order ORD-123
//     all land in partition 2 (based on hash of "ORD-123")
//   - Consumers processing partition 2 see these events IN ORDER
//   - This is critical for state machines that must process events sequentially

using Confluent.Kafka;
using Kafka101.Models;
using Newtonsoft.Json;

namespace Kafka101.Intermediate;

/// <summary>
/// Demonstrates partitioned message production with keys.
/// Covers the most important intermediate Kafka concept: message keys.
/// </summary>
public class PartitionedProducer
{
    private readonly ProducerConfig _config;
    private readonly string _topicName;

    public PartitionedProducer(string bootstrapServers, string topicName = "intermediate-orders")
    {
        _topicName = topicName;

        // -----------------------------------------------------------------------
        // INTERMEDIATE PRODUCER CONFIG:
        // Building on the beginner config with additional performance settings.
        //
        // LingerMs (linger.ms):
        //   Time to WAIT before sending a batch, even if batch isn't full.
        //   - 0ms: Send immediately (lowest latency, small batches)
        //   - 5-50ms: Wait a bit to collect more messages (higher throughput)
        //   Tradeoff: latency vs throughput. In analytics/logging, 5-10ms is common.
        //
        // BatchSize (batch.size):
        //   Maximum bytes to include in a single batch per partition.
        //   Default: 16,384 bytes (16KB). Increase for higher throughput.
        //   Kafka sends when EITHER BatchSize is reached OR LingerMs expires.
        //
        // CompressionType:
        //   Compresses messages before sending. Reduces network and storage.
        //   - None: No compression (fastest CPU, most bandwidth)
        //   - Gzip: Best compression ratio, slower
        //   - Snappy: Good ratio, fast (good default for most use cases)
        //   - Lz4: Very fast, moderate compression (good for latency-sensitive)
        //   - Zstd: Best ratio with good speed (modern choice)
        //
        // EnableIdempotence:
        //   When true, the producer guarantees each message is written EXACTLY ONCE
        //   even if the network causes the producer to retry.
        //
        //   HOW IT WORKS:
        //   Each producer gets a unique ProducerId. Each message gets a sequence
        //   number. The broker deduplicates retried messages based on
        //   (ProducerId, Partition, SequenceNumber).
        //
        //   REQUIREMENT: Acks must be All, and MaxInFlight must be ≤ 5
        //   Automatically sets these if not configured explicitly.
        // -----------------------------------------------------------------------
        _config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            Acks = Acks.All,
            LingerMs = 5,         // Wait up to 5ms to batch messages
            BatchSize = 65536,    // 64KB batch size
            CompressionType = CompressionType.Snappy,

            // IDEMPOTENT PRODUCER - prevents duplicate messages on retry
            EnableIdempotence = true,

            // Max in-flight requests per connection (must be ≤ 5 with idempotence)
            MaxInFlight = 5,

            ClientId = "kafka101-partitioned-producer"
        };
    }

    /// <summary>
    /// Sends an order event using the OrderId as the message key.
    ///
    /// KEY → PARTITION MAPPING:
    /// Kafka uses the Murmur2 hash algorithm:
    ///   partition = abs(murmur2(key)) % numPartitions
    ///
    /// For a topic with 3 partitions:
    /// "ORD-12345" → hash → partition 1 (deterministic!)
    /// "ORD-67890" → hash → partition 0
    /// All events for "ORD-12345" will ALWAYS go to partition 1.
    /// </summary>
    public async Task SendOrderEventAsync(OrderEvent orderEvent)
    {
        // <string, string>: Key=OrderId (for partition routing), Value=JSON payload
        using var producer = new ProducerBuilder<string, string>(_config).Build();

        // Serialize the complex object to JSON for the message value
        // In production, prefer Avro or Protobuf for schema evolution support
        var jsonValue = JsonConvert.SerializeObject(orderEvent);

        var message = new Message<string, string>
        {
            // The KEY determines partition assignment
            // Using OrderId ensures all events for an order go to the same partition
            Key = orderEvent.OrderId,

            Value = jsonValue,

            // HEADERS: Optional metadata for routing, filtering, tracing
            // Consumers can inspect headers without deserializing the full payload
            Headers = new Headers
            {
                { "event-type", System.Text.Encoding.UTF8.GetBytes(orderEvent.EventType) },
                { "schema-version", System.Text.Encoding.UTF8.GetBytes("1.0") },
                { "correlation-id", System.Text.Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()) }
            },

            // Explicit timestamp (defaults to current time if omitted)
            Timestamp = new Timestamp(orderEvent.Timestamp)
        };

        var result = await producer.ProduceAsync(_topicName, message);

        Console.WriteLine($"📦 Order {orderEvent.OrderId} → Partition {result.Partition} @ Offset {result.Offset}");
        Console.WriteLine($"   Key: {result.Message.Key} | Event: {orderEvent.EventType}");
    }

    /// <summary>
    /// Sends a message to a SPECIFIC partition by partition number.
    ///
    /// WHEN TO USE MANUAL PARTITION ASSIGNMENT:
    /// 1. Replay: "Re-process all messages in partition 3 from offset 1000"
    /// 2. Geographic routing: "EU orders always go to partition 0"
    /// 3. Priority lanes: "VIP orders go to partition 0, regular to 1-5"
    /// 4. Testing: Verify partition-specific consumer behavior
    ///
    /// WARNING: Manual assignment bypasses key-based routing.
    /// This means ordering by key is no longer guaranteed.
    /// </summary>
    public async Task SendToSpecificPartitionAsync(string key, string value, int partitionNumber)
    {
        using var producer = new ProducerBuilder<string, string>(_config).Build();

        // TopicPartition explicitly specifies where to send the message
        var topicPartition = new TopicPartition(_topicName, new Partition(partitionNumber));

        var result = await producer.ProduceAsync(
            topicPartition,  // Explicit partition, ignoring key-based routing
            new Message<string, string> { Key = key, Value = value });

        Console.WriteLine($"📌 Message forced to partition {partitionNumber} @ offset {result.Offset}");
    }

    /// <summary>
    /// Demonstrates the ordering guarantee of keyed messages.
    ///
    /// WHAT THIS PROVES:
    /// All events for the same OrderId will be in the SAME partition,
    /// in the order they were sent. A consumer reading that partition
    /// will see them in the correct sequence.
    ///
    /// WITHOUT KEYS (round-robin):
    /// Event 1 → Partition 0
    /// Event 2 → Partition 1
    /// Event 3 → Partition 2
    /// Event 4 → Partition 0
    /// Consumer might process Event 4 before Event 2 → WRONG ORDER!
    ///
    /// WITH KEYS (same key = same partition):
    /// Event 1 (key="ORD-123") → Partition 1
    /// Event 2 (key="ORD-123") → Partition 1
    /// Event 3 (key="ORD-123") → Partition 1
    /// Consumer reads partition 1 → sees events in order ✓
    /// </summary>
    public async Task DemonstrateOrderingAsync()
    {
        using var producer = new ProducerBuilder<string, string>(_config).Build();

        var orderId = "ORD-DEMO-001";
        var eventTypes = new[] { "OrderCreated", "OrderPaid", "OrderShipped", "OrderDelivered" };

        Console.WriteLine($"\n📋 Sending lifecycle events for order {orderId}:");
        Console.WriteLine("   (All should land in the SAME partition)");

        foreach (var eventType in eventTypes)
        {
            var orderEvent = OrderEventFactory.Create(orderId, eventType);
            var json = JsonConvert.SerializeObject(orderEvent);

            var result = await producer.ProduceAsync(_topicName,
                new Message<string, string>
                {
                    Key = orderId,    // Same key = same partition every time
                    Value = json
                });

            Console.WriteLine($"   [{eventType}] → Partition {result.Partition} @ Offset {result.Offset}");
        }

        Console.WriteLine($"\n✅ All 4 events for {orderId} should be in the same partition!");
        Console.WriteLine("   A consumer reading that partition will see them in ORDER.");
    }

    /// <summary>Entry point for the partitioned producer demo.</summary>
    public static async Task RunDemoAsync(string bootstrapServers)
    {
        var producer = new PartitionedProducer(bootstrapServers);

        Console.WriteLine("=== INTERMEDIATE: Partitioned Producer Demo ===\n");

        // Demo 1: Send multiple orders (different keys = different partitions)
        Console.WriteLine("--- Demo 1: Multiple Orders (Key-Based Partitioning) ---");
        var orders = OrderEventFactory.CreateMany(6);
        foreach (var order in orders)
        {
            await producer.SendOrderEventAsync(order);
        }

        // Demo 2: Demonstrate ordering guarantee
        Console.WriteLine("\n--- Demo 2: Event Ordering Guarantee ---");
        await producer.DemonstrateOrderingAsync();

        Console.WriteLine("\n✅ Partitioned Producer demo complete!");
        Console.WriteLine("💡 Notice how the same OrderId always maps to the same partition.");
    }
}
