using AzureFunctionsFundamentals.Shared;

namespace AzureFunctionsFundamentals.Modules.ServiceBusQueue;

public sealed class OrderConsumer
{
    private readonly OrderValidator _validator;

    public OrderConsumer(OrderValidator validator)
    {
        _validator = validator;
    }

    public Task<ProcessedOrder> ProcessAsync(Order order, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<string> errors = _validator.Validate(order);
        if (errors.Count > 0)
        {
            throw new OrderValidationException(errors);
        }

        return Task.FromResult(new ProcessedOrder(order.Id, order.CustomerId, order.Total));
    }
}

public sealed record ProcessedOrder(string OrderId, int CustomerId, decimal Total);
