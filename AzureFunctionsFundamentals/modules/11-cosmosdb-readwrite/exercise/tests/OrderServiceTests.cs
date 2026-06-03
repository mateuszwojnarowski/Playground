using AzureFunctionsFundamentals.Modules.CosmosDbReadWrite.Exercise;
using AzureFunctionsFundamentals.Shared;
using Xunit;

namespace CosmosDbReadWriteExercise.Tests;

public sealed class OrderServiceTests
{
    [Fact]
    public async Task SaveAsync_creates_new_order_when_id_is_missing()
    {
        var repository = new FakeOrderRepository();
        var service = new OrderService(repository);

        SaveOrderResult result = await service.SaveAsync(new Order { CustomerId = 7, Product = "  Mouse  ", Quantity = 1, UnitPrice = 10m });

        Assert.True(result.IsValid);
        Assert.True(result.Created);
        Assert.False(string.IsNullOrWhiteSpace(result.Order!.Id));
        Assert.Equal("Mouse", result.Order.Product);
        Assert.Single(repository.Saved);
    }

    [Fact]
    public async Task SaveAsync_updates_existing_order()
    {
        var repository = new FakeOrderRepository();
        repository.Seed(new Order { Id = "order-9", CustomerId = 7, Product = "Mouse", Quantity = 1, UnitPrice = 10m });
        var service = new OrderService(repository);

        SaveOrderResult result = await service.SaveAsync(new Order { Id = "order-9", CustomerId = 7, Product = "Mouse", Quantity = 3, UnitPrice = 10m });

        Assert.True(result.IsValid);
        Assert.False(result.Created);
        Assert.Equal(3, result.Order!.Quantity);
        Assert.Single(repository.Saved);
    }

    [Fact]
    public async Task QueryByCustomerAsync_uses_repository_partition_query()
    {
        var repository = new FakeOrderRepository();
        repository.Seed(new Order { Id = "a", CustomerId = 1, Product = "A", Quantity = 1, UnitPrice = 1m });
        repository.Seed(new Order { Id = "b", CustomerId = 2, Product = "B", Quantity = 1, UnitPrice = 1m });
        var service = new OrderService(repository);

        QueryOrdersResult result = await service.QueryByCustomerAsync(2);

        Assert.True(result.IsValid);
        Assert.Single(result.Orders);
        Assert.Equal("b", result.Orders[0].Id);
        Assert.Equal(2, repository.LastQueriedCustomerId);
    }

    private sealed class FakeOrderRepository : IOrderRepository
    {
        private readonly Dictionary<(string Id, int CustomerId), Order> _orders = [];
        public List<Order> Saved { get; } = [];
        public int? LastQueriedCustomerId { get; private set; }

        public void Seed(Order order) => _orders[(order.Id, order.CustomerId)] = order;

        public Task<Order?> GetAsync(string id, int customerId, CancellationToken cancellationToken = default)
        {
            _orders.TryGetValue((id, customerId), out Order? order);
            return Task.FromResult(order);
        }

        public Task<Order> UpsertAsync(Order order, CancellationToken cancellationToken = default)
        {
            _orders[(order.Id, order.CustomerId)] = order;
            Saved.Add(order);
            return Task.FromResult(order);
        }

        public Task<IReadOnlyList<Order>> QueryByCustomerAsync(int customerId, CancellationToken cancellationToken = default)
        {
            LastQueriedCustomerId = customerId;
            IReadOnlyList<Order> orders = _orders.Values.Where(o => o.CustomerId == customerId).ToArray();
            return Task.FromResult(orders);
        }
    }
}
