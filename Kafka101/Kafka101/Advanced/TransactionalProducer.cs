// =============================================================================
// TransactionalProducer.cs - ADVANCED LEVEL: ACID Guarantees with Transactions
// =============================================================================
//
// WHAT ARE KAFKA TRANSACTIONS?
//   Kafka transactions allow you to produce to MULTIPLE topics/partitions
//   atomically - either ALL messages are committed, or NONE are.
//
//   This is similar to database transactions:
//   BEGIN TRANSACTION
//     INSERT INTO orders ...
//     UPDATE inventory ...
//   COMMIT (or ROLLBACK if anything fails)
//
// WHY TRANSACTIONS ARE NEEDED:
//   Consider an "Order Processing" service that:
//   1. Reads an order from "incoming-orders" topic
//   2. Publishes to "confirmed-orders" topic
//   3. Publishes to "inventory-updates" topic
//
//   WITHOUT TRANSACTIONS: If step 3 fails, step 2 is already committed!
//   - "confirmed-orders" has the order
//   - "inventory-updates" does NOT have the update
//   - Your system is in an INCONSISTENT STATE
//
//   WITH TRANSACTIONS:
//   - If step 3 fails → entire transaction is aborted
//   - Neither "confirmed-orders" nor "inventory-updates" see the messages
//   - System remains consistent
//
// EXACTLY-ONCE SEMANTICS (EOS):
//   Combining idempotent producer + transactions + consumer isolation gives you:
//   - EXACTLY-ONCE: Each message is processed and written exactly once
//   - No duplicates (idempotent producer)
//   - No partial writes (transactions)
//   - Consumers only see committed data (isolation.level=read_committed)
//
// THE READ-PROCESS-WRITE PATTERN (most common transactional use case):
//
//   consumer.Poll() → process message → producer.Send() to output topic
//                                     → consumer.CommitOffset() as part of transaction
//
//   This ensures: input offset commit AND output message are atomic.
//   If anything fails, BOTH are rolled back.
//
// TRANSACTION COORDINATOR:
//   Kafka uses a "Transaction Coordinator" broker to manage transaction state.
//   The TransactionalId uniquely identifies a producer instance across restarts.
//   If a producer crashes mid-transaction, the new instance can abort/commit it.
//
// PERFORMANCE COST:
//   Transactions add overhead:
//   - Extra metadata in each message
//   - Coordinator communication (begin, commit, abort)
//   - Consumers must filter out aborted transaction messages
//   - Typically 10-20% throughput reduction vs non-transactional
//   Use only when exactly-once semantics are truly required!

using Confluent.Kafka;
using Kafka101.Models;
using Newtonsoft.Json;

namespace Kafka101.Advanced;

/// <summary>
/// Demonstrates Kafka transactions for exactly-once, atomic writes across topics.
/// </summary>
public class TransactionalProducer
{
    private readonly string _bootstrapServers;
    private readonly string _inputTopic;
    private readonly string _outputTopic;
    private readonly string _errorTopic;

    public TransactionalProducer(
        string bootstrapServers,
        string inputTopic = "advanced-orders",
        string outputTopic = "processed-orders",
        string errorTopic = "failed-orders")
    {
        _bootstrapServers = bootstrapServers;
        _inputTopic = inputTopic;
        _outputTopic = outputTopic;
        _errorTopic = errorTopic;
    }

    /// <summary>
    /// Creates a transactional producer configuration.
    ///
    /// REQUIREMENTS for transactional producers:
    /// 1. TransactionalId MUST be set and UNIQUE per producer instance
    ///    In production: use service_name + partition_number
    ///    Example: "order-processor-0", "order-processor-1"
    ///    This ensures only ONE instance uses a given TransactionalId at a time.
    ///
    /// 2. Acks MUST be All (-1) - transactions require full acknowledgment
    ///
    /// 3. EnableIdempotence MUST be true - auto-set when TransactionalId is specified
    ///
    /// TRANSACTIONAL ID UNIQUENESS:
    ///   If two producers use the same TransactionalId simultaneously,
    ///   the older one gets "fenced" (any writes are rejected).
    ///   This is a safety mechanism - prevents zombie producers from corrupting data.
    /// </summary>
    private ProducerConfig CreateTransactionalConfig(string transactionalId) => new()
    {
        BootstrapServers = _bootstrapServers,

        // THE KEY SETTING: Unique ID for this producer across its lifetime
        // Kafka uses this to:
        // 1. Deduplicate retried messages (idempotence)
        // 2. Complete or abort transactions from crashed producers
        // 3. "Fence" old producer instances when new ones start
        TransactionalId = transactionalId,

        // Required for transactions (auto-enabled by TransactionalId)
        EnableIdempotence = true,
        Acks = Acks.All,

        // Transaction timeout: How long before an open transaction is auto-aborted
        // Default: 60000ms (1 minute)
        // Keep short for most use cases; long-running transactions hold resources
        TransactionTimeoutMs = 30000,

        ClientId = $"kafka101-transactional-{transactionalId}"
    };

