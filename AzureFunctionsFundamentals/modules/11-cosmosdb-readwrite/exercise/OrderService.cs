using AzureFunctionsFundamentals.Shared;

namespace AzureFunctionsFundamentals.Modules.CosmosDbReadWrite.Exercise;

public sealed class OrderService(IOrderRepository repository)
{
    // TODO: Implement order persistence.
    // - Validate and normalize the incoming order.
    // - Upsert through the repository and report whether the order was newly created.
    // - Match the save behavior documented in README.md for this module.
    public async Task<SaveOrderResult> SaveAsync(Order? order, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("TODO: implement this method.");
    }

    // TODO: Implement customer order queries.
    // - Validate the customer id and return Invalid(...) for bad input.
    // - Query the repository and return the expected QueryOrdersResult.
    // - Follow README.md for this module.
    public async Task<QueryOrdersResult> QueryByCustomerAsync(int customerId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("TODO: implement this method.");
    }

    // TODO: Implement order validation.
    // - Return the validation errors required by README.md when the payload is missing or invalid.
    // - Collect all relevant validation errors for SaveAsync.
    private static IReadOnlyList<string> Validate(Order? order)
    {
        throw new NotImplementedException("TODO: implement this method.");
    }
}

public sealed record SaveOrderResult(bool IsValid, bool Created, Order? Order, IReadOnlyList<string> Errors)
{
    public static SaveOrderResult Saved(Order order, bool created) => new(true, created, order, []);
    public static SaveOrderResult Invalid(IReadOnlyList<string> errors) => new(false, false, null, errors);
}

public sealed record QueryOrdersResult(bool IsValid, IReadOnlyList<Order> Orders, IReadOnlyList<string> Errors)
{
    public static QueryOrdersResult Found(IReadOnlyList<Order> orders) => new(true, orders, []);
    public static QueryOrdersResult Invalid(IReadOnlyList<string> errors) => new(false, [], errors);
}
