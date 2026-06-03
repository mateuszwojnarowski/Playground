using AzureFunctionsFundamentals.Shared;

namespace AzureFunctionsFundamentals.Modules.SqlRead;

public sealed class OrderEnricher
{
    private readonly ICustomerRepository _customers;

    public OrderEnricher(ICustomerRepository customers)
    {
        _customers = customers;
    }

    public async Task<EnrichedOrder> EnrichAsync(Order order, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(order);

        Customer customer = await _customers.GetByIdAsync(order.CustomerId, cancellationToken)
            ?? throw new CustomerNotFoundException(order.CustomerId);

        return new EnrichedOrder
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            CustomerName = customer.Name,
            CustomerTier = customer.Tier,
            Product = order.Product,
            Quantity = order.Quantity,
            Total = order.Total
        };
    }
}

public sealed class CustomerNotFoundException : Exception
{
    public CustomerNotFoundException(int customerId)
        : base($"Customer {customerId} was not found.")
    {
        CustomerId = customerId;
    }

    public int CustomerId { get; }
}
