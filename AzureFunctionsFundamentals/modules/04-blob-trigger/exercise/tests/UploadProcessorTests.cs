using System.Text.Json;
using BlobTriggerExercise;
using Xunit;

namespace BlobTriggerExercise.Tests;

public sealed class UploadProcessorTests
{
    private const string SampleCsv = """
        id,customerId,product,quantity,unitPrice
        order-2001,42,Keyboard,2,49.99
        order-2002,7,Mouse,1,25.50
        """;

    [Fact]
    public void Transform_ParsesCsvAndCalculatesTotals()
    {
        var processor = new UploadProcessor();

        var result = processor.Transform(SampleCsv, "orders.csv");

        Assert.Equal("orders.csv", result.SourceFile);
        Assert.Equal(2, result.OrderCount);
        Assert.Equal(125.48m, result.TotalRevenue);
        Assert.Contains(result.Orders, order => order.Id == "order-2001" && order.Total == 99.98m);
    }

    [Fact]
    public void TransformToJson_ReturnsNormalizedJson()
    {
        var processor = new UploadProcessor();

        var json = processor.TransformToJson(SampleCsv, "orders.csv");

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal("orders.csv", root.GetProperty("sourceFile").GetString());
        Assert.Equal(2, root.GetProperty("orderCount").GetInt32());
        Assert.Equal(125.48m, root.GetProperty("totalRevenue").GetDecimal());
        Assert.Equal("Keyboard", root.GetProperty("orders")[0].GetProperty("product").GetString());
    }

    [Fact]
    public void Transform_RejectsUnexpectedHeader()
    {
        var processor = new UploadProcessor();

        var exception = Assert.Throws<InvalidOperationException>(() => processor.Transform("bad,header\n1,2", "bad.csv"));

        Assert.Contains("CSV header", exception.Message);
    }
}
