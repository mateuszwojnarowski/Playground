using System.Globalization;
using System.Text.Json;
using AzureFunctionsFundamentals.Shared;

namespace BlobTriggerExercise;

public sealed class UploadProcessor
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    // TODO: Implement blob upload transformation.
    // - Parse and validate the CSV content according to README.md.
    // - Build the ProcessedUpload summary with order count, revenue total, parsed orders, and timestamp.
    // - Throw the documented exceptions when the upload is invalid.
    public ProcessedUpload Transform(string content, string fileName)
    {
        throw new NotImplementedException("TODO: implement this method.");
    }

    // TODO: Implement JSON projection for transformed uploads.
    // - Call Transform(content, fileName) internally.
    // - Serialize the returned ProcessedUpload using JsonOptions.
    // - See README.md for the expected JSON shape.
    public string TransformToJson(string content, string fileName)
    {
        throw new NotImplementedException("TODO: implement this method.");
    }

    // TODO: Implement CSV parsing for blob uploads.
    // - Validate the CSV header and parse each row into an Order.
    // - Enforce the parsing rules and error cases described in README.md.
    // - Return the parsed orders for Transform to consume.
    private static IEnumerable<Order> ParseCsv(string content)
    {
        throw new NotImplementedException("TODO: implement this method.");
    }
}

public sealed record ProcessedUpload(
    string SourceFile,
    int OrderCount,
    decimal TotalRevenue,
    IReadOnlyCollection<Order> Orders,
    DateTimeOffset ProcessedAtUtc);
