using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AzureFunctionsFundamentals.Modules.Auth.OidcOAuth.Exercise;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;
using Xunit;

namespace OidcOAuthExercise.Tests;

public sealed class OrdersApiFunctionTests
{
    [Fact]
    public void Functions_HaveCorrectAttributes()
    {
        // Health
        var healthMethod = typeof(OrdersApiFunction).GetMethod("Health");
        Assert.NotNull(healthMethod);
        var healthFunctionAttr = healthMethod.GetCustomAttribute<FunctionAttribute>();
        Assert.NotNull(healthFunctionAttr);
        Assert.Equal("Health", healthFunctionAttr.Name);

        var healthParam = healthMethod.GetParameters().FirstOrDefault(p => p.ParameterType == typeof(HttpRequest));
        Assert.NotNull(healthParam);
        var healthTriggerAttr = healthParam.GetCustomAttribute<HttpTriggerAttribute>();
        Assert.NotNull(healthTriggerAttr);
        Assert.Equal(AuthorizationLevel.Anonymous, healthTriggerAttr.AuthLevel);
        Assert.Contains("get", healthTriggerAttr.Methods);
        Assert.Equal("health", healthTriggerAttr.Route);

        // GetOrdersAsync
        var ordersMethod = typeof(OrdersApiFunction).GetMethod("GetOrdersAsync");
        Assert.NotNull(ordersMethod);
        var ordersFunctionAttr = ordersMethod.GetCustomAttribute<FunctionAttribute>();
        Assert.NotNull(ordersFunctionAttr);
        Assert.Equal("GetOrders", ordersFunctionAttr.Name);

        var ordersParam = ordersMethod.GetParameters().FirstOrDefault(p => p.ParameterType == typeof(HttpRequest));
        Assert.NotNull(ordersParam);
        var ordersTriggerAttr = ordersParam.GetCustomAttribute<HttpTriggerAttribute>();
        Assert.NotNull(ordersTriggerAttr);
        Assert.Equal(AuthorizationLevel.Anonymous, ordersTriggerAttr.AuthLevel);
        Assert.Contains("get", ordersTriggerAttr.Methods);
        Assert.Equal("orders", ordersTriggerAttr.Route);
    }

    [Fact]
    public async Task GetOrdersAsync_ThrowsNotImplementedException_Initially()
    {
        var authOpts = Options.Create(new AuthOptions
        {
            Authority = "https://example.com",
            Audience = "aud",
            RequiredPermission = "read"
        });
        var tokenValidation = new TokenValidation(authOpts);
        var authorizer = new TokenAuthorizer(tokenValidation);
        var function = new OrdersApiFunction(authorizer, authOpts);
        var context = new DefaultHttpContext();

        await Assert.ThrowsAsync<NotImplementedException>(() => function.GetOrdersAsync(context.Request, CancellationToken.None));
    }

    [Fact]
    public void Health_ReturnsOk()
    {
        var authOpts = Options.Create(new AuthOptions
        {
            Authority = "https://example.com",
            Audience = "aud",
            RequiredPermission = "read"
        });
        var tokenValidation = new TokenValidation(authOpts);
        var authorizer = new TokenAuthorizer(tokenValidation);
        var function = new OrdersApiFunction(authorizer, authOpts);
        var context = new DefaultHttpContext();

        var result = function.Health(context.Request);
        Assert.IsType<OkObjectResult>(result);
    }
}
