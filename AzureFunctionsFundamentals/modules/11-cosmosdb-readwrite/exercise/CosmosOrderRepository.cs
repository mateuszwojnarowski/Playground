using AzureFunctionsFundamentals.Shared;
using Microsoft.Azure.Cosmos;

namespace AzureFunctionsFundamentals.Modules.CosmosDbReadWrite.Exercise;

public sealed class CosmosOrderRepository : IOrderRepository
{
    private readonly Container _container;

    public CosmosOrderRepository(CosmosClient client)
    {
        _container = client.GetContainer("LearningDb", "orders");
    }

    public async Task<Order?> GetAsync(string id, int customerId, CancellationToken cancellationToken = default)
    {
        try
        {
            ItemResponse<Order> response = await _container.ReadItemAsync<Order>(id, new PartitionKey(customerId), cancellationToken: cancellationToken);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<Order> UpsertAsync(Order order, CancellationToken cancellationToken = default)
    {
        ItemResponse<Order> response = await _container.UpsertItemAsync(order, new PartitionKey(order.CustomerId), cancellationToken: cancellationToken);
        return response.Resource;
    }

    public async Task<IReadOnlyList<Order>> QueryByCustomerAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.customerId = @customerId")
            .WithParameter("@customerId", customerId);
        using FeedIterator<Order> iterator = _container.GetItemQueryIterator<Order>(
            query,
            requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(customerId) });

        var orders = new List<Order>();
        while (iterator.HasMoreResults)
        {
            foreach (Order order in await iterator.ReadNextAsync(cancellationToken))
            {
                orders.Add(order);
            }
        }

        return orders;
    }
}
