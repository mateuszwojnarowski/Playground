using AzureFunctionsFundamentals.Shared;

namespace AzureFunctionsFundamentals.Modules.ServiceBusQueue;

public sealed class OrderConsumer
{
    private readonly OrderValidator _validator;

    public OrderConsumer(OrderValidator validator)
    {
        _validator = validator;
    }

    // TODO: Implement queue order consumption.
    // - Respect cancellation, validate the order, and throw OrderValidationException when needed.
    // - Return the processed order summary expected by the exercise tests.
    // - Use README.md for the full acceptance criteria.
    public Task<ProcessedOrder> ProcessAsync(Order order, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("TODO: implement this method.");
    }
}

public sealed record ProcessedOrder(string OrderId, int CustomerId, decimal Total);
