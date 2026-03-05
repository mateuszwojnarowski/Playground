// =============================================================================
// Program.cs - Kafka101: Apache Kafka Learning Project Entry Point
// =============================================================================
//
// WELCOME TO KAFKA101!
//
// This is a comprehensive Apache Kafka learning project for C# developers.
// It covers Kafka concepts from basic message sending to advanced patterns
// like exactly-once semantics and custom serialization.
//
// PROJECT STRUCTURE:
//   Beginner/
//     SimpleProducer.cs    - Send your first Kafka message
//     SimpleConsumer.cs    - Read your first Kafka message
//   Intermediate/
//     PartitionedProducer.cs   - Message keys and partition routing
//     ConsumerGroupDemo.cs     - Multiple consumers sharing the load
//     OffsetManagement.cs      - Manual control over message delivery guarantees
//   Advanced/
//     CustomSerializationProducer.cs  - Send complex objects with custom serializers
//     ErrorHandlingConsumer.cs        - Retry logic and dead letter queues
//     PerformanceTuningProducer.cs    - Optimize for high throughput
//     TransactionalProducer.cs        - ACID guarantees across topics
//   Models/
//     OrderEvent.cs  - Sample e-commerce order domain model
//     UserEvent.cs   - Sample user activity/clickstream event model
//
// PREREQUISITES:
//   1. .NET 8 SDK (or later)
//   2. Kafka running locally (use docker-compose.yml at root of project)
//      > docker-compose up -d
//   3. Verify Kafka is healthy:
//      > docker ps (should show kafka and zookeeper containers)
//
// HOW TO RUN:
//   > cd Kafka101
//   > dotnet run
//   Then select a demo from the menu.
//
// LEARNING PATH (RECOMMENDED ORDER):
//   1. Start with Beginner demos to understand the core producer/consumer loop
//   2. Move to Intermediate to learn about partitions and delivery guarantees
//   3. Tackle Advanced demos once you're comfortable with the basics
//
// CONFIGURATION:
//   Kafka connection settings are in appsettings.json.
//   Default: localhost:9092 (standard local Kafka port)

using System.Text.Json;
using Kafka101.Beginner;
using Kafka101.Intermediate;
using Kafka101.Advanced;

// ─────────────────────────────────────────────────────────────────────────────
// CONFIGURATION LOADING
// Read Kafka settings from appsettings.json
// ─────────────────────────────────────────────────────────────────────────────
var configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
var bootstrapServers = "localhost:9092"; // Default if config file not found

