// =============================================================================
// ErrorHandlingConsumer.cs - ADVANCED LEVEL: Robust Error Handling Patterns
// =============================================================================
//
// WHY ERROR HANDLING MATTERS:
//   In production, things go wrong constantly:
//   - Network blips causing transient failures
//   - Malformed messages that can never be processed (poison pills)
//   - Downstream service unavailability
//   - Schema mismatches after deployments
//   - Business logic exceptions
//
//   Without proper error handling, your consumer will:
//   1. Crash completely (losing processing)
//   2. Get stuck on a poison message forever
//   3. Skip messages silently (data loss)
//   4. Retry indefinitely (wasting resources)
//
// CORE PATTERNS COVERED:
//
//   1. RETRY WITH BACKOFF:
//      Try again with increasing delays. Good for transient failures.
//      Pattern: 1s → 2s → 4s → 8s → give up
//
//   2. DEAD LETTER QUEUE (DLQ):
//      Move unprocessable messages to a separate "dead letter" topic.
//      Operations team can inspect, fix, and replay later.
//      Prevents one bad message from blocking the whole stream.
//
//   3. CIRCUIT BREAKER:
//      After N failures, stop trying for a period. Protects downstream services.
//      State: CLOSED (normal) → OPEN (failing) → HALF-OPEN (testing recovery)
//
//   4. DEAD LETTER TOPIC:
//      Standard pattern: original-topic.DLT or original-topic-dlq
//      Include original message + error details in the dead letter message

using System.Text;
using Confluent.Kafka;
using Kafka101.Models;
using Newtonsoft.Json;

namespace Kafka101.Advanced;

/// <summary>
/// Demonstrates production-grade error handling for Kafka consumers.
/// </summary>
public class ErrorHandlingConsumer
{
    private readonly string _bootstrapServers;
    private readonly string _topicName;
    private readonly string _dlqTopicName;

    // Maximum number of retries before sending to dead letter queue
    private const int MaxRetries = 3;

    // Base delay for exponential backoff
    private static readonly TimeSpan BaseRetryDelay = TimeSpan.FromSeconds(1);

    public ErrorHandlingConsumer(string bootstrapServers, string topicName = "advanced-orders")
    {
        _bootstrapServers = bootstrapServers;
        _topicName = topicName;

        // NAMING CONVENTION: Dead Letter Queue topic = original topic + ".DLT"
        // This is the standard convention used by Spring Kafka and most Kafka frameworks.
        // ".DLT" = Dead Letter Topic
        _dlqTopicName = $"{topicName}.DLT";
    }

