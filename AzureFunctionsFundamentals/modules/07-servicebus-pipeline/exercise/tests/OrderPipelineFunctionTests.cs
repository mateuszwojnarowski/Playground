using System;
using System.Linq;
using System.Reflection;
using AzureFunctionsFundamentals.Modules.ServiceBusPipeline;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ServiceBusPipelineExercise.Tests;

public sealed class OrderPipelineFunctionTests
{
    [Fact]
    public void Function_HasCorrectAttributes()
    {
        var method = typeof(OrderPipelineFunction).GetMethod("Run");
        Assert.NotNull(method);

        var functionAttr = method.GetCustomAttribute<FunctionAttribute>();
        Assert.NotNull(functionAttr);
        Assert.Equal(nameof(OrderPipelineFunction), functionAttr.Name);

        var outputAttr = method.GetCustomAttribute<ServiceBusOutputAttribute>();
        Assert.NotNull(outputAttr);
        Assert.Equal("orders-out", outputAttr.QueueOrTopicName);
        Assert.Equal("ServiceBusConnection", outputAttr.Connection);

        var parameter = method.GetParameters().FirstOrDefault(p => p.ParameterType == typeof(string));
        Assert.NotNull(parameter);

        var triggerAttr = parameter.GetCustomAttribute<ServiceBusTriggerAttribute>();
        Assert.NotNull(triggerAttr);
        Assert.Equal("enrich-in", triggerAttr.QueueOrTopicName);
        Assert.Equal("ServiceBusConnection", triggerAttr.Connection);
    }

    [Fact]
    public void Run_ThrowsNotImplementedException_Initially()
    {
        var transformer = new OrderTransformer(TimeProvider.System);
        var function = new OrderPipelineFunction(transformer, NullLogger<OrderPipelineFunction>.Instance);
        Assert.Throws<NotImplementedException>(() => function.Run("{}"));
    }
}
