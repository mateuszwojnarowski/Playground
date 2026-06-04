using AzureFunctionsFundamentals.Modules.SqlRead;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = FunctionsApplication.CreateBuilder(args);
builder.ConfigureFunctionsWebApplication();

builder.Services.AddApplicationInsightsTelemetryWorkerService();

builder.Logging.Services.Configure<LoggerFilterOptions>(options =>
{
    LoggerFilterRule? rule = options.Rules.FirstOrDefault(r =>
        r.ProviderName ==
        "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
    if (rule is not null)
    {
        options.Rules.Remove(rule);
    }
});

builder.Services.AddSingleton<ICustomerRepository>(provider =>
{
    string connectionString = provider.GetRequiredService<IConfiguration>().GetConnectionString("SqlConnectionString")
        ?? provider.GetRequiredService<IConfiguration>()["SqlConnectionString"]
        ?? throw new InvalidOperationException("SqlConnectionString is not configured.");

    return new SqlCustomerRepository(connectionString);
});
builder.Services.AddSingleton<OrderEnricher>();

builder.Build().Run();
