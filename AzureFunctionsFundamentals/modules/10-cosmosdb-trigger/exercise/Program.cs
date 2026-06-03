using AzureFunctionsFundamentals.Modules.CosmosDbTrigger.Exercise;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);
builder.Services.AddSingleton<AuditProjector>();
builder.Build().Run();
