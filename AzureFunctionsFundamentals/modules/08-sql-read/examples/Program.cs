using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SqlReadExample;

var builder = FunctionsApplication.CreateBuilder(args);
builder.Services.AddSingleton<ICustomerRepository>(provider =>
{
    string connectionString = provider.GetRequiredService<IConfiguration>().GetConnectionString("SqlConnectionString")
        ?? provider.GetRequiredService<IConfiguration>()["SqlConnectionString"]
        ?? throw new InvalidOperationException("SqlConnectionString is not configured.");

    return new SqlCustomerRepository(connectionString);
});
builder.Build().Run();
