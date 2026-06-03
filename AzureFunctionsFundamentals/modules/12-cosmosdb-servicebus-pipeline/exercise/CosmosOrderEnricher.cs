using AzureFunctionsFundamentals.Shared;

namespace AzureFunctionsFundamentals.Modules.CosmosDbServiceBusPipeline.Exercise;

public sealed class CosmosOrderEnricher(IProductRepository products, IOrderRepository orders)
{
    public async Task<EnrichedOrder> EnrichAsync(Order order, CancellationToken cancellationToken = default)
    {
        if (order.CustomerId <= 0)
        {
            throw new ArgumentException("CustomerId must be greater than zero.", nameof(order));
        }

        ProductDocument? product = string.IsNullOrWhiteSpace(order.Product)
            ? null
            : await products.FindByNameAsync(order.Product.Trim(), cancellationToken);
        Customer customer = await orders.FindCustomerAsync(order.CustomerId, cancellationToken)
            ?? new Customer { Id = order.CustomerId, Name = $"Customer {order.CustomerId}", Tier = "Unknown" };

        decimal unitPrice = product is { UnitPrice: > 0 } ? product.UnitPrice : order.UnitPrice;

        return new EnrichedOrder
        {
            Id = string.IsNullOrWhiteSpace(order.Id) ? Guid.NewGuid().ToString() : order.Id,
            CustomerId = order.CustomerId,
            CustomerName = customer.Name,
            CustomerTier = customer.Tier,
            Product = product?.Name ?? order.Product.Trim(),
            Quantity = order.Quantity,
            Total = order.Quantity * unitPrice
        };
    }
}
