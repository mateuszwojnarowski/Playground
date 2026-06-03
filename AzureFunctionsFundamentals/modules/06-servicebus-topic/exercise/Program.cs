using AzureFunctionsFundamentals.Modules.ServiceBusTopic;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);
builder.Services.AddSingleton<AuditHandler>();
builder.Services.AddSingleton<FulfilmentHandler>();
builder.Build().Run();
