using AzureFunctionsFundamentals.Modules.ServiceBusPipeline;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);
builder.Services.AddSingleton<IProcessingClock, SystemProcessingClock>();
builder.Services.AddSingleton<OrderTransformer>();
builder.Build().Run();
