// =============================================================================
// CustomSerializationProducer.cs - ADVANCED LEVEL: Custom Serializers
// =============================================================================
//
// WHY CUSTOM SERIALIZATION?
//   Kafka transmits messages as raw bytes. You need to convert your objects
//   to bytes (serialize) before sending, and back to objects (deserialize)
//   after receiving.
//
//   DEFAULT SERIALIZERS available in Confluent.Kafka:
//   - Serializers.Utf8   - strings
//   - Serializers.Int32  - 32-bit integers
//   - Serializers.Int64  - 64-bit integers
//   - Serializers.Null   - null values
//
//   For COMPLEX OBJECTS, you need custom serialization.
//
// SERIALIZATION OPTIONS COMPARED:
//
//   JSON (used here):
//   + Human-readable, easy to debug
//   + Widely supported, no schema registry needed
//   + Schema evolution is possible with care (add nullable fields)
//   - Verbose = larger message size
//   - No compile-time type safety for the schema
//   - Schema drift possible without enforcement
//   Use when: Development, debugging, simple schemas, small scale
//
//   Avro:
//   + Compact binary format (smaller messages)
//   + Schema registry enforces compatibility
//   + Type-safe schema evolution with compatibility checks
//   + Supported by Confluent Schema Registry
//   - Requires schema registry infrastructure
//   - Less human-readable
//   Use when: Production, schema evolution important, high volume
//
//   Protobuf (Protocol Buffers):
//   + Very compact binary format
//   + Strongly typed with code generation
//   + Language-neutral schema definition
//   + Excellent backward/forward compatibility
//   - Requires schema files (.proto) and code generation
//   Use when: Cross-language teams, maximum performance needed
//
//   MessagePack:
//   + Compact binary, faster than JSON
//   + More compact than JSON, less than Avro/Protobuf
//   - Less ecosystem support than Avro/Protobuf
//   Use when: JSON is too slow/large but you want simplicity
//
// THE SCHEMA REGISTRY PATTERN (mentioned for completeness):
//   In production, use Confluent Schema Registry to:
//   1. Register schemas and get an ID
//   2. Store only the schema ID (4 bytes) + data in the message
//   3. Consumers look up the schema by ID to deserialize
//   This enables schema evolution with compatibility guarantees.

using System.Text;
using Confluent.Kafka;
using Kafka101.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Kafka101.Advanced;

// =============================================================================
// CUSTOM SERIALIZER IMPLEMENTATION
// =============================================================================

/// <summary>
/// JSON serializer for complex objects.
/// Implements ISerializer&lt;T&gt; from Confluent.Kafka to integrate with the producer.
///
/// SERIALIZER CONTRACT:
///   byte[] Serialize(T data, SerializationContext context)
///   - data: The object to serialize
///   - context: Metadata (topic, is it a key or value, headers)
///   - Returns: byte array to be stored in Kafka
/// </summary>
public class JsonSerializer<T> : ISerializer<T>
{
    private static readonly JsonSerializerSettings _settings = new()
    {
        // Include enum names instead of numbers (more readable + stable across code changes)
        Converters = { new StringEnumConverter() },

        // Format timestamps as ISO 8601 (universally parseable)
        DateFormatHandling = DateFormatHandling.IsoDateFormat,

        // Skip null properties to reduce message size
        NullValueHandling = NullValueHandling.Ignore,

        // For compact production messages, use Formatting.None
        // For readable debugging, use Formatting.Indented
        Formatting = Formatting.None
    };

    /// <summary>
    /// Converts an object to UTF-8 encoded JSON bytes.
    /// </summary>
    public byte[] Serialize(T data, SerializationContext context)
    {
        if (data == null)
            return [];

        var json = JsonConvert.SerializeObject(data, _settings);
        return Encoding.UTF8.GetBytes(json);
    }
}

/// <summary>
/// JSON deserializer for complex objects.
/// Implements IDeserializer&lt;T&gt; to integrate with the consumer.
///
/// DESERIALIZER CONTRACT:
///   T Deserialize(ReadOnlySpan&lt;byte&gt; data, bool isNull, SerializationContext context)
///   - data: The raw bytes from Kafka
///   - isNull: Whether the message value is null (tombstone message)
///   - Returns: The deserialized object
/// </summary>
public class JsonDeserializer<T> : IDeserializer<T>
{
    private static readonly JsonSerializerSettings _settings = new()
    {
        Converters = { new StringEnumConverter() },
        DateFormatHandling = DateFormatHandling.IsoDateFormat,
        NullValueHandling = NullValueHandling.Ignore,

        // IMPORTANT: In production, consider setting MissingMemberHandling
        // to handle schema evolution (new fields in serialized data
        // that don't exist in our current class definition)
        MissingMemberHandling = MissingMemberHandling.Ignore
    };

    /// <summary>
    /// Converts UTF-8 JSON bytes back to a typed object.
    /// </summary>
    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        if (isNull || data.IsEmpty)
            return default!;

