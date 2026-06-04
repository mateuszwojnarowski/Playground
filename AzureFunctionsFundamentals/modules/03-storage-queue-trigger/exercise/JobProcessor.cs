using System.Text.Json;
using AzureFunctionsFundamentals.Shared;

namespace StorageQueueExercise;

public sealed class JobProcessor
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    // TODO: Implement queue message processing.
    // - Parse and validate the incoming job payload according to the module README.md.
    // - Build the processed job model with the expected totals, lane selection, and timestamp.
    // - Throw the documented exceptions for invalid messages.
    public ProcessedJob Process(string message)
    {
        throw new NotImplementedException("TODO: implement this method.");
    }

    // TODO: Implement JSON projection for processed jobs.
    // - Call Process(message) internally.
    // - Serialize the returned ProcessedJob using JsonOptions.
    // - See README.md for the expected JSON shape.
    public string ProcessToJson(string message)
    {
        throw new NotImplementedException("TODO: implement this method.");
    }

    // TODO: Implement CSV parsing for queue jobs.
    // - Parse the CSV payload into Order values using the rules in README.md.
    // - Validate headers and column values exactly as the exercise requires.
    // - Return the parsed orders for Process to consume.
    private static IEnumerable<Order> ParseCsv(string message)
    {
        throw new NotImplementedException("TODO: implement this method.");
    }
}

public sealed record ProcessedJob(
    string OrderId,
    int CustomerId,
    string Product,
    int Quantity,
    decimal UnitPrice,
    decimal Total,
    string FulfillmentLane,
    DateTimeOffset ProcessedAtUtc);
