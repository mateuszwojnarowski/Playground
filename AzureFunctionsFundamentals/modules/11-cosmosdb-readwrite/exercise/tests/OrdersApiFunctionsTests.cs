using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AzureFunctionsFundamentals.Modules.CosmosDbReadWrite.Exercise;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Xunit;

namespace CosmosDbReadWriteExercise.Tests;

public sealed class OrdersApiFunctionsTests
{
    [Fact]
    public void Functions_HaveCorrectAttributes()
    {
        // UpsertOrder
        var upsertMethod = typeof(OrdersApiFunctions).GetMethod("UpsertAsync");
        Assert.NotNull(upsertMethod);
        var upsertFunctionAttr = upsertMethod.GetCustomAttribute<FunctionAttribute>();
        Assert.NotNull(upsertFunctionAttr);
        Assert.Equal("UpsertOrder", upsertFunctionAttr.Name);

        var upsertParam = upsertMethod.GetParameters().FirstOrDefault(p => p.ParameterType == typeof(HttpRequest));
        Assert.NotNull(upsertParam);
        var upsertTriggerAttr = upsertParam.GetCustomAttribute<HttpTriggerAttribute>();
        Assert.NotNull(upsertTriggerAttr);
        Assert.Equal(AuthorizationLevel.Anonymous, upsertTriggerAttr.AuthLevel);
        Assert.Contains("post", upsertTriggerAttr.Methods);
        Assert.Equal("orders", upsertTriggerAttr.Route);

        // GetOrdersByCustomer
        var getMethod = typeof(OrdersApiFunctions).GetMethod("GetByCustomerAsync");
        Assert.NotNull(getMethod);
        var getFunctionAttr = getMethod.GetCustomAttribute<FunctionAttribute>();
        Assert.NotNull(getFunctionAttr);
        Assert.Equal("GetOrdersByCustomer", getFunctionAttr.Name);

        var getParam = getMethod.GetParameters().FirstOrDefault(p => p.ParameterType == typeof(HttpRequest));
        Assert.NotNull(getParam);
        var getTriggerAttr = getParam.GetCustomAttribute<HttpTriggerAttribute>();
        Assert.NotNull(getTriggerAttr);
        Assert.Equal(AuthorizationLevel.Anonymous, getTriggerAttr.AuthLevel);
        Assert.Contains("get", getTriggerAttr.Methods);
        Assert.Equal("orders/{customerId:int}", getTriggerAttr.Route);
    }

    [Fact]
    public async Task UpsertAsync_ThrowsNotImplementedException_Initially()
    {
        var repo = new FakeOrderRepository();
        var service = new OrderService(repo);
        var function = new OrdersApiFunctions(service);
        var context = new DefaultHttpContext();

        await Assert.ThrowsAsync<NotImplementedException>(() => function.UpsertAsync(context.Request, CancellationToken.None));
    }

    [Fact]
    public async Task GetByCustomerAsync_ThrowsNotImplementedException_Initially()
    {
        var repo = new FakeOrderRepository();
        var service = new OrderService(repo);
        var function = new OrdersApiFunctions(service);
        var context = new DefaultHttpContext();

        await Assert.ThrowsAsync<NotImplementedException>(() => function.GetByCustomerAsync(context.Request, 123, CancellationToken.None));
    }
}
