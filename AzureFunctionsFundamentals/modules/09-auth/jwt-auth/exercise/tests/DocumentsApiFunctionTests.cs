using System;
using System.Linq;
using System.Reflection;
using AzureFunctionsFundamentals.Modules.Auth.JwtAuth.Exercise;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Xunit;

namespace JwtAuthExercise.Tests;

public sealed class DocumentsApiFunctionTests
{
    [Fact]
    public void Functions_HaveCorrectAttributes()
    {
        // Health
        var healthMethod = typeof(DocumentsApiFunction).GetMethod("Health");
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

        // GetDocuments
        var documentsMethod = typeof(DocumentsApiFunction).GetMethod("GetDocuments");
        Assert.NotNull(documentsMethod);
        var documentsFunctionAttr = documentsMethod.GetCustomAttribute<FunctionAttribute>();
        Assert.NotNull(documentsFunctionAttr);
        Assert.Equal("GetDocuments", documentsFunctionAttr.Name);

        var documentsParam = documentsMethod.GetParameters().FirstOrDefault(p => p.ParameterType == typeof(HttpRequest));
        Assert.NotNull(documentsParam);
        var documentsTriggerAttr = documentsParam.GetCustomAttribute<HttpTriggerAttribute>();
        Assert.NotNull(documentsTriggerAttr);
        Assert.Equal(AuthorizationLevel.Anonymous, documentsTriggerAttr.AuthLevel);
        Assert.Contains("get", documentsTriggerAttr.Methods);
        Assert.Equal("documents", documentsTriggerAttr.Route);
    }

    [Fact]
    public void GetDocuments_ThrowsNotImplementedException_Initially()
    {
        var options = Options.Create(new JwtOptions { Issuer = "iss", Audience = "aud", SigningKey = "super_secret_key_that_is_at_least_32_bytes_long_12345" });
        var tokenService = new JwtTokenService(options);
        var authorizer = new DocumentsAuthorizer(tokenService);
        var config = new ConfigurationBuilder().Build();
        var function = new DocumentsApiFunction(authorizer, config);
        var context = new DefaultHttpContext();

        Assert.Throws<NotImplementedException>(() => function.GetDocuments(context.Request));
    }

    [Fact]
    public void Health_ReturnsOk()
    {
        var options = Options.Create(new JwtOptions { Issuer = "iss", Audience = "aud", SigningKey = "super_secret_key_that_is_at_least_32_bytes_long_12345" });
        var tokenService = new JwtTokenService(options);
        var authorizer = new DocumentsAuthorizer(tokenService);
        var config = new ConfigurationBuilder().Build();
        var function = new DocumentsApiFunction(authorizer, config);
        var context = new DefaultHttpContext();

        var result = function.Health(context.Request);
        Assert.IsType<OkObjectResult>(result);
    }
}
