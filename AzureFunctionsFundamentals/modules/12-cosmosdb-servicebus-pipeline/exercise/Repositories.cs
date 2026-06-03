using System.Text.Json.Serialization;
using AzureFunctionsFundamentals.Shared;
using Microsoft.Azure.Cosmos;

namespace AzureFunctionsFundamentals.Modules.CosmosDbServiceBusPipeline.Exercise;

public interface IProductRepository
{
    Task<ProductDocument?> FindByNameAsync(string productName, CancellationToken cancellationToken = default);
}

public interface IOrderRepository
{
    Task<Customer?> FindCustomerAsync(int customerId, CancellationToken cancellationToken = default);
}

public sealed class CosmosProductRepository : IProductRepository
{
    private readonly Container _container;

    public CosmosProductRepository(CosmosClient client)
    {
        _container = client.GetContainer("LearningDb", "products");
    }

    public async Task<ProductDocument?> FindByNameAsync(string productName, CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition("SELECT TOP 1 * FROM c WHERE c.name = @name")
            .WithParameter("@name", productName);
        using FeedIterator<ProductDocument> iterator = _container.GetItemQueryIterator<ProductDocument>(query);
        while (iterator.HasMoreResults)
        {
            foreach (ProductDocument product in await iterator.ReadNextAsync(cancellationToken))
            {
                return product;
            }
        }

        return null;
    }
}

public sealed class CosmosOrderRepository : IOrderRepository
{
    private readonly Container _container;

    public CosmosOrderRepository(CosmosClient client)
    {
        _container = client.GetContainer("LearningDb", "audit");
    }

    public async Task<Customer?> FindCustomerAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition("SELECT TOP 1 VALUE { id: c.customerId, name: c.customerName, tier: c.customerTier } FROM c WHERE c.customerId = @customerId")
            .WithParameter("@customerId", customerId);
        using FeedIterator<Customer> iterator = _container.GetItemQueryIterator<Customer>(
            query,
            requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(customerId) });

        while (iterator.HasMoreResults)
        {
            foreach (Customer customer in await iterator.ReadNextAsync(cancellationToken))
            {
                return customer;
            }
        }

        return null;
    }
}

public sealed record ProductDocument
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
}
