using System;
using System.Linq;
using System.Reflection;
using AzureFunctionsFundamentals.Modules.Auth.BasicAuth.Exercise;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Xunit;

namespace BasicAuthExercise.Tests;

public sealed class ReportsApiFunctionTests
{
    [Fact]
    public void Functions_HaveCorrectAttributes()
    {
        // Health
        var healthMethod = typeof(ReportsApiFunction).GetMethod("Health");
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

        // GetReports
        var reportsMethod = typeof(ReportsApiFunction).GetMethod("GetReports");
        Assert.NotNull(reportsMethod);
        var reportsFunctionAttr = reportsMethod.GetCustomAttribute<FunctionAttribute>();
        Assert.NotNull(reportsFunctionAttr);
        Assert.Equal("GetReports", reportsFunctionAttr.Name);

        var reportsParam = reportsMethod.GetParameters().FirstOrDefault(p => p.ParameterType == typeof(HttpRequest));
        Assert.NotNull(reportsParam);
        var reportsTriggerAttr = reportsParam.GetCustomAttribute<HttpTriggerAttribute>();
        Assert.NotNull(reportsTriggerAttr);
        Assert.Equal(AuthorizationLevel.Anonymous, reportsTriggerAttr.AuthLevel);
        Assert.Contains("get", reportsTriggerAttr.Methods);
        Assert.Equal("reports", reportsTriggerAttr.Route);
    }

    [Fact]
    public void GetReports_ThrowsNotImplementedException_Initially()
    {
        var store = new InMemoryCredentialStore(new Dictionary<string, string>());
        var authenticator = new BasicAuthenticator(store);
        var function = new ReportsApiFunction(authenticator);
        var context = new DefaultHttpContext();

        Assert.Throws<NotImplementedException>(() => function.GetReports(context.Request));
    }

    [Fact]
    public void Health_ReturnsOk()
    {
        var store = new InMemoryCredentialStore(new Dictionary<string, string>());
        var authenticator = new BasicAuthenticator(store);
        var function = new ReportsApiFunction(authenticator);
        var context = new DefaultHttpContext();

        var result = function.Health(context.Request);
        Assert.IsType<OkObjectResult>(result);
    }

    private sealed class InMemoryCredentialStore(IReadOnlyDictionary<string, string> credentials) : ICredentialStore
    {
        public bool TryGetExpectedPassword(string username, out string? password)
        {
            var found = credentials.TryGetValue(username, out var value);
            password = value;
            return found;
        }
    }
}
