using System;
using System.Linq;
using System.Reflection;
using AzureFunctionsFundamentals.Modules.SqlRead;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace SqlReadExercise.Tests;

public sealed class OrderEnrichmentFunctionTests
{
    [Fact]
    public void Function_HasCorrectAttributes()
    {
        var method = typeof(OrderEnrichmentFunction).GetMethod("RunAsync");
        Assert.NotNull(method);

        var functionAttr = method.GetCustomAttribute<FunctionAttribute>();
        Assert.NotNull(functionAttr);
        Assert.Equal(nameof(OrderEnrichmentFunction), functionAttr.Name);

        var outputAttr = method.GetCustomAttribute<ServiceBusOutputAttribute>();
        Assert.NotNull(outputAttr);
        Assert.Equal("orders-out", outputAttr.QueueOrTopicName);
        Assert.Equal("ServiceBusConnection", outputAttr.Connection);

        var parameter = method.GetParameters().FirstOrDefault(p => p.ParameterType == typeof(string));
        Assert.NotNull(parameter);

        var triggerAttr = parameter.GetCustomAttribute<ServiceBusTriggerAttribute>();
        Assert.NotNull(triggerAttr);
        Assert.Equal("enrich-in", triggerAttr.QueueName);
        Assert.Equal("ServiceBusConnection", triggerAttr.Connection);
    }

    [Fact]
    public async Task RunAsync_ThrowsNotImplementedException_Initially()
    {
        var enricher = new OrderEnricher(new FakeCustomerRepository());
        var function = new OrderEnrichmentFunction(enricher, NullLogger<OrderEnrichmentFunction>.Instance);
        await Assert.ThrowsAsync<NotImplementedException>(() => function.RunAsync("{}", CancellationToken.None));
    }

    private sealed class FakeCustomerRepository : ICustomerRepository
    {
        public Task<Customer?> GetByIdAsync(int customerId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Customer?>(null);
        }
    }
}
