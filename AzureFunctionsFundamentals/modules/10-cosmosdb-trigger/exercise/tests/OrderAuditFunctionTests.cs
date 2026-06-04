using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AzureFunctionsFundamentals.Modules.CosmosDbTrigger.Exercise;
using AzureFunctionsFundamentals.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CosmosDbTriggerExercise.Tests;

public sealed class OrderAuditFunctionTests
{
    [Fact]
    public void Function_HasCorrectAttributes()
    {
        var method = typeof(OrderAuditFunction).GetMethod("Run");
        Assert.NotNull(method);

        var functionAttr = method.GetCustomAttribute<FunctionAttribute>();
        Assert.NotNull(functionAttr);
        Assert.Equal(nameof(OrderAuditFunction), functionAttr.Name);

        var outputAttr = method.GetCustomAttribute<CosmosDBOutputAttribute>();
        Assert.NotNull(outputAttr);
        Assert.Equal("LearningDb", outputAttr.DatabaseName);
        Assert.Equal("audit", outputAttr.ContainerName);
        Assert.Equal("CosmosDbConnection", outputAttr.Connection);

        var parameter = method.GetParameters().FirstOrDefault(p => p.ParameterType == typeof(IReadOnlyList<Order>));
        Assert.NotNull(parameter);

        var triggerAttr = parameter.GetCustomAttribute<CosmosDBTriggerAttribute>();
        Assert.NotNull(triggerAttr);
        Assert.Equal("LearningDb", triggerAttr.DatabaseName);
        Assert.Equal("orders", triggerAttr.ContainerName);
        Assert.Equal("CosmosDbConnection", triggerAttr.Connection);
        Assert.Equal("orders-leases", triggerAttr.LeaseContainerName);
        Assert.True(triggerAttr.CreateLeaseContainerIfNotExists);
    }

    [Fact]
    public void Run_ThrowsNotImplementedException_Initially()
    {
        var projector = new AuditProjector(TimeProvider.System);
        var function = new OrderAuditFunction(projector, NullLogger<OrderAuditFunction>.Instance);
        Assert.Throws<NotImplementedException>(() => function.Run(new List<Order>()));
    }
}
