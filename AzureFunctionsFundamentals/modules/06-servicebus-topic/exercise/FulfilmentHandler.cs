using AzureFunctionsFundamentals.Shared;

namespace AzureFunctionsFundamentals.Modules.ServiceBusTopic;

public sealed class FulfilmentHandler
{
    public FulfilmentDecision Decide(Order order)
    {
        if (order.Quantity <= 0 || string.IsNullOrWhiteSpace(order.Product))
        {
            return new FulfilmentDecision(order.Id, false, "Order is missing product or quantity details.");
        }

        if (order.Total >= 500m)
        {
            return new FulfilmentDecision(order.Id, true, "Priority shipping required for high-value order.");
        }

        return new FulfilmentDecision(order.Id, true, "Standard shipping approved.");
    }
}

public sealed record FulfilmentDecision(string OrderId, bool ShouldShip, string Reason);
