using AzureFunctionsFundamentals.Shared;

namespace AzureFunctionsFundamentals.Modules.ServiceBusQueue;

public sealed class OrderValidator
{
    public IReadOnlyList<string> Validate(Order order)
    {
        List<string> errors = [];

        if (string.IsNullOrWhiteSpace(order.Id))
        {
            errors.Add("Order id is required.");
        }

        if (order.CustomerId <= 0)
        {
            errors.Add("Customer id must be greater than zero.");
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
            errors.Add("Unit price must be greater than zero.");
        }

        return errors;
    }
}
