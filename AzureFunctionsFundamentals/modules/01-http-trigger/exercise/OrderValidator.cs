using AzureFunctionsFundamentals.Shared;

namespace AzureFunctionsFundamentals.Modules.HttpTrigger.Exercise;

public sealed class OrderValidator
{
    public IReadOnlyList<string> Validate(Order? order)
    {
        if (order is null)
        {
            return ["Order payload is required."];
        }

        var errors = new List<string>();

        if (order.CustomerId <= 0)
        {
            errors.Add("CustomerId must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(order.Product))
        {
            errors.Add("Product is required.");
        }

        if (order.Quantity <= 0)
        {
            errors.Add("Quantity must be greater than zero.");
        }

        if (order.UnitPrice <= 0)
        {
            errors.Add("UnitPrice must be greater than zero.");
        }

        return errors;
    }
}
