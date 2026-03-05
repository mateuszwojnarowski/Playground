// =============================================================================
// PerformanceTuningProducer.cs - ADVANCED LEVEL: High-Throughput Optimization
// =============================================================================
//
// KAFKA PERFORMANCE FUNDAMENTALS:
//   Kafka is designed for high throughput, but default settings favor safety
//   over performance. Understanding these settings lets you tune for your needs.
//
// THE THROUGHPUT EQUATION:
//   throughput ≈ (batch_size × num_partitions) / linger_ms
//
//   To maximize throughput:
//   1. Increase batch size (more messages per network request)
//   2. Use compression (more data per network byte)
//   3. Increase linger.ms (wait longer to fill batches)
//   4. Use more partitions (more parallelism)
//   5. Increase buffer.memory (more messages queued in memory)
//   6. Use async fire-and-forget (don't wait for each ack)
//
// LATENCY VS THROUGHPUT TRADEOFF:
//   Low latency  ←─────────────────→ High throughput
//   linger.ms=0     linger.ms=5ms     linger.ms=20ms
//   batch.size=1KB  batch.size=64KB   batch.size=1MB
//   acks=1          acks=all          acks=all (with batching)
//
// REAL-WORLD BENCHMARKS (rough guidelines, hardware-dependent):
//   Default settings:    ~50,000 msg/sec
//   Optimized settings:  ~500,000-1,000,000 msg/sec
//   With compression:    Up to 5x more data per second
//
// MEMORY CALCULATION:
//   buffer.memory should be at least:
//   batch.size × number_of_partitions × linger_ms_factor
//   Example: 64KB × 100 partitions × 10 = ~64MB minimum buffer

using System.Diagnostics;
using Confluent.Kafka;

namespace Kafka101.Advanced;

/// <summary>
/// Demonstrates performance tuning techniques for high-throughput Kafka producers.
/// </summary>
public class PerformanceTuningProducer
{
    private readonly string _bootstrapServers;
    private readonly string _topicName;

    public PerformanceTuningProducer(string bootstrapServers, string topicName = "advanced-perf-test")
    {
        _bootstrapServers = bootstrapServers;
        _topicName = topicName;
    }

    /// <summary>
    /// Creates a baseline producer with DEFAULT settings.
    /// Use this to compare against optimized settings.
    ///
    /// DEFAULT SETTINGS VALUES (from Confluent documentation):
    /// - BatchSize: 16,384 bytes (16KB)
    /// - LingerMs: 0 (send immediately, no batching)
    /// - CompressionType: None
    /// - BufferMemory: 33,554,432 bytes (32MB)
    /// - MaxInFlight: 5
    /// - Acks: Leader (1)
    /// </summary>
    private ProducerConfig CreateBaselineConfig() => new()
    {
        BootstrapServers = _bootstrapServers,
        ClientId = "kafka101-baseline-producer"
        // All other settings are Kafka defaults
    };

