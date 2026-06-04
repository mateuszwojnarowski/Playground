using AzureFunctionsFundamentals.Shared;

namespace AzureFunctionsFundamentals.Modules.CosmosDbServiceBusPipeline.Exercise;

public sealed class CosmosOrderEnricher(IProductRepository products, IOrderRepository orders)
{
    // TODO: Implement Cosmos-based order enrichment.
    // - Validate the incoming order and load the related product and customer data.
    // - Apply the enrichment and pricing rules described in README.md.
    // - Return the EnrichedOrder expected by the exercise tests.
    public async Task<EnrichedOrder> EnrichAsync(Order order, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("TODO: implement this method.");
    }
}
