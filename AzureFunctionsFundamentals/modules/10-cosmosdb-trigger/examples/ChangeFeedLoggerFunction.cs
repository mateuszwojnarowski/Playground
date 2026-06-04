using AzureFunctionsFundamentals.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureFunctionsFundamentals.Modules.CosmosDbTrigger.Examples;

public sealed class ChangeFeedLoggerFunction(ILogger<ChangeFeedLoggerFunction> logger)
{
    [Function(nameof(ChangeFeedLoggerFunction))]
    public void Run(
        [CosmosDBTrigger(
            databaseName: "LearningDb",
            containerName: "orders",
            Connection = "CosmosDbConnection",
            LeaseContainerName = "orders-leases",
            CreateLeaseContainerIfNotExists = true)]
        IReadOnlyList<Order> orders)
    {
        logger.LogInformation("Cosmos change feed delivered {OrderCount} order documents.", orders.Count);

        foreach (var order in orders)
        {
            logger.LogInformation("Order {OrderId} for customer {CustomerId} changed; total {Total}.", order.Id, order.CustomerId, order.Total);
        }
    }
}
