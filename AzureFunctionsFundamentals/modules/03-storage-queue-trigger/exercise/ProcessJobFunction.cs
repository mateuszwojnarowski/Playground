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

    [Function(nameof(ProcessJobFunction))]
    [BlobOutput("processed/{rand-guid}.json", Connection = "AzureWebJobsStorage")]
    public string Run(
        [QueueTrigger("incoming-jobs", Connection = "AzureWebJobsStorage")] string message)
    {
        var output = _processor.ProcessToJson(message);
        _logger.LogInformation("Processed queue job and wrote a result document to the processed container.");
        return output;
    }
}