    /// <summary>
    /// Demonstrates writing to MULTIPLE topics atomically.
    ///
    /// SCENARIO: Process an order and simultaneously:
    /// 1. Write to "processed-orders" topic (order confirmed)
    /// 2. Write to an audit log topic (for compliance)
    ///
    /// Either BOTH writes succeed, or NEITHER does.
    /// This eliminates the partial-write inconsistency problem.
    /// </summary>
    public async Task WriteToMultipleTopicsAtomicallyAsync(OrderEvent order)
    {
        using var producer = new ProducerBuilder<string, string>(
            CreateTransactionalConfig("order-processor-multi-0")).Build();

        // STEP 1: Initialize transactions (must be called once before any transactions)
        // This registers the producer with the Transaction Coordinator
        producer.InitTransactions(TimeSpan.FromSeconds(30));

        Console.WriteLine($"\n🔄 Starting atomic write for order {order.OrderId}");

        try
        {
            // STEP 2: Begin a new transaction
            producer.BeginTransaction();
            Console.WriteLine("   Transaction STARTED");

            // STEP 3: Write to multiple topics within the same transaction
            var orderJson = JsonConvert.SerializeObject(order);

            // Write 1: Processed order
            await producer.ProduceAsync(_outputTopic,
                new Message<string, string>
                {
                    Key = order.OrderId,
                    Value = orderJson,
                    Headers = new Headers { { "transaction-type", System.Text.Encoding.UTF8.GetBytes("order-processed") } }
                });
            Console.WriteLine($"   ✏️  Wrote to {_outputTopic}");

            // Write 2: Audit log (same transaction!)
            var auditRecord = JsonConvert.SerializeObject(new
            {
                Action = "ORDER_PROCESSED",
                OrderId = order.OrderId,
                ProcessedAt = DateTime.UtcNow,
                ProcessorId = "order-processor-0"
            });

            await producer.ProduceAsync("audit-log",
                new Message<string, string>
                {
                    Key = order.OrderId,
                    Value = auditRecord
                });
            Console.WriteLine($"   ✏️  Wrote to audit-log");

            // STEP 4: Commit - makes BOTH writes visible to consumers atomically
            producer.CommitTransaction();
            Console.WriteLine("   Transaction COMMITTED ✅ (both writes visible)");
        }
        catch (Exception ex)
        {
            // STEP 4 (alternative): Abort - NEITHER write becomes visible
            Console.WriteLine($"   ❌ Error during transaction: {ex.Message}");

            try
            {
                producer.AbortTransaction();
                Console.WriteLine("   Transaction ABORTED ↩️  (neither write visible)");
            }
            catch (KafkaException abortEx)
            {
                // AbortTransaction can also fail (e.g., timeout with broker)
                // In this case, the transaction coordinator will eventually abort it
                Console.WriteLine($"   ⚠️  Abort failed: {abortEx.Message}. Coordinator will clean up.");
            }

            throw;
        }
    }

    /// <summary>
    /// Implements the READ-PROCESS-WRITE (consume-transform-produce) pattern.
    ///
    /// This is the MOST COMMON use of Kafka transactions, used in:
    /// - Stream processing applications (Kafka Streams uses this internally)
    /// - ETL pipelines
    /// - Event-driven microservices needing exactly-once processing
    ///
    /// THE PATTERN:
    /// 1. Consumer reads message from input topic
    /// 2. Application transforms/enriches the message
    /// 3. Producer writes result to output topic
    /// 4. Input offset commit + output write are in the SAME TRANSACTION
    ///
    /// This guarantees:
    /// - If transaction commits: input offset advanced, output written ✓
    /// - If transaction aborts: input offset stays, output not written ✓
    /// - If consumer restarts: picks up from uncommitted offset, reprocesses ✓
    ///
    /// The result: EXACTLY-ONCE processing from input to output.
    /// </summary>
    public async Task ReadProcessWriteAsync(CancellationToken cancellationToken, int maxMessages = 3)
    {
        // CONSUMER: Must use isolation.level=read_committed to skip aborted transactions
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _bootstrapServers,
            GroupId = "transactional-processor-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false, // WE commit offsets as part of the transaction

            // READ_COMMITTED: Only see messages from committed transactions
            // READ_UNCOMMITTED (default): See all messages including aborted ones
            // For transactions to work end-to-end, consumers MUST use read_committed
            IsolationLevel = IsolationLevel.ReadCommitted
        };

