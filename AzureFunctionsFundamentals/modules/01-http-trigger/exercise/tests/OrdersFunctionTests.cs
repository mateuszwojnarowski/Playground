using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AzureFunctionsFundamentals.Modules.HttpTrigger.Exercise;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Xunit;

namespace AzureFunctionsFundamentals.Modules.HttpTrigger.Exercise.Tests;

public sealed class OrdersFunctionTests
{
    [Fact]
    public void Function_HasCorrectAttributes()
    {
        var method = typeof(OrdersFunction).GetMethod("CreateOrderAsync");
        Assert.NotNull(method);

        var functionAttr = method.GetCustomAttribute<FunctionAttribute>();
        Assert.NotNull(functionAttr);
        Assert.Equal("CreateOrder", functionAttr.Name);

        var parameter = method.GetParameters().FirstOrDefault(p => p.ParameterType == typeof(HttpRequest));
        Assert.NotNull(parameter);

        var triggerAttr = parameter.GetCustomAttribute<HttpTriggerAttribute>();
        Assert.NotNull(triggerAttr);
        Assert.Equal(AuthorizationLevel.Anonymous, triggerAttr.AuthLevel);
        Assert.Contains("post", triggerAttr.Methods);
        Assert.Equal("orders", triggerAttr.Route);
    }

    [Fact]
    public async Task CreateOrderAsync_ThrowsNotImplementedException_Initially()
    {
        var function = new OrdersFunction(new OrderService(new OrderValidator()));
        await Assert.ThrowsAsync<NotImplementedException>(() =>
            function.CreateOrderAsync(null!, CancellationToken.None));
    }
}
