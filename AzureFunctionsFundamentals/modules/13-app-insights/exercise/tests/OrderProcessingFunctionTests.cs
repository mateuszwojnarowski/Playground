using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AzureFunctionsFundamentals.Modules.AppInsights.Exercise;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AzureFunctionsFundamentals.Modules.AppInsights.Exercise.Tests;

public sealed class OrderProcessingFunctionTests
{
    [Fact]
    public void Function_HasCorrectAttributes()
    {
        var method = typeof(OrderProcessingFunction).GetMethod("ProcessOrderAsync");
        Assert.NotNull(method);

        var functionAttr = method.GetCustomAttribute<FunctionAttribute>();
        Assert.NotNull(functionAttr);
        Assert.Equal("ProcessOrder", functionAttr.Name);

        var parameter = method.GetParameters().FirstOrDefault(p => p.ParameterType == typeof(HttpRequest));
        Assert.NotNull(parameter);

        var triggerAttr = parameter.GetCustomAttribute<HttpTriggerAttribute>();
        Assert.NotNull(triggerAttr);
        Assert.Equal(AuthorizationLevel.Anonymous, triggerAttr.AuthLevel);
        Assert.Contains("post", triggerAttr.Methods);
        Assert.Equal("telemetry/orders", triggerAttr.Route);
    }

    [Fact]
    public async Task ProcessOrderAsync_ThrowsNotImplementedException_Initially()
    {
        var telemetryConfiguration = new TelemetryConfiguration();
        var telemetryClient = new TelemetryClient(telemetryConfiguration);
        var telemetryService = new OrderTelemetryService(NullLogger<OrderTelemetryService>.Instance, telemetryClient);
        var function = new OrderProcessingFunction(telemetryService);
        var context = new DefaultHttpContext();

        await Assert.ThrowsAsync<NotImplementedException>(() => function.ProcessOrderAsync(context.Request, CancellationToken.None));
    }
}
