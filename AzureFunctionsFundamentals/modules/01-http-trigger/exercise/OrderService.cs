using AzureFunctionsFundamentals.Shared;

namespace AzureFunctionsFundamentals.Modules.HttpTrigger.Exercise;

public sealed record OrderCreationResult(bool IsValid, Order? Order, IReadOnlyList<string> Errors)
{
    public static OrderCreationResult Created(Order order) => new(true, order, []);
    public static OrderCreationResult Invalid(IReadOnlyList<string> errors) => new(false, null, errors);
}

public sealed class OrderService(OrderValidator validator)
{
    // TODO: Implement order creation.
    // - Validate the payload with OrderValidator and return Invalid(...) when errors exist.
    // - Normalize the order data and assign an id when one is missing.
    // - Return the created order exactly as described in README.md for this module.
    public OrderCreationResult Create(Order? order)
    {
        throw new NotImplementedException("TODO: implement this method.");
    }
}