if (File.Exists(configPath))
{
    try
    {
        var configJson = await File.ReadAllTextAsync(configPath);
        using var doc = JsonDocument.Parse(configJson);
        if (doc.RootElement.TryGetProperty("Kafka", out var kafka) &&
            kafka.TryGetProperty("BootstrapServers", out var servers))
        {
            bootstrapServers = servers.GetString() ?? bootstrapServers;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️  Could not read appsettings.json: {ex.Message}. Using defaults.");
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// MAIN MENU
// ─────────────────────────────────────────────────────────────────────────────
Console.OutputEncoding = System.Text.Encoding.UTF8;

PrintWelcomeBanner(bootstrapServers);

while (true)
{
    PrintMenu();
    var choice = Console.ReadLine()?.Trim() ?? "";

    if (choice == "0")
    {
        Console.WriteLine("\n👋 Goodbye! Happy Kafka learning!");
        break;
    }

    try
    {
        await ExecuteDemo(choice, bootstrapServers);
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n❌ Demo failed: {ex.GetType().Name}: {ex.Message}");
        Console.ResetColor();
        Console.WriteLine("\n💡 Make sure Kafka is running: docker-compose up -d");
        Console.WriteLine("   Check connection: " + bootstrapServers);
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }

    Console.WriteLine("\nPress any key to return to menu...");
    Console.ReadKey();
}

// ─────────────────────────────────────────────────────────────────────────────
// DEMO EXECUTION ROUTER
// ─────────────────────────────────────────────────────────────────────────────
static async Task ExecuteDemo(string choice, string bootstrapServers)
{
    Console.Clear();
    Console.WriteLine(new string('─', 60));

    switch (choice)
    {
        // BEGINNER DEMOS
        case "1":
            await SimpleProducer.RunDemoAsync(bootstrapServers);
            break;

        case "2":
            SimpleConsumer.RunDemo(bootstrapServers);
            break;

        // INTERMEDIATE DEMOS
        case "3":
            await PartitionedProducer.RunDemoAsync(bootstrapServers);
            break;

        case "4":
            await ConsumerGroupDemo.RunDemoAsync(bootstrapServers);
            break;

        case "5":
            OffsetManagement.RunDemo(bootstrapServers);
            break;

        // ADVANCED DEMOS
        case "6":
            await CustomSerializationProducer.RunDemoAsync(bootstrapServers);
            break;

        case "7":
            await ErrorHandlingConsumer.RunDemoAsync(bootstrapServers);
            break;

        case "8":
            await PerformanceTuningProducer.RunDemoAsync(bootstrapServers);
            break;

        case "9":
            await TransactionalProducer.RunDemoAsync(bootstrapServers);
            break;

        // FULL LEARNING PATH
        case "A":
        case "a":
            await RunFullBeginnerPath(bootstrapServers);
            break;

        case "B":
        case "b":
            await RunFullIntermediatePath(bootstrapServers);
            break;

        case "C":
        case "c":
            await RunFullAdvancedPath(bootstrapServers);
            break;

        default:
            Console.WriteLine($"❓ Unknown option: '{choice}'. Please try again.");
            break;
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// LEARNING PATH RUNNERS
// ─────────────────────────────────────────────────────────────────────────────

static async Task RunFullBeginnerPath(string bootstrapServers)
{
    Console.WriteLine("🎓 BEGINNER LEARNING PATH");
    Console.WriteLine("=".PadRight(60, '='));
    Console.WriteLine("This will run all beginner demos in sequence.\n");

    Console.WriteLine("STEP 1: Send messages as a producer");
    await SimpleProducer.RunDemoAsync(bootstrapServers);

    Console.WriteLine("\n\nSTEP 2: Read those messages as a consumer");
    SimpleConsumer.RunDemo(bootstrapServers);

    Console.WriteLine("\n✅ Beginner path complete! You can now produce and consume Kafka messages.");
    Console.WriteLine("   Next: Try the Intermediate path to learn about partitions and ordering.");
}

static async Task RunFullIntermediatePath(string bootstrapServers)
{
    Console.WriteLine("🎓 INTERMEDIATE LEARNING PATH");
    Console.WriteLine("=".PadRight(60, '='));

    Console.WriteLine("STEP 1: Partitioned producer with message keys");
    await PartitionedProducer.RunDemoAsync(bootstrapServers);

    Console.WriteLine("\n\nSTEP 2: Consumer groups and load balancing");
    await ConsumerGroupDemo.RunDemoAsync(bootstrapServers);

    Console.WriteLine("\n\nSTEP 3: Manual offset management");
    OffsetManagement.RunDemo(bootstrapServers);

    Console.WriteLine("\n✅ Intermediate path complete! You understand partitions and delivery guarantees.");
}

static async Task RunFullAdvancedPath(string bootstrapServers)
{
    Console.WriteLine("🎓 ADVANCED LEARNING PATH");
    Console.WriteLine("=".PadRight(60, '='));

    Console.WriteLine("STEP 1: Custom serialization");
    await CustomSerializationProducer.RunDemoAsync(bootstrapServers);

    Console.WriteLine("\n\nSTEP 2: Error handling and DLQ");
    await ErrorHandlingConsumer.RunDemoAsync(bootstrapServers);

    Console.WriteLine("\n\nSTEP 3: Performance tuning");
    await PerformanceTuningProducer.RunDemoAsync(bootstrapServers);

    Console.WriteLine("\n\nSTEP 4: Transactional producer");
    await TransactionalProducer.RunDemoAsync(bootstrapServers);

    Console.WriteLine("\n✅ Advanced path complete! You're now a Kafka power user!");
}

// ─────────────────────────────────────────────────────────────────────────────
// UI HELPERS
// ─────────────────────────────────────────────────────────────────────────────

static void PrintWelcomeBanner(string bootstrapServers)
{
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine(@"
  _  __      __ _          _ ___  ___ 
 | |/ /__ _ / _| | ____ _ / |   \/ _ \
 | ' // _` | |_| |/ / _` || | |) | | |
 | . \ (_| |  _|   < (_| || |___/| |_|
 |_|\_\__,_|_| |_|\_\__,_||_|    \___/
                                       
  Apache Kafka Learning Project for C#
");
    Console.ResetColor();
    Console.WriteLine($"  Kafka Broker: {bootstrapServers}");
    Console.WriteLine($"  .NET Version: {Environment.Version}");
    Console.WriteLine();
}

static void PrintMenu()
{
    Console.WriteLine();
    Console.WriteLine("─── KAFKA101 DEMO MENU ───────────────────────────────");
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("  🌱 BEGINNER LEVEL");
    Console.ResetColor();
    Console.WriteLine("   1. Simple Producer     - Send string messages to Kafka");
    Console.WriteLine("   2. Simple Consumer     - Read messages from Kafka");
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("  📈 INTERMEDIATE LEVEL");
    Console.ResetColor();
    Console.WriteLine("   3. Partitioned Producer - Message keys & partition routing");
    Console.WriteLine("   4. Consumer Groups      - Load balancing across consumers");
    Console.WriteLine("   5. Offset Management    - Delivery guarantee strategies");
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("  🚀 ADVANCED LEVEL");
    Console.ResetColor();
    Console.WriteLine("   6. Custom Serialization  - Strongly-typed message objects");
    Console.WriteLine("   7. Error Handling + DLQ  - Retry logic & dead letter queues");
    Console.WriteLine("   8. Performance Tuning    - High-throughput optimization");
    Console.WriteLine("   9. Transactional Producer- ACID guarantees across topics");
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("  📚 FULL LEARNING PATHS");
    Console.ResetColor();
    Console.WriteLine("   A. Run all Beginner demos");
    Console.WriteLine("   B. Run all Intermediate demos");
    Console.WriteLine("   C. Run all Advanced demos");
    Console.WriteLine();
    Console.WriteLine("   0. Exit");
    Console.WriteLine("─────────────────────────────────────────────────────");
    Console.Write("Select option: ");
}
