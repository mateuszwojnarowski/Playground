using System;
using System.Linq;
using System.Reflection;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging.Abstractions;
using StorageQueueExercise;
using Xunit;

namespace StorageQueueExercise.Tests;

public sealed class ProcessJobFunctionTests
{
    [Fact]
    public void Function_HasCorrectAttributes()
    {
        var method = typeof(ProcessJobFunction).GetMethod("Run");
        Assert.NotNull(method);

        var functionAttr = method.GetCustomAttribute<FunctionAttribute>();
        Assert.NotNull(functionAttr);
        Assert.Equal(nameof(ProcessJobFunction), functionAttr.Name);

        var blobAttr = method.GetCustomAttribute<BlobOutputAttribute>();
        Assert.NotNull(blobAttr);
        Assert.Equal("processed/{rand-guid}.json", blobAttr.BlobPath);
        Assert.Equal("AzureWebJobsStorage", blobAttr.Connection);

        var parameter = method.GetParameters().FirstOrDefault(p => p.ParameterType == typeof(string));
        Assert.NotNull(parameter);

        var triggerAttr = parameter.GetCustomAttribute<QueueTriggerAttribute>();
        Assert.NotNull(triggerAttr);
        Assert.Equal("incoming-jobs", triggerAttr.QueueName);
        Assert.Equal("AzureWebJobsStorage", triggerAttr.Connection);
    }

    [Fact]
    public void Run_ThrowsNotImplementedException_Initially()
    {
        var processor = new JobProcessor();
        var function = new ProcessJobFunction(processor, NullLogger<ProcessJobFunction>.Instance);
        Assert.Throws<NotImplementedException>(() => function.Run("test-message"));
    }
}
