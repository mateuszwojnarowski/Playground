using System.Text.Json;
using AzureFunctionsFundamentals.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace SqlReadExample;

public sealed class OrderCustomerLookupFunction
{
    private readonly ICustomerRepository _customers;
    private readonly ILogger<OrderCustomerLookupFunction> _logger;

    public OrderCustomerLookupFunction(ICustomerRepository customers, ILogger<OrderCustomerLookupFunction> logger)
    {
        _customers = customers;
        _logger = logger;
    }

    [Function(nameof(OrderCustomerLookupFunction))]
    public async Task RunAsync(
        [ServiceBusTrigger("orders", Connection = "ServiceBusConnection")] string message,
        CancellationToken cancellationToken)
    {
        Order order = JsonSerializer.Deserialize<Order>(message)
            ?? throw new InvalidOperationException("The Service Bus message did not contain a valid order.");

        Customer? customer = await _customers.GetByIdAsync(order.CustomerId, cancellationToken);
        if (customer is null)
        {
            _logger.LogWarning("Customer {CustomerId} was not found for order {OrderId}.", order.CustomerId, order.Id);
            return;
        }

        _logger.LogInformation("Order {OrderId} belongs to {CustomerName} ({CustomerTier}).", order.Id, customer.Name, customer.Tier);
    }
}
