using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AzureFunctionsFundamentals.Modules.ServiceBusQueue;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ServiceBusQueueExercise.Tests;

public sealed class OrderQueueFunctionTests
{
    [Fact]
    public void Function_HasCorrectAttributes()
    {
        var method = typeof(OrderQueueFunction).GetMethod("RunAsync");
        Assert.NotNull(method);

        var functionAttr = method.GetCustomAttribute<FunctionAttribute>();
        Assert.NotNull(functionAttr);
        Assert.Equal(nameof(OrderQueueFunction), functionAttr.Name);

        var parameter = method.GetParameters().FirstOrDefault(p => p.ParameterType == typeof(string));
        Assert.NotNull(parameter);

        var triggerAttr = parameter.GetCustomAttribute<ServiceBusTriggerAttribute>();
        Assert.NotNull(triggerAttr);
        Assert.Equal("orders", triggerAttr.QueueName);
        Assert.Equal("ServiceBusConnection", triggerAttr.Connection);
    }

    [Fact]
    public async Task RunAsync_ThrowsNotImplementedException_Initially()
    {
        var consumer = new OrderConsumer(new OrderValidator());
        var function = new OrderQueueFunction(consumer, NullLogger<OrderQueueFunction>.Instance);
        await Assert.ThrowsAsync<NotImplementedException>(() =>
            function.RunAsync("{}", CancellationToken.None));
    }
}
