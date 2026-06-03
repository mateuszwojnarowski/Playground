using AzureFunctionsFundamentals.Shared;

namespace AzureFunctionsFundamentals.Modules.CosmosDbReadWrite.Exercise;

public interface IOrderRepository
{
    Task<Order?> GetAsync(string id, int customerId, CancellationToken cancellationToken = default);
    Task<Order> UpsertAsync(Order order, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> QueryByCustomerAsync(int customerId, CancellationToken cancellationToken = default);
}
