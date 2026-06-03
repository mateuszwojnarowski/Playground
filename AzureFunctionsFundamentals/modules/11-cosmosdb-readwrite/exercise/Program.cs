using AzureFunctionsFundamentals.Modules.CosmosDbReadWrite.Exercise;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);
builder.ConfigureFunctionsWebApplication();
builder.Services.AddSingleton(_ => new CosmosClient(
    Environment.GetEnvironmentVariable("CosmosDbConnection") ?? throw new InvalidOperationException("CosmosDbConnection is not configured."),
    new CosmosClientOptions { ConnectionMode = ConnectionMode.Gateway }));
builder.Services.AddSingleton<IOrderRepository, CosmosOrderRepository>();
builder.Services.AddSingleton<OrderService>();
builder.Build().Run();
