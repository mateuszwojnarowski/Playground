using System;
using System.Linq;
using System.Reflection;
using AzureFunctionsFundamentals.Modules.TimerTrigger.Exercise;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AzureFunctionsFundamentals.Modules.TimerTrigger.Exercise.Tests;

public sealed class DailyCleanupFunctionTests
{
    [Fact]
    public void Function_HasCorrectAttributes()
    {
        var method = typeof(DailyCleanupFunction).GetMethod("Run");
        Assert.NotNull(method);

        var functionAttr = method.GetCustomAttribute<FunctionAttribute>();
        Assert.NotNull(functionAttr);
        Assert.Equal("DailyOrderCleanup", functionAttr.Name);

        var parameter = method.GetParameters().FirstOrDefault(p => p.ParameterType == typeof(TimerInfo));
        Assert.NotNull(parameter);

        var triggerAttr = parameter.GetCustomAttribute<TimerTriggerAttribute>();
        Assert.NotNull(triggerAttr);
        Assert.Equal("0 0 2 * * *", triggerAttr.Schedule);
    }

    [Fact]
    public void Run_ThrowsNotImplementedException_Initially()
    {
        var service = new OrderCleanupService(TimeProvider.System);
        var function = new DailyCleanupFunction(service, NullLogger<DailyCleanupFunction>.Instance);
        Assert.Throws<NotImplementedException>(() => function.Run(null!));
    }
}