    /// <summary>
    /// Creates a high-throughput optimized producer.
    ///
    /// OPTIMIZATION EXPLANATIONS:
    ///
    /// BatchNumMessages (batch.num.messages):
    ///   Maximum number of messages in a single batch.
    ///   Confluent.Kafka uses this instead of batch.size bytes.
    ///   Higher = larger batches = better throughput but more latency.
    ///
    /// LingerMs (linger.ms):
    ///   Wait this many milliseconds for more messages before sending.
    ///   0ms = send immediately (low latency, small batches)
    ///   5-10ms = wait to accumulate messages (higher throughput)
    ///   In practice, even 1ms makes a significant difference.
    ///
    /// CompressionType:
    ///   Compress batches before sending. Snappy is a good default:
    ///   - Fast compression/decompression
    ///   - Good compression ratio for JSON/text (typically 4-6x)
    ///   - Small CPU overhead
    ///
    /// QueueBufferingMaxMessages (queue.buffering.max.messages):
    ///   Maximum messages to buffer in memory before blocking.
    ///   Increase for burst scenarios. Each message uses ~1KB minimum.
    ///
    /// QueueBufferingMaxKbytes (queue.buffering.max.kbytes):
    ///   Total memory for all buffered messages across all partitions.
    ///   Default: 1GB. Reduce if memory-constrained.
    ///
    /// MessageSendMaxRetries (message.send.max.retries):
    ///   Retry transient send failures before giving up.
    ///   Default: 2147483647 (basically infinite with idempotence).
    ///
    /// RetryBackoffMs (retry.backoff.ms):
    ///   Wait time between retry attempts.
    ///   Prevents hammering a struggling broker.
    /// </summary>
    private ProducerConfig CreateHighThroughputConfig() => new()
    {
        BootstrapServers = _bootstrapServers,
        ClientId = "kafka101-highthroughput-producer",

        // BATCHING: Wait up to 5ms to accumulate messages in a batch
        // This single setting can 5-10x throughput at the cost of 5ms latency
        LingerMs = 5,

        // BATCH SIZE: Up to 1000 messages per batch (Confluent.Kafka specific)
        BatchNumMessages = 1000,

        // COMPRESSION: Snappy reduces network/disk usage by ~4-6x for JSON data
        CompressionType = CompressionType.Snappy,

        // BUFFERING: Allow more messages to queue in memory during bursts
        QueueBufferingMaxMessages = 1000000,  // 1 million messages
        QueueBufferingMaxKbytes = 1048576,    // 1GB buffer

        // ACKNOWLEDGMENT: Use Acks.Leader (1) for performance
        // (vs Acks.All which requires all replicas to confirm)
        Acks = Acks.Leader,

        // RETRY: Retry transient failures
        MessageSendMaxRetries = 3,
        RetryBackoffMs = 100,

        // SOCKET: Increase OS-level socket buffer for high throughput
        SocketSendBufferBytes = 131072, // 128KB

        // METADATA: Cache metadata longer to reduce broker requests
        MetadataMaxAgeMs = 300000 // 5 minutes
    };

    /// <summary>
    /// Creates an ultra-low-latency producer configuration.
    ///
    /// LATENCY OPTIMIZATIONS:
    /// - LingerMs = 0: Send immediately, no batching
    /// - Acks = Leader: Don't wait for replicas
    /// - Small batch: Less accumulation time
    ///
    /// TRADEOFFS:
    /// - Lower throughput (more network requests)
    /// - More CPU and memory overhead per message
    /// - Higher network bandwidth usage
    ///
    /// USE CASES:
    /// - Real-time alerts/notifications
    /// - Low-latency command/control systems
    /// - Financial trading (every millisecond counts)
    /// </summary>
    private ProducerConfig CreateLowLatencyConfig() => new()
    {
        BootstrapServers = _bootstrapServers,
        ClientId = "kafka101-lowlatency-producer",

        // Send immediately - no waiting for batch accumulation
        LingerMs = 0,

        // Single message batches for minimum latency
        BatchNumMessages = 1,

        // No compression (adds CPU overhead = latency)
        CompressionType = CompressionType.None,

        // Acknowledge from leader only (fastest)
        Acks = Acks.Leader
    };

    /// <summary>
    /// Benchmarks producer performance with different configurations.
    /// Sends a large batch of messages and measures throughput.
    ///
    /// NOTE: Results vary significantly based on:
    /// - Network latency to Kafka broker
    /// - Broker hardware (disk I/O, network)
    /// - Number of partitions
    /// - Message size
    /// - Replication factor
    /// </summary>
    public async Task BenchmarkAsync(int messageCount = 1000)
    {
        var message = new string('X', 200); // 200-byte test payload

        Console.WriteLine($"📊 Benchmarking with {messageCount} messages of 200 bytes each\n");

        // Run with baseline config
        var baselineThroughput = await MeasureThroughputAsync(
            "Baseline (defaults)", CreateBaselineConfig(), messageCount, message);

        // Run with high-throughput config
        var optimizedThroughput = await MeasureThroughputAsync(
            "High-Throughput (batching + compression)", CreateHighThroughputConfig(), messageCount, message);

        Console.WriteLine($"\n📈 Performance Summary:");
        Console.WriteLine($"   Baseline:     {baselineThroughput:N0} msg/sec");
        Console.WriteLine($"   Optimized:    {optimizedThroughput:N0} msg/sec");

        if (baselineThroughput > 0)
        {
            var improvement = (optimizedThroughput - baselineThroughput) / baselineThroughput * 100;
            Console.WriteLine($"   Improvement:  {improvement:+0.0;-0.0}%");
        }

        Console.WriteLine("\n💡 Higher linger.ms = better throughput but more latency");
        Console.WriteLine("   Adjust based on your latency SLA requirements");
    }

