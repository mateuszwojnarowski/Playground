using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AzureFunctionsFundamentals.Modules.CosmosDbServiceBusPipeline.Exercise;
using AzureFunctionsFundamentals.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CosmosDbServiceBusPipelineExercise.Tests;

public sealed class EnrichOrderFunctionTests
{
    [Fact]
    public void Function_HasCorrectAttributes()
    {
        var method = typeof(EnrichOrderFunction).GetMethod("RunAsync");
        Assert.NotNull(method);

        var functionAttr = method.GetCustomAttribute<FunctionAttribute>();
        Assert.NotNull(functionAttr);
        Assert.Equal(nameof(EnrichOrderFunction), functionAttr.Name);

        var outputAttr = method.GetCustomAttribute<CosmosDBOutputAttribute>();
        Assert.NotNull(outputAttr);
        Assert.Equal("LearningDb", outputAttr.DatabaseName);
        Assert.Equal("orders", outputAttr.ContainerName);
        Assert.Equal("CosmosDbConnection", outputAttr.Connection);

        var parameter = method.GetParameters().FirstOrDefault(p => p.ParameterType == typeof(string));
        Assert.NotNull(parameter);

        var triggerAttr = parameter.GetCustomAttribute<ServiceBusTriggerAttribute>();
        Assert.NotNull(triggerAttr);
        Assert.Equal("enrich-in", triggerAttr.QueueOrTopicName);
        Assert.Equal("ServiceBusConnection", triggerAttr.Connection);
    }

    [Fact]
    public async Task RunAsync_ThrowsNotImplementedException_Initially()
    {
        var productRepo = new FakeProductRepository();
        var customerRepo = new FakeCustomerRepository();
        var enricher = new CosmosOrderEnricher(productRepo, customerRepo);
        var function = new EnrichOrderFunction(enricher, NullLogger<EnrichOrderFunction>.Instance);

        await Assert.ThrowsAsync<NotImplementedException>(() => function.RunAsync("{}", CancellationToken.None));
    }
}
