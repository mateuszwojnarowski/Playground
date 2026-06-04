using AzureFunctionsFundamentals.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureFunctionsFundamentals.Modules.CosmosDbTrigger.Exercise;

public sealed class OrderAuditFunction(AuditProjector projector, ILogger<OrderAuditFunction> logger)
{
    // TODO: Implement the OrderAuditFunction.
    // Hints:
    // - Trigger: Use [CosmosDBTrigger("LearningDb", "orders", Connection = "CosmosDbConnection", LeaseContainerName = "orders-leases", CreateLeaseContainerIfNotExists = true)] to read changed order documents.
    // - Output: Use [CosmosDBOutput("LearningDb", "audit", Connection = "CosmosDbConnection")] on the method or return type to project the data.
    // - Signature: Returns an IReadOnlyList<OrderAuditProjection>.
    // - Logic: Log the number of changed order documents, project each changed order using projector.Project, and return the projected audit items.
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
        throw new NotImplementedException("TODO: Implement the CosmosDB Audit Projector trigger/output function according to the exercise guidelines.");
    }
}