    /// <summary>
    /// Measures throughput for a given producer configuration.
    /// Uses fire-and-forget (Produce + Flush) for maximum throughput measurement.
    /// </summary>
    private async Task<double> MeasureThroughputAsync(
        string configName, ProducerConfig config, int messageCount, string payload)
    {
        Console.WriteLine($"🚀 Testing: {configName}");

        using var producer = new ProducerBuilder<Null, string>(config).Build();

        var sw = Stopwatch.StartNew();
        var delivered = 0;
        var failed = 0;

        // Fire-and-forget for maximum throughput
        for (var i = 0; i < messageCount; i++)
        {
            producer.Produce(
                _topicName,
                new Message<Null, string> { Value = $"{payload}#{i}" },
                report =>
                {
                    if (report.Error.Code == ErrorCode.NoError)
                        Interlocked.Increment(ref delivered);
                    else
                        Interlocked.Increment(ref failed);
                });
        }

        // Wait for all messages to be acknowledged
        producer.Flush(TimeSpan.FromSeconds(60));
        sw.Stop();

        var throughput = messageCount / sw.Elapsed.TotalSeconds;
        Console.WriteLine($"   ✅ {delivered} delivered, {failed} failed in {sw.Elapsed.TotalMilliseconds:F0}ms");
        Console.WriteLine($"   📊 Throughput: {throughput:N0} msg/sec ({throughput * 200 / 1024 / 1024:F1} MB/sec)\n");

        await Task.CompletedTask;
        return throughput;
    }

    /// <summary>
    /// Demonstrates message size impact on throughput.
    ///
    /// LARGE vs SMALL MESSAGES:
    /// - Small messages (< 1KB): Network overhead dominates. Use batching.
    /// - Large messages (> 1MB): Serialization and network transfer dominate.
    ///
    /// OPTIMAL MESSAGE SIZE:
    /// Usually 1KB - 100KB. Avoid messages > 1MB (configure max.message.bytes
    /// on broker if needed, but better to split into multiple messages).
    ///
    /// FOR LARGE DATA:
    /// - Store data in blob storage (S3, Azure Blob)
    /// - Send reference (URL) in Kafka message
    /// - This is the "Claim Check" pattern
    /// </summary>
    public async Task DemonstrateMessageSizesAsync()
    {
        Console.WriteLine("📐 Message Size Comparison:");

        var sizes = new[] { 100, 1024, 10240 }; // 100B, 1KB, 10KB

        foreach (var size in sizes)
        {
            var payload = new string('X', size);
            Console.WriteLine($"\n   Message size: {size:N0} bytes ({size / 1024.0:F1} KB)");

            await MeasureThroughputAsync(
                $"Size: {size}B",
                CreateHighThroughputConfig(),
                messageCount: 100,
                payload: payload);
        }
    }

    /// <summary>Entry point for performance tuning demo.</summary>
    public static async Task RunDemoAsync(string bootstrapServers)
    {
        var demo = new PerformanceTuningProducer(bootstrapServers);

        Console.WriteLine("=== ADVANCED: Performance Tuning Demo ===\n");
        Console.WriteLine("⚠️  This demo sends many messages for benchmarking.");
        Console.WriteLine("   Results depend on your hardware and Kafka setup.\n");

        await demo.BenchmarkAsync(messageCount: 500);

        Console.WriteLine("\n✅ Performance Tuning demo complete!");
        Console.WriteLine("💡 Key insight: linger.ms=5 + compression often gives 5-10x throughput improvement");
    }
}
