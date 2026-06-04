using System;
using System.Linq;
using System.Reflection;
using AzureFunctionsFundamentals.Modules.ServiceBusTopic;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ServiceBusTopicExercise.Tests;

public sealed class OrderEventFunctionsTests
{
    [Fact]
    public void Functions_HaveCorrectAttributes()
    {
        // AuditSubscriber
        var auditMethod = typeof(OrderEventFunctions).GetMethod("AuditSubscriber");
        Assert.NotNull(auditMethod);

        var auditFunctionAttr = auditMethod.GetCustomAttribute<FunctionAttribute>();
        Assert.NotNull(auditFunctionAttr);
        Assert.Equal("AuditSubscriber", auditFunctionAttr.Name);

        var auditParam = auditMethod.GetParameters().FirstOrDefault(p => p.ParameterType == typeof(string));
        Assert.NotNull(auditParam);

        var auditTriggerAttr = auditParam.GetCustomAttribute<ServiceBusTriggerAttribute>();
        Assert.NotNull(auditTriggerAttr);
        Assert.Equal("order-events", auditTriggerAttr.TopicName);
        Assert.Equal("audit", auditTriggerAttr.SubscriptionName);
        Assert.Equal("ServiceBusConnection", auditTriggerAttr.Connection);

        // FulfilmentSubscriber
        var fulfilmentMethod = typeof(OrderEventFunctions).GetMethod("FulfilmentSubscriber");
        Assert.NotNull(fulfilmentMethod);

        var fulfilmentFunctionAttr = fulfilmentMethod.GetCustomAttribute<FunctionAttribute>();
        Assert.NotNull(fulfilmentFunctionAttr);
        Assert.Equal("FulfilmentSubscriber", fulfilmentFunctionAttr.Name);

        var fulfilmentParam = fulfilmentMethod.GetParameters().FirstOrDefault(p => p.ParameterType == typeof(string));
        Assert.NotNull(fulfilmentParam);

        var fulfilmentTriggerAttr = fulfilmentParam.GetCustomAttribute<ServiceBusTriggerAttribute>();
        Assert.NotNull(fulfilmentTriggerAttr);
        Assert.Equal("order-events", fulfilmentTriggerAttr.TopicName);
        Assert.Equal("fulfilment", fulfilmentTriggerAttr.SubscriptionName);
        Assert.Equal("ServiceBusConnection", fulfilmentTriggerAttr.Connection);
    }

    [Fact]
    public void AuditSubscriber_ThrowsNotImplementedException_Initially()
    {
        var function = new OrderEventFunctions(new AuditHandler(), new FulfilmentHandler(), NullLogger<OrderEventFunctions>.Instance);
        Assert.Throws<NotImplementedException>(() => function.AuditSubscriber("{}"));
    }

    [Fact]
    public void FulfilmentSubscriber_ThrowsNotImplementedException_Initially()
    {
        var function = new OrderEventFunctions(new AuditHandler(), new FulfilmentHandler(), NullLogger<OrderEventFunctions>.Instance);
        Assert.Throws<NotImplementedException>(() => function.FulfilmentSubscriber("{}"));
    }
}
