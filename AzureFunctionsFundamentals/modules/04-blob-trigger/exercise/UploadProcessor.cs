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

    public ProcessedUpload Transform(string content, string fileName)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Upload content must not be empty.", nameof(content));
        }

        var orders = ParseCsv(content).ToArray();
        if (orders.Length == 0)
        {
            throw new InvalidOperationException("Upload did not contain any orders.");
        }

        return new ProcessedUpload(
            fileName,
            orders.Length,
            orders.Sum(order => order.Total),
            orders,
            DateTimeOffset.UtcNow);
    }

    public string TransformToJson(string content, string fileName)
    {
        var result = Transform(content, fileName);
        return JsonSerializer.Serialize(result, JsonOptions);
    }

    private static IEnumerable<Order> ParseCsv(string content)
    {
        using var reader = new StringReader(content);
        var header = reader.ReadLine();
        if (!string.Equals(header?.Trim(), "id,customerId,product,quantity,unitPrice", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("CSV header must be: id,customerId,product,quantity,unitPrice");
        }

        var lineNumber = 1;
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var columns = line.Split(',');
            if (columns.Length != 5)
            {
                throw new InvalidOperationException($"Line {lineNumber} must contain 5 columns.");
            }

            yield return new Order
            {
                Id = columns[0].Trim(),
                CustomerId = int.Parse(columns[1], CultureInfo.InvariantCulture),
                Product = columns[2].Trim(),
                Quantity = int.Parse(columns[3], CultureInfo.InvariantCulture),
                UnitPrice = decimal.Parse(columns[4], CultureInfo.InvariantCulture)
            };
        }
    }
}

public sealed record ProcessedUpload(
    string SourceFile,
    int OrderCount,
    decimal TotalRevenue,
    IReadOnlyCollection<Order> Orders,
    DateTimeOffset ProcessedAtUtc);