    /// <summary>
    /// Consumer with comprehensive retry logic and dead letter queue.
    ///
    /// RETRY STRATEGY DECISION TREE:
    ///
    ///   Error occurs
    ///   ├── Is it a deserialization error?
    ///   │   └── SKIP or send to DLQ immediately (retrying won't fix bad bytes)
    ///   ├── Is it a transient error? (network, DB connection timeout)
    ///   │   └── RETRY with exponential backoff, up to MaxRetries
    ///   ├── Is it a permanent error? (invalid business data, logic error)
    ///   │   └── Send to DLQ immediately, don't waste retries
    ///   └── Is it a fatal consumer error?
    ///       └── Recreate the consumer (see below)
    /// </summary>
    public async Task ConsumeWithRetryAndDlqAsync(CancellationToken cancellationToken, int maxMessages = 10)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _bootstrapServers,
            GroupId = "error-handling-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnableAutoOffsetStore = false
        };

        using var dlqProducer = CreateDlqProducer();
        using var consumer = new ConsumerBuilder<string, string>(config)
            // Handle deserialization errors without crashing the consumer
            .SetErrorHandler((_, error) =>
            {
                Console.WriteLine($"⚠️  Consumer-level error: [{error.Code}] {error.Reason}");
                // Fatal errors require consumer restart - log and alert your team
                if (error.IsFatal)
                    Console.WriteLine("💀 FATAL ERROR: Consumer must be restarted!");
            })
            .Build();

        consumer.Subscribe(_topicName);
        Console.WriteLine($"👂 Error-handling consumer listening on {_topicName}");
        Console.WriteLine($"   DLQ topic: {_dlqTopicName}\n");

        var processed = 0;

        try
        {
            while (!cancellationToken.IsCancellationRequested && processed < maxMessages)
            {
                ConsumeResult<string, string>? result = null;

                try
                {
                    result = consumer.Consume(TimeSpan.FromSeconds(3));
                    if (result == null || result.IsPartitionEOF) continue;

                    Console.WriteLine($"\n📨 Processing: {result.Message.Key} @ offset {result.Offset}");

                    // Attempt to process with retry logic
                    var success = await ProcessWithRetryAsync(result, cancellationToken);

                    if (success)
                    {
                        // SUCCESS: Commit the offset
                        consumer.StoreOffset(result);
                        consumer.Commit();
                        processed++;
                        Console.WriteLine($"✅ Committed offset {result.Offset}");
                    }
                    else
                    {
                        // PERMANENT FAILURE: Send to dead letter queue
                        await SendToDeadLetterQueueAsync(dlqProducer, result,
                            "Exceeded maximum retries");

                        // Still commit to move past this message!
                        // If we don't commit, we'll be stuck on this message forever.
                        consumer.StoreOffset(result);
                        consumer.Commit();
                        Console.WriteLine($"⚰️  Message sent to DLQ and offset committed");
                    }
                }
                catch (ConsumeException ex) when (!ex.Error.IsFatal)
                {
                    // Non-fatal consume error (e.g., broker unavailable momentarily)
                    Console.WriteLine($"⚠️  Non-fatal consume error: {ex.Error.Reason}");
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw; // Let the outer catch handle graceful shutdown
                }
                catch (Exception ex)
                {
                    // Unexpected error - log and try to continue
                    Console.WriteLine($"❌ Unexpected error: {ex.GetType().Name}: {ex.Message}");

                    // If we have a result, send to DLQ and move on
                    if (result != null)
                    {
                        await SendToDeadLetterQueueAsync(dlqProducer, result, ex.Message);
                        consumer.StoreOffset(result);
                        consumer.Commit();
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("\n🛑 Consumer shutting down...");
        }
        finally
        {
            consumer.Close();
        }

        Console.WriteLine($"\n✅ Error handling consumer complete. Processed: {processed}");
    }

    /// <summary>
    /// Processes a message with exponential backoff retry logic.
    ///
    /// EXPONENTIAL BACKOFF EXPLAINED:
    /// Instead of retrying immediately (which can overwhelm a struggling service),
    /// we wait increasingly longer between retries:
    /// Attempt 1: Wait 1 second
    /// Attempt 2: Wait 2 seconds
    /// Attempt 3: Wait 4 seconds
    /// After 3 attempts: Give up and send to DLQ
    ///
    /// OPTIONAL ENHANCEMENT: Add jitter (randomness) to the backoff to prevent
    /// multiple consumers from retrying simultaneously (thundering herd problem):
    /// delay = baseDelay * 2^attempt + random(0, 1000ms)
    /// </summary>
    private static async Task<bool> ProcessWithRetryAsync(
        ConsumeResult<string, string> result,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt <= MaxRetries; attempt++)
        {
            try
            {
                if (attempt > 0)
                {
                    // Exponential backoff: 1s, 2s, 4s
                    var delay = BaseRetryDelay * Math.Pow(2, attempt - 1);

                    // ADD JITTER: Avoid thundering herd when many consumers retry simultaneously
                    var jitter = TimeSpan.FromMilliseconds(new Random().Next(0, 500));
                    delay += jitter;

                    Console.WriteLine($"   ⏳ Retry attempt {attempt}/{MaxRetries} after {delay.TotalSeconds:F1}s...");
                    await Task.Delay(delay, cancellationToken);
                }

                // TRY TO PROCESS THE MESSAGE
                // In real code, this would call your business logic:
                // await _orderService.ProcessOrderAsync(order);
                // await _database.SaveAsync(result);
                // await _emailService.SendConfirmationAsync(order);
                SimulateUnreliableProcessing(result.Message.Value, attempt);

                Console.WriteLine($"   ✅ Processing succeeded on attempt {attempt + 1}");
                return true; // Success!
            }
            catch (PermanentProcessingException ex)
            {
                // PERMANENT ERROR: Retrying won't help (e.g., invalid data format)
                // Send to DLQ immediately without using up all retry attempts
                Console.WriteLine($"   💀 Permanent error (no retry): {ex.Message}");
                return false;
            }
            catch (TransientProcessingException ex)
            {
                // TRANSIENT ERROR: Retry makes sense (network, temporary unavailability)
                Console.WriteLine($"   ⚠️  Transient error on attempt {attempt + 1}: {ex.Message}");
                if (attempt == MaxRetries)
                {
                    Console.WriteLine($"   💀 Max retries ({MaxRetries}) exceeded");
                    return false; // Give up, send to DLQ
                }
                // Continue to next iteration (retry)
            }
        }

        return false;
    }

    /// <summary>
    /// Sends a failed message to the Dead Letter Queue topic.
    ///
    /// DLQ MESSAGE ENRICHMENT:
    /// Don't just copy the original message - add context to help debugging:
    /// - Original topic, partition, offset
    /// - Error message and stack trace
    /// - Number of processing attempts
    /// - Timestamp when it was moved to DLQ
    /// - Consumer group ID (which service failed to process it)
    ///
    /// This metadata is critical for operations teams to understand
    /// WHY messages are in the DLQ and HOW to fix them.
    /// </summary>
    private async Task SendToDeadLetterQueueAsync(
        IProducer<string, string> dlqProducer,
        ConsumeResult<string, string> originalResult,
        string errorMessage)
    {
        // Enrich the DLQ message with diagnostic information
        var dlqPayload = new
        {
            OriginalMessage = originalResult.Message.Value,
            OriginalKey = originalResult.Message.Key,
            OriginalTopic = originalResult.Topic,
            OriginalPartition = originalResult.Partition.Value,
            OriginalOffset = originalResult.Offset.Value,
            ErrorMessage = errorMessage,
            FailedAt = DateTime.UtcNow.ToString("O"), // ISO 8601 format
            ConsumerGroup = "error-handling-group",
            RetryCount = MaxRetries
        };

        var dlqMessage = new Message<string, string>
        {
            Key = originalResult.Message.Key,
            Value = JsonConvert.SerializeObject(dlqPayload),
            Headers = new Headers
            {
                // Copy original headers for tracing
                { "x-original-topic", Encoding.UTF8.GetBytes(originalResult.Topic) },
                { "x-original-partition", Encoding.UTF8.GetBytes(originalResult.Partition.Value.ToString()) },
                { "x-original-offset", Encoding.UTF8.GetBytes(originalResult.Offset.Value.ToString()) },
                { "x-error-message", Encoding.UTF8.GetBytes(errorMessage) },
                { "x-failed-at", Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("O")) }
            }
        };

        try
        {
            var dlqResult = await dlqProducer.ProduceAsync(_dlqTopicName, dlqMessage);
            Console.WriteLine($"📬 DLQ message sent: {_dlqTopicName} P{dlqResult.Partition}@{dlqResult.Offset}");
        }
        catch (ProduceException<string, string> ex)
        {
            // If we can't even write to DLQ, log loudly and alert!
            // This is a critical situation - messages may be permanently lost.
            Console.WriteLine($"💥 CRITICAL: Failed to write to DLQ! Error: {ex.Error.Reason}");
            Console.WriteLine($"   ORIGINAL MESSAGE LOST: {originalResult.Message.Key}");
            // In production: alert PagerDuty / OpsGenie / send to fallback DLQ
        }
    }

    /// <summary>Creates a reliable producer for dead letter queue messages.</summary>
    private IProducer<string, string> CreateDlqProducer()
    {
        var config = new ProducerConfig
        {
            BootstrapServers = _bootstrapServers,
            Acks = Acks.All,          // Maximum reliability for DLQ
            EnableIdempotence = true,  // No duplicate DLQ entries
            ClientId = "kafka101-dlq-producer"
        };

        return new ProducerBuilder<string, string>(config).Build();
    }

    /// <summary>
    /// Simulates unreliable message processing for demonstration.
    /// In production, this would be your actual business logic.
    /// </summary>
    private static void SimulateUnreliableProcessing(string messageValue, int attempt)
    {
        // Simulate "poison pill" - message that can never be processed
        if (messageValue.Contains("INVALID"))
            throw new PermanentProcessingException("Message contains invalid data marker");

        // Simulate transient failures that succeed after 2 retries
        if (attempt < 2 && new Random().Next(3) == 0)
            throw new TransientProcessingException("Simulated transient downstream service failure");

        // Simulate processing time
        Thread.Sleep(new Random().Next(10, 30));
    }

    /// <summary>Entry point for error handling demo.</summary>
    public static async Task RunDemoAsync(string bootstrapServers)
    {
        var demo = new ErrorHandlingConsumer(bootstrapServers);

        Console.WriteLine("=== ADVANCED: Error Handling Consumer Demo ===\n");
        Console.WriteLine($"Topic: advanced-orders");
        Console.WriteLine($"DLQ: advanced-orders.DLT\n");

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

        await demo.ConsumeWithRetryAndDlqAsync(cts.Token, maxMessages: 5);

        Console.WriteLine("\n✅ Error Handling demo complete!");
        Console.WriteLine("💡 Check the DLQ topic for messages that couldn't be processed.");
    }
}

// =============================================================================
// CUSTOM EXCEPTION TYPES FOR ERROR CLASSIFICATION
// =============================================================================

/// <summary>
/// Exception indicating a transient error that may succeed if retried.
/// Examples: Network timeout, database connection lost, downstream service temporarily unavailable.
/// </summary>
public class TransientProcessingException : Exception
{
    public TransientProcessingException(string message) : base(message) { }
}

/// <summary>
/// Exception indicating a permanent error that will never succeed on retry.
/// Examples: Invalid data format, business rule violation, message schema mismatch.
/// </summary>
public class PermanentProcessingException : Exception
{
    public PermanentProcessingException(string message) : base(message) { }
}
