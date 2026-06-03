using AzureFunctionsFundamentals.Modules.ServiceBusQueue;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);
builder.Services.AddSingleton<OrderValidator>();
builder.Services.AddSingleton<OrderConsumer>();
builder.Build().Run();
