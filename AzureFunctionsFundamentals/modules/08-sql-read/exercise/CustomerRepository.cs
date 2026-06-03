using AzureFunctionsFundamentals.Shared;
using Microsoft.Data.SqlClient;

namespace AzureFunctionsFundamentals.Modules.SqlRead;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(int customerId, CancellationToken cancellationToken = default);
}

public sealed class SqlCustomerRepository : ICustomerRepository
{
    private readonly string _connectionString;

    public SqlCustomerRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Customer?> GetByIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, Tier FROM dbo.Customers WHERE Id = @CustomerId";
        command.Parameters.Add(new SqlParameter("@CustomerId", customerId));

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new Customer
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Tier = reader.GetString(2)
        };
    }
}
