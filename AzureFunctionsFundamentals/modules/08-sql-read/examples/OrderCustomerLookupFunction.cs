using System.Text.Json;
using AzureFunctionsFundamentals.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace SqlReadExample;

public sealed class OrderCustomerLookupFunction(ICustomerRepository customers, ILogger<OrderCustomerLookupFunction> logger)
{
    [Function(nameof(OrderCustomerLookupFunction))]
    public async Task RunAsync(
        [ServiceBusTrigger("orders", Connection = "ServiceBusConnection")] string message,
        CancellationToken cancellationToken)
    {
        Order order = JsonSerializer.Deserialize<Order>(message)
            ?? throw new InvalidOperationException("The Service Bus message did not contain a valid order.");

        logger.LogInformation("Looking up customer {CustomerId} for order {OrderId}.", order.CustomerId, order.Id);

        Customer? customer = await customers.GetByIdAsync(order.CustomerId, cancellationToken);
        if (customer is null)
        {
            logger.LogWarning("Customer {CustomerId} was not found for order {OrderId}.", order.CustomerId, order.Id);
            return;
        }

        logger.LogInformation("Order {OrderId} belongs to {CustomerName} ({CustomerTier}).", order.Id, customer.Name, customer.Tier);
    }
}
