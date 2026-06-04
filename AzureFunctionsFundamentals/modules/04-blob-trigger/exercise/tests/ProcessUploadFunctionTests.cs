using System;
using System.Linq;
using System.Reflection;
using BlobTriggerExercise;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BlobTriggerExercise.Tests;

public sealed class ProcessUploadFunctionTests
{
    [Fact]
    public void Function_HasCorrectAttributes()
    {
        var method = typeof(ProcessUploadFunction).GetMethod("Run");
        Assert.NotNull(method);

        var functionAttr = method.GetCustomAttribute<FunctionAttribute>();
        Assert.NotNull(functionAttr);
        Assert.Equal(nameof(ProcessUploadFunction), functionAttr.Name);

        var blobAttr = method.GetCustomAttribute<BlobOutputAttribute>();
        Assert.NotNull(blobAttr);
        Assert.Equal("processed/{name}.processed.json", blobAttr.BlobPath);
        Assert.Equal("AzureWebJobsStorage", blobAttr.Connection);

        var contentParam = method.GetParameters().FirstOrDefault(p => p.Name == "content");
        Assert.NotNull(contentParam);

        var triggerAttr = contentParam.GetCustomAttribute<BlobTriggerAttribute>();
        Assert.NotNull(triggerAttr);
        Assert.Equal("uploads/{name}", triggerAttr.BlobPath);
        Assert.Equal("AzureWebJobsStorage", triggerAttr.Connection);
    }

    [Fact]
    public void Run_ThrowsNotImplementedException_Initially()
    {
        var processor = new UploadProcessor();
        var function = new ProcessUploadFunction(processor, NullLogger<ProcessUploadFunction>.Instance);
        Assert.Throws<NotImplementedException>(() => function.Run("test-content", "test-name"));
    }
}
