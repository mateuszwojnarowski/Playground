using AzureFunctionsFundamentals.Modules.TimerTrigger.Exercise;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = FunctionsApplication.CreateBuilder(args);

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

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<OrderCleanupService>();

builder.Build().Run();
