using AzureFunctionsFundamentals.Shared;

namespace AzureFunctionsFundamentals.Modules.CosmosDbReadWrite.Exercise;

public sealed class OrderService(IOrderRepository repository)
{
    public async Task<SaveOrderResult> SaveAsync(Order? order, CancellationToken cancellationToken = default)
    {
        var errors = Validate(order);
        if (errors.Count > 0)
        {
            return SaveOrderResult.Invalid(errors);
        }

        Order normalized = order! with
        {
            Id = string.IsNullOrWhiteSpace(order.Id) ? Guid.NewGuid().ToString() : order.Id.Trim(),
            Product = order.Product.Trim()
        };

        bool existed = await repository.GetAsync(normalized.Id, normalized.CustomerId, cancellationToken) is not null;
        Order saved = await repository.UpsertAsync(normalized, cancellationToken);
        return SaveOrderResult.Saved(saved, !existed);
    }

    public async Task<QueryOrdersResult> QueryByCustomerAsync(int customerId, CancellationToken cancellationToken = default)
    {
        if (customerId <= 0)
        {
            return QueryOrdersResult.Invalid(["CustomerId must be greater than zero."]);
        }

        return QueryOrdersResult.Found(await repository.QueryByCustomerAsync(customerId, cancellationToken));
    }

    private static IReadOnlyList<string> Validate(Order? order)
    {
        if (order is null)
        {
            return ["Order payload is required."];
        }

        var errors = new List<string>();
        if (order.CustomerId <= 0) errors.Add("CustomerId must be greater than zero.");
        if (string.IsNullOrWhiteSpace(order.Product)) errors.Add("Product is required.");
        if (order.Quantity <= 0) errors.Add("Quantity must be greater than zero.");
        if (order.UnitPrice <= 0) errors.Add("UnitPrice must be greater than zero.");
        return errors;
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
