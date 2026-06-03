using AzureFunctionsFundamentals.Shared;

namespace AzureFunctionsFundamentals.Modules.HttpTrigger.Exercise;

public sealed record OrderCreationResult(bool IsValid, Order? Order, IReadOnlyList<string> Errors)
{
    public static OrderCreationResult Created(Order order) => new(true, order, []);
    public static OrderCreationResult Invalid(IReadOnlyList<string> errors) => new(false, null, errors);
}

public sealed class OrderService(OrderValidator validator)
{
    public OrderCreationResult Create(Order? order)
    {
        var errors = validator.Validate(order);
        if (errors.Count > 0)
        {
            return OrderCreationResult.Invalid(errors);
        }

        var created = order! with
        {
            Id = string.IsNullOrWhiteSpace(order.Id) ? Guid.NewGuid().ToString() : order.Id,
            Product = order.Product.Trim()
        };

        return OrderCreationResult.Created(created);
    }
}
