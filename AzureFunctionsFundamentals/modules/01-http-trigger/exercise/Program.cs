using AzureFunctionsFundamentals.Modules.HttpTrigger.Exercise;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);
builder.ConfigureFunctionsWebApplication();
builder.Services.AddSingleton<OrderValidator>();
builder.Services.AddSingleton<OrderService>();
builder.Build().Run();