        using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        using var producer = new ProducerBuilder<string, string>(
            CreateTransactionalConfig("order-processor-rpw-0")).Build();

        producer.InitTransactions(TimeSpan.FromSeconds(30));
        consumer.Subscribe(_inputTopic);

        Console.WriteLine($"\n🔄 Read-Process-Write pattern");
        Console.WriteLine($"   Input:  {_inputTopic}");
        Console.WriteLine($"   Output: {_outputTopic}");
        Console.WriteLine($"   Mode:   EXACTLY-ONCE\n");

        var processed = 0;

        try
        {
            while (!cancellationToken.IsCancellationRequested && processed < maxMessages)
            {
                var result = consumer.Consume(TimeSpan.FromSeconds(3));
                if (result == null || result.IsPartitionEOF) continue;

                Console.WriteLine($"📨 Read: {result.Message.Key} @ offset {result.Offset}");

                try
                {
                    producer.BeginTransaction();

                    // TRANSFORM: Enrich the order (business logic)
                    var processedOrder = TransformOrder(result.Message.Value);

                    // WRITE: Produce to output topic
                    await producer.ProduceAsync(_outputTopic,
                        new Message<string, string>
                        {
                            Key = result.Message.Key,
                            Value = processedOrder
                        });

                    // CRITICAL: Commit the INPUT OFFSET as part of the transaction!
                    // This is the key to exactly-once - the consumer offset advance
                    // and the output write are atomic.
                    producer.SendOffsetsToTransaction(
                        new[] { new TopicPartitionOffset(result.TopicPartition, result.Offset + 1) },
                        consumer.ConsumerGroupMetadata,
                        TimeSpan.FromSeconds(10));

                    producer.CommitTransaction();

                    processed++;
                    Console.WriteLine($"✅ Processed and committed: {result.Message.Key}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Processing failed: {ex.Message}");
                    producer.AbortTransaction();
                    // Consumer offset is NOT advanced - will reprocess this message
                    Console.WriteLine("   Transaction aborted - message will be reprocessed");
                }
            }
        }
        finally
        {
            consumer.Close();
        }

        Console.WriteLine($"\n✅ Read-Process-Write complete. Processed: {processed} messages.");
    }

    /// <summary>
    /// Simulates order transformation/enrichment logic.
    /// In a real app, this would be your business processing.
    /// </summary>
    private static string TransformOrder(string rawOrderJson)
    {
        // Parse and enrich the order
        var order = JsonConvert.DeserializeObject<OrderEvent>(rawOrderJson);
        if (order == null) throw new ArgumentException("Invalid order JSON");

        // Apply business logic transformation
        order.Status = OrderStatus.Confirmed;

        return JsonConvert.SerializeObject(new
        {
            Original = order,
            ProcessedAt = DateTime.UtcNow,
            ProcessingVersion = "1.0.0",
            ValidationStatus = "PASSED"
        });
    }

    /// <summary>Entry point for transactional producer demo.</summary>
    public static async Task RunDemoAsync(string bootstrapServers)
    {
        var demo = new TransactionalProducer(bootstrapServers);

        Console.WriteLine("=== ADVANCED: Transactional Producer Demo ===\n");

        // Demo 1: Write to multiple topics atomically
        Console.WriteLine("--- Demo 1: Atomic Multi-Topic Write ---");
        var sampleOrder = OrderEventFactory.Create();

        try
        {
            await demo.WriteToMultipleTopicsAtomicallyAsync(sampleOrder);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   Demo failed (expected without full setup): {ex.Message}");
        }

        // Demo 2: Read-Process-Write
        Console.WriteLine("\n--- Demo 2: Read-Process-Write (Exactly-Once) ---");
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));

        try
        {
            await demo.ReadProcessWriteAsync(cts.Token, maxMessages: 3);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   Demo failed (expected without full setup): {ex.Message}");
        }

        Console.WriteLine("\n✅ Transactional Producer demo complete!");
        Console.WriteLine("💡 Transactions ensure atomic writes across multiple topics.");
        Console.WriteLine("   Combined with read_committed consumers = exactly-once processing.");
    }
}
