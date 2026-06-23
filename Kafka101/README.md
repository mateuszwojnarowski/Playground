# Kafka101: Apache Kafka Learning Project for C#

A comprehensive, heavily-commented Apache Kafka learning project covering all complexity levels — from sending your first message to implementing exactly-once transactional producers.

## 🎯 Learning Objectives

By working through this project you will understand:

- **Beginner**: What Kafka is, how to produce and consume messages, basic configuration
- **Intermediate**: Partitions, consumer groups, message keys, offset management, delivery semantics
- **Advanced**: Custom serialization, error handling with DLQ, performance tuning, ACID transactions

---

## 📋 Prerequisites

### Required Software

| Software | Version | Download |
|----------|---------|----------|
| .NET SDK | 8.0+ | [dotnet.microsoft.com](https://dotnet.microsoft.com/download) |
| Docker Desktop | Latest | [docker.com](https://www.docker.com/products/docker-desktop/) |

### Verify Your Setup

```bash
# Check .NET version (should be 8.0 or later)
dotnet --version

# Check Docker is running
docker --version
docker ps
```

---

## 🚀 Quick Start

### Step 1: Start Kafka with Docker

```bash
# From the Kafka101 directory:
cd Kafka101

# Start Kafka and Zookeeper in the background
docker-compose up -d

# Verify containers are healthy (wait ~30 seconds after starting)
docker ps

# Expected output shows 3 containers running:
# kafka101-broker      (Kafka)
# kafka101-zookeeper   (ZooKeeper)
# kafka101-ui          (Kafka UI)
```

### Step 2: Verify Kafka is Ready

```bash
# Option A: Check Kafka UI at http://localhost:8080
# Option B: Test connection via command line
docker exec kafka101-broker kafka-topics \
  --bootstrap-server localhost:9092 \
  --list
```

### Step 3: Run the Learning Project

```bash
# Navigate to the project
cd Kafka101/Kafka101

# Run the interactive demo menu
dotnet run
```

You'll see an interactive menu where you can select any demo to run.

---

## 📁 Project Structure

```
Kafka101/
├── docker-compose.yml          # Local Kafka infrastructure
├── README.md                   # This file
└── Kafka101/                   # .NET console application
    ├── Kafka101.csproj         # Project file (Confluent.Kafka + Newtonsoft.Json)
    ├── Program.cs              # Entry point with interactive demo menu
    ├── appsettings.json        # Kafka connection configuration
    │
    ├── Models/                 # Sample domain models for realistic examples
    │   ├── OrderEvent.cs       # E-commerce order event (with factory)
    │   └── UserEvent.cs        # User activity/clickstream event (with factory)
    │
    ├── Beginner/               # 🌱 Start here!
    │   ├── SimpleProducer.cs   # Send string messages to Kafka
    │   └── SimpleConsumer.cs   # Read messages from Kafka
    │
    ├── Intermediate/           # 📈 Once you're comfortable with basics
    │   ├── PartitionedProducer.cs  # Keys, partitions, ordering guarantees
    │   ├── ConsumerGroupDemo.cs    # Load balancing across multiple consumers
    │   └── OffsetManagement.cs     # at-least-once, at-most-once, seek/replay
    │
    └── Advanced/               # 🚀 Production-grade patterns
        ├── CustomSerializationProducer.cs  # Typed messages with JSON serializers
        ├── ErrorHandlingConsumer.cs        # Retry + Dead Letter Queue
        ├── PerformanceTuningProducer.cs    # Benchmarking and optimization
        └── TransactionalProducer.cs        # Exactly-once, multi-topic atomicity
```

---

## 📚 Learning Path

### 🌱 Beginner Level

**Concepts**: Topics, brokers, producers, consumers, the poll loop, auto-offset-reset

#### SimpleProducer.cs
```
What you'll learn:
✓ What Kafka is and why it's used
✓ How to configure a ProducerConfig
✓ ProduceAsync (recommended) vs Produce (fire-and-forget)
✓ Reading DeliveryResult (topic, partition, offset)
✓ Why you MUST call Flush() with fire-and-forget
✓ Different acknowledgment modes (Acks.None, Leader, All)
```

#### SimpleConsumer.cs
```
What you'll learn:
✓ Consumer groups and the GroupId setting
✓ AutoOffsetReset.Earliest vs Latest
✓ The poll loop (Consume with timeout)
✓ EnableAutoCommit and its risks
✓ Graceful shutdown with CancellationToken
✓ Why consumer.Close() matters more than Dispose()
```

#### Try it yourself:
```bash
# Terminal 1: Start the consumer first
dotnet run
# Select option 2 (Simple Consumer)

# Terminal 2: Send messages
dotnet run
# Select option 1 (Simple Producer)
```

---

### 📈 Intermediate Level

**Concepts**: Partitions, message keys, consumer groups, offset management

#### PartitionedProducer.cs
```
What you'll learn:
✓ How partitions enable parallelism and scalability
✓ Why message keys guarantee ordering (same key → same partition)
✓ The Murmur2 hashing algorithm for key-to-partition mapping
✓ Idempotent producers (no duplicates on retry)
✓ Batching with LingerMs and BatchNumMessages
✓ Compression types (Snappy, Gzip, LZ4, Zstd)
```

#### ConsumerGroupDemo.cs
```
What you'll learn:
✓ How partitions are distributed across consumer group members
✓ The "golden rule": consumers ≤ partitions for active consumption
✓ Rebalancing: what happens when consumers join/leave
✓ CooperativeSticky assignment strategy
✓ OnPartitionsAssigned / OnPartitionsRevoked callbacks
✓ Why you must commit during OnPartitionsRevoked
```

#### OffsetManagement.cs
```
What you'll learn:
✓ What an offset is and how Kafka uses it
✓ At-least-once delivery (commit after processing)
✓ At-most-once delivery (commit before processing)
✓ Exactly-once preview (see Advanced for full implementation)
✓ Seeking to specific offsets for replay
✓ StoreOffset vs Commit for batch commit patterns
```

---

### 🚀 Advanced Level

**Concepts**: Custom serialization, error handling, performance tuning, transactions

#### CustomSerializationProducer.cs
```
What you'll learn:
✓ Why custom serializers are needed for complex objects
✓ Implementing ISerializer<T> and IDeserializer<T>
✓ JSON serialization settings for Kafka (enum handling, null values)
✓ Typed producers: IProducer<string, OrderEvent>
✓ Schema evolution considerations
✓ When to use JSON vs Avro vs Protobuf
```

#### ErrorHandlingConsumer.cs
```
What you'll learn:
✓ Classifying errors: transient vs permanent
✓ Exponential backoff with jitter
✓ The Dead Letter Queue (DLQ) pattern
✓ Enriching DLQ messages with diagnostic metadata
✓ Fatal vs non-fatal consumer errors
✓ Why you must always commit after sending to DLQ
```

#### PerformanceTuningProducer.cs
```
What you'll learn:
✓ Default vs optimized throughput (5-10x improvement)
✓ LingerMs and BatchNumMessages for batching
✓ Compression reduces network/storage by 4-6x for JSON
✓ QueueBufferingMaxMessages for burst scenarios
✓ Benchmarking methodology
✓ Small vs large message strategies
```

#### TransactionalProducer.cs
```
What you'll learn:
✓ ACID transactions across multiple Kafka topics
✓ TransactionalId and producer fencing
✓ The Read-Process-Write (consume-transform-produce) pattern
✓ IsolationLevel.ReadCommitted for transaction-aware consumers
✓ SendOffsetsToTransaction for atomic offset + message commit
✓ AbortTransaction for rollback
✓ When exactly-once is worth the overhead
```

---

## ⚙️ Configuration

Edit `Kafka101/appsettings.json` to change the Kafka broker address:

```json
{
  "Kafka": {
    "BootstrapServers": "localhost:9092"
  }
}
```

### Connecting to Different Kafka Environments

| Environment | BootstrapServers value |
|-------------|----------------------|
| Local Docker | `localhost:9092` |
| Multiple brokers | `broker1:9092,broker2:9092,broker3:9092` |
| Confluent Cloud | `pkc-xxxxx.region.provider.confluent.cloud:9092` |
| Amazon MSK | `b-1.cluster.xxxxx.kafka.region.amazonaws.com:9092` |
| Azure Event Hubs | `namespace.servicebus.windows.net:9093` |

### Confluent Cloud Configuration

For Confluent Cloud, add security settings:

```json
{
  "Kafka": {
    "BootstrapServers": "pkc-xxxxx.region.provider.confluent.cloud:9092",
    "Security": {
      "SecurityProtocol": "SaslSsl",
      "SaslMechanism": "Plain",
      "SaslUsername": "YOUR_API_KEY",
      "SaslPassword": "YOUR_API_SECRET"
    }
  }
}
```

---

## 🔧 Kafka UI

Access the Kafka UI at **http://localhost:8080** after starting Docker.

The UI lets you:
- **Browse topics** and their partitions
- **View messages** with offset and timestamp
- **Monitor consumer group lag** (how far behind consumers are)
- **Create and delete topics**
- **Publish test messages** without writing code

---

## 🐛 Troubleshooting

### "Connection refused" or "broker not available"

```bash
# Check if Docker containers are running
docker ps

# If not running, start them
docker-compose up -d

# Check Kafka logs for errors
docker logs kafka101-broker --tail=50

# Wait 30 seconds for Kafka to fully initialize after starting
```

### "Topic auto-creation is disabled"

```bash
# Create topics manually
docker exec kafka101-broker kafka-topics \
  --bootstrap-server localhost:9092 \
  --create --topic beginner-demo \
  --partitions 3 \
  --replication-factor 1
```

### Consumer not receiving messages

```bash
# Check if consumer group has any offset
docker exec kafka101-broker kafka-consumer-groups \
  --bootstrap-server localhost:9092 \
  --describe --group beginner-consumer-group
```

Check `AutoOffsetReset`:
- `AutoOffsetReset.Earliest`: Reads from the beginning (good for demos)
- `AutoOffsetReset.Latest`: Only reads NEW messages (misses already-sent messages)

### "Leader not available" error

This usually means Kafka just started and is still electing partition leaders. Wait 15-30 seconds and retry.

### High consumer lag

```bash
# View consumer group lag
docker exec kafka101-broker kafka-consumer-groups \
  --bootstrap-server localhost:9092 \
  --describe --group intermediate-consumer-group
```

If lag is high, increase the number of consumer instances (up to number of partitions).

### Out of memory with Docker

Increase Docker Desktop memory to at least 4GB:
Docker Desktop → Settings → Resources → Memory → 4GB+

---

## 🧹 Cleanup

```bash
# Stop containers (data preserved)
docker-compose down

# Stop and DELETE all data (fresh start)
docker-compose down -v

# Remove Docker images
docker-compose down --rmi all -v
```

---

## 📖 Key Kafka Concepts Quick Reference

### Core Terminology

| Term | Definition |
|------|-----------|
| **Broker** | A Kafka server that stores messages and serves clients |
| **Topic** | A named category of messages (like a database table) |
| **Partition** | A subdivided log within a topic for parallelism |
| **Offset** | Sequential ID of a message within a partition |
| **Producer** | Application that writes messages to Kafka |
| **Consumer** | Application that reads messages from Kafka |
| **Consumer Group** | Set of consumers sharing topic consumption |
| **Replication** | Copying partitions across brokers for fault tolerance |

### Delivery Semantics

| Semantic | Behavior | How to Achieve | Use Case |
|----------|----------|----------------|----------|
| At-most-once | May lose messages | Commit before processing | Metrics, logs |
| At-least-once | May duplicate messages | Commit after processing | Most use cases |
| Exactly-once | No loss, no duplicates | Transactions + idempotent | Financial, critical data |

### Partition Selection Rules

| Key Present? | Rule | Partition |
|-------------|------|-----------|
| Yes | `abs(murmur2(key)) % numPartitions` | Deterministic |
| No | Round-robin | Changes each message |
| Manual | `TopicPartition(topic, n)` | Explicit |

---

## 📚 Additional Learning Resources

### Official Documentation
- [Apache Kafka Documentation](https://kafka.apache.org/documentation/)
- [Confluent.Kafka .NET Client](https://docs.confluent.io/kafka-clients/dotnet/current/overview.html)
- [Confluent Developer Portal](https://developer.confluent.io/)

### Books
- *Kafka: The Definitive Guide* by Neha Narkhede, Gwen Shapira, and Todd Palino
- *Designing Event-Driven Systems* by Ben Stopford (free PDF at Confluent)

### Courses
- [Confluent Learn](https://developer.confluent.io/learn-kafka/) - Free interactive courses
- [Apache Kafka for Beginners](https://www.udemy.com/course/apache-kafka/) by Stephane Maarek

### Tools
- [Kafka Tool](https://www.kafkatool.com/) - GUI client for Kafka
- [kcat (kafkacat)](https://github.com/edenhill/kcat) - CLI tool for producing/consuming
- [Conduktor](https://www.conduktor.io/) - Advanced Kafka management platform

---

## 🏗️ Building for Production

When moving beyond this learning project, consider:

1. **Use a Schema Registry** (Confluent or Apicurio) for Avro/Protobuf schema management
2. **Configure proper authentication**: SASL/PLAIN, SASL/GSSAPI (Kerberos), mTLS
3. **Set `auto.create.topics.enable=false`** on production brokers
4. **Use at least 3 brokers** for high availability
5. **Set replication factor ≥ 3** for critical topics
6. **Monitor consumer group lag** with Prometheus + Grafana
7. **Implement health checks** that verify Kafka connectivity
8. **Use Kubernetes** for deploying consumer services (Kafka scales horizontally)

---

*Happy learning! Start with the Beginner demos and work your way up.  
Questions? The Confluent Community Slack is a great resource: [launchpass.com/confluentcommunity](https://launchpass.com/confluentcommunity)*
