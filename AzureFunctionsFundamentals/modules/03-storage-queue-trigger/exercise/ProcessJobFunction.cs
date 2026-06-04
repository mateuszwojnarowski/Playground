using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace StorageQueueExercise;

public sealed class ProcessJobFunction
{
    private readonly JobProcessor _processor;
    private readonly ILogger<ProcessJobFunction> _logger;

    public ProcessJobFunction(JobProcessor processor, ILogger<ProcessJobFunction> logger)
    {
        _processor = processor;
        _logger = logger;
    }

    // TODO: Implement the ProcessJobFunction.
    // Hints:
    // - Trigger: Use [QueueTrigger("incoming-jobs", Connection = "AzureWebJobsStorage")] parameter.
    // - Output: Use [BlobOutput("processed/{rand-guid}.json", Connection = "AzureWebJobsStorage")] on the return value or method.
    // - Signature: Returns string which is written as a blob.
    // - Logic: Invoke _processor.ProcessToJson(message), log completion, and return the result.
    [Function(nameof(ProcessJobFunction))]
    [BlobOutput("processed/{rand-guid}.json", Connection = "AzureWebJobsStorage")]
    public string Run(
        [QueueTrigger("incoming-jobs", Connection = "AzureWebJobsStorage")] string message)
    {
        throw new NotImplementedException("TODO: Implement the Queue-triggered function with Blob Output according to the exercise guidelines.");
    }
}
