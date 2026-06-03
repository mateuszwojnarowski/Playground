using AzureFunctionsFundamentals.Modules.CosmosDbServiceBusPipeline.Exercise;
using AzureFunctionsFundamentals.Shared;
using Xunit;

namespace CosmosDbServiceBusPipelineExercise.Tests;

public sealed class CosmosOrderEnricherTests
{
    [Fact]
    public async Task EnrichAsync_uses_product_and_customer_data()
    {
        var products = new FakeProductRepository(new ProductDocument { Id = "p1", Category = "peripherals", Name = "Keyboard", UnitPrice = 25m });
        var orders = new FakeOrderRepository(new Customer { Id = 42, Name = "Contoso", Tier = "Gold" });
        var enricher = new CosmosOrderEnricher(products, orders);

        EnrichedOrder enriched = await enricher.EnrichAsync(new Order { Id = "order-1", CustomerId = 42, Product = "Keyboard", Quantity = 2, UnitPrice = 10m });

        Assert.Equal("order-1", enriched.Id);
        Assert.Equal("Contoso", enriched.CustomerName);
        Assert.Equal("Gold", enriched.CustomerTier);
        Assert.Equal("Keyboard", enriched.Product);
        Assert.Equal(50m, enriched.Total);
        Assert.Equal("Keyboard", products.LastProductName);
        Assert.Equal(42, orders.LastCustomerId);
    }

    [Fact]
    public async Task EnrichAsync_falls_back_when_cosmos_reference_data_is_missing()
    {
        var enricher = new CosmosOrderEnricher(new FakeProductRepository(null), new FakeOrderRepository(null));

        EnrichedOrder enriched = await enricher.EnrichAsync(new Order { Id = "order-2", CustomerId = 9, Product = "Cable", Quantity = 3, UnitPrice = 4m });

        Assert.Equal("Customer 9", enriched.CustomerName);
        Assert.Equal("Unknown", enriched.CustomerTier);
        Assert.Equal("Cable", enriched.Product);
        Assert.Equal(12m, enriched.Total);
    }

    private sealed class FakeProductRepository(ProductDocument? product) : IProductRepository
    {
        public string? LastProductName { get; private set; }

        public Task<ProductDocument?> FindByNameAsync(string productName, CancellationToken cancellationToken = default)
        {
            LastProductName = productName;
            return Task.FromResult(product);
        }
    }

    private sealed class FakeOrderRepository(Customer? customer) : IOrderRepository
    {
        public int? LastCustomerId { get; private set; }

        public Task<Customer?> FindCustomerAsync(int customerId, CancellationToken cancellationToken = default)
        {
            LastCustomerId = customerId;
            return Task.FromResult(customer);
        }
    }
}
