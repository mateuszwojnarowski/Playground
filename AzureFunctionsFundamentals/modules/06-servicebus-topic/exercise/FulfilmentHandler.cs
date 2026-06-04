using AzureFunctionsFundamentals.Shared;

namespace AzureFunctionsFundamentals.Modules.ServiceBusTopic;

public sealed class FulfilmentHandler
{
    // TODO: Implement fulfilment decision logic.
    // - Decide whether the order should ship and why, using the rules in README.md.
    // - Handle invalid order data with the expected failure decision.
    // - Return the correct FulfilmentDecision for the incoming event.
    public FulfilmentDecision Decide(Order order)
    {
        throw new NotImplementedException("TODO: implement this method.");
    }
}

public sealed record FulfilmentDecision(string OrderId, bool ShouldShip, string Reason);