        var json = Encoding.UTF8.GetString(data);

        // JsonConvert throws JsonException on malformed data.
        // In production, catch and send to dead letter queue.
        return JsonConvert.DeserializeObject<T>(json, _settings)
            ?? throw new JsonException($"Deserialization returned null for type {typeof(T).Name}");
    }
}

// =============================================================================
// PRODUCER USING CUSTOM SERIALIZATION
// =============================================================================

/// <summary>
/// Producer that sends strongly-typed objects using JSON serialization.
/// Demonstrates the power of custom serializers - no manual JSON conversion needed!
/// </summary>
public class CustomSerializationProducer
{
    private readonly string _bootstrapServers;
    private readonly string _topicName;

    public CustomSerializationProducer(string bootstrapServers, string topicName = "advanced-orders")
    {
        _bootstrapServers = bootstrapServers;
        _topicName = topicName;
    }

    /// <summary>
    /// Creates a producer configured with custom JSON serialization.
    ///
    /// KEY CONCEPT: IProducer&lt;OrderEvent, OrderEvent&gt;
    /// We can use the OrderEvent type directly as both key and value!
    /// The custom serializer handles the conversion automatically.
    ///
    /// More realistically you'd use:
    /// IProducer&lt;string, OrderEvent&gt; where Key=OrderId (string), Value=OrderEvent
    /// </summary>
    public async Task SendTypedMessageAsync(OrderEvent order)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = _bootstrapServers,
            Acks = Acks.All,
            EnableIdempotence = true,
            CompressionType = CompressionType.Snappy,
            ClientId = "kafka101-typed-producer"
        };

        // Build producer with custom serializers
        // SetKeySerializer: How to serialize the message KEY
        // SetValueSerializer: How to serialize the message VALUE
        using var producer = new ProducerBuilder<string, OrderEvent>(config)
            .SetValueSerializer(new JsonSerializer<OrderEvent>())
            // Key uses built-in string serializer (no need to set it explicitly)
            .Build();

        var message = new Message<string, OrderEvent>
        {
            Key = order.OrderId,    // string key
            Value = order,          // OrderEvent object - serialized automatically!
            Headers = new Headers
            {
                { "content-type", Encoding.UTF8.GetBytes("application/json") },
                { "producer-version", Encoding.UTF8.GetBytes("1.0.0") }
            }
        };

        var result = await producer.ProduceAsync(_topicName, message);
        Console.WriteLine($"✅ Typed message sent: {order.OrderId} → P{result.Partition}@{result.Offset}");
    }

    /// <summary>
    /// Consumer using matching custom deserializer.
    /// Shows the symmetric deserialize side.
    /// </summary>
    public void ConsumeTypedMessages(CancellationToken cancellationToken, int maxMessages = 5)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _bootstrapServers,
            GroupId = "typed-consumer-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        // Build consumer with custom deserializer
        using var consumer = new ConsumerBuilder<string, OrderEvent>(config)
            .SetValueDeserializer(new JsonDeserializer<OrderEvent>())
            // Handle deserialization errors (instead of crashing on bad data)
            .SetErrorHandler((_, error) =>
                Console.WriteLine($"⚠️  Consumer error: {error.Reason}"))
            .Build();

        consumer.Subscribe(_topicName);
        Console.WriteLine($"👂 Listening for typed OrderEvent messages on {_topicName}...\n");

        var count = 0;
        try
        {
            while (!cancellationToken.IsCancellationRequested && count < maxMessages)
            {
                var result = consumer.Consume(TimeSpan.FromSeconds(3));
                if (result == null || result.IsPartitionEOF) continue;

                count++;
                var order = result.Message.Value; // Already deserialized to OrderEvent!

                Console.WriteLine($"📦 Order received: {order}");
                Console.WriteLine($"   Items: {order.Items.Count}, Total: {order.TotalAmount:C}");
                Console.WriteLine($"   Shipping to: {order.ShippingAddress.City}, {order.ShippingAddress.Country}");
            }
        }
        finally
        {
            consumer.Close();
        }
    }

    /// <summary>Entry point for serialization demo.</summary>
    public static async Task RunDemoAsync(string bootstrapServers)
    {
        var demo = new CustomSerializationProducer(bootstrapServers);

        Console.WriteLine("=== ADVANCED: Custom Serialization Demo ===\n");

        Console.WriteLine("--- Sending typed OrderEvent objects ---");
        var orders = OrderEventFactory.CreateMany(3);
        foreach (var order in orders)
        {
            await demo.SendTypedMessageAsync(order);
        }

        Console.WriteLine("\n--- Consuming typed OrderEvent objects ---");
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        demo.ConsumeTypedMessages(cts.Token, maxMessages: 3);

        Console.WriteLine("\n✅ Custom Serialization demo complete!");
        Console.WriteLine("💡 Notice: No manual JSON.Deserialize needed in the consumer!");
    }
}
