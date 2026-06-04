using AzureFunctionsFundamentals.Shared;

namespace AzureFunctionsFundamentals.Modules.ServiceBusQueue;

public sealed class OrderValidator
{
    // TODO: Implement Service Bus order validation.
    // - Validate the order id, customer id, product, quantity, and unit price.
    // - Return every validation error collected for the incoming message.
    // - Follow the exercise rules in README.md for this module.
    public IReadOnlyList<string> Validate(Order order)
    {
        throw new NotImplementedException("TODO: implement this method.");
    }
}
