using AzureFunctionsFundamentals.Shared;

namespace AzureFunctionsFundamentals.Modules.HttpTrigger.Exercise;

public sealed class OrderValidator
{
    // TODO: Implement order validation.
    // - Return validation errors for null, CustomerId <= 0, empty Product, Quantity <= 0, and UnitPrice <= 0.
    // - Collect every validation error instead of stopping at the first one.
    // - Follow the acceptance criteria in README.md for this module.
    public IReadOnlyList<string> Validate(Order? order)
    {
        throw new NotImplementedException("TODO: implement this method.");
    }
}
