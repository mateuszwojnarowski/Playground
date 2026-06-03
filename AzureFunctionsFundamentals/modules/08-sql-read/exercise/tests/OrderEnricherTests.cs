using AzureFunctionsFundamentals.Modules.SqlRead;
using AzureFunctionsFundamentals.Shared;
using Xunit;

namespace SqlReadExercise.Tests;

public sealed class OrderEnricherTests
{
    [Fact]
    public async Task EnrichAsync_WhenCustomerExists_ReturnsEnrichedOrder()
    {
        var repository = new FakeCustomerRepository(new Customer { Id = 42, Name = "Ada Lovelace", Tier = "Gold" });
        var enricher = new OrderEnricher(repository);
        var order = new Order
        {
            Id = "order-801",
            CustomerId = 42,
            Product = "Keyboard",
            Quantity = 2,
            UnitPrice = 49.99m
        };

        EnrichedOrder enriched = await enricher.EnrichAsync(order);

        Assert.Equal("order-801", enriched.Id);
        Assert.Equal(42, enriched.CustomerId);
        Assert.Equal("Ada Lovelace", enriched.CustomerName);
        Assert.Equal("Gold", enriched.CustomerTier);
        Assert.Equal("Keyboard", enriched.Product);
        Assert.Equal(2, enriched.Quantity);
        Assert.Equal(99.98m, enriched.Total);
    }

    [Fact]
    public async Task EnrichAsync_WhenCustomerMissing_ThrowsCustomerNotFoundException()
    {
        var enricher = new OrderEnricher(new FakeCustomerRepository(null));
        var order = new Order
        {
            Id = "order-802",
            CustomerId = 99,
            Product = "Mouse",
            Quantity = 1,
            UnitPrice = 25m
        };

        CustomerNotFoundException exception = await Assert.ThrowsAsync<CustomerNotFoundException>(() => enricher.EnrichAsync(order));

        Assert.Equal(99, exception.CustomerId);
    }

    private sealed class FakeCustomerRepository : ICustomerRepository
    {
        private readonly Customer? _customer;

        public FakeCustomerRepository(Customer? customer)
        {
            _customer = customer;
        }

        public Task<Customer?> GetByIdAsync(int customerId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_customer?.Id == customerId ? _customer : null);
        }
    }
}
