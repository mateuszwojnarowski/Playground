using AzureFunctionsFundamentals.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureFunctionsFundamentals.Modules.CosmosDbTrigger.Exercise;

public sealed class OrderAuditFunction(AuditProjector projector, ILogger<OrderAuditFunction> logger)
{
    [Function(nameof(OrderAuditFunction))]
    [CosmosDBOutput("LearningDb", "audit", Connection = "CosmosDbConnection")]
    public IReadOnlyList<OrderAuditProjection> Run(
        [CosmosDBTrigger(
            databaseName: "LearningDb",
            containerName: "orders",
            Connection = "CosmosDbConnection",
            LeaseContainerName = "orders-leases",
            CreateLeaseContainerIfNotExists = true)]
        IReadOnlyList<Order> orders)
    {
        logger.LogInformation("Projecting {Count} changed order documents into the audit container.", orders.Count);
        return orders.Select(projector.Project).ToArray();
    }
}
