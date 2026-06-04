using AzureFunctionsFundamentals.Shared;

namespace AzureFunctionsFundamentals.Modules.SqlRead;

public sealed class OrderEnricher
{
    private readonly ICustomerRepository _customers;

    public OrderEnricher(ICustomerRepository customers)
    {
        _customers = customers;
    }

    // TODO: Implement order enrichment.
    // - Validate the incoming order and load the matching customer from the repository.
    // - Throw CustomerNotFoundException when the customer cannot be found.
    // - Return the enriched order described in README.md for this module.
    public async Task<EnrichedOrder> EnrichAsync(Order order, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("TODO: implement this method.");
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
