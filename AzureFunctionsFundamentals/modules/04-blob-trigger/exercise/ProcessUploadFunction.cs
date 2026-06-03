using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BlobTriggerExercise;

public sealed class ProcessUploadFunction
{
    private readonly UploadProcessor _processor;
    private readonly ILogger<ProcessUploadFunction> _logger;

    public ProcessUploadFunction(UploadProcessor processor, ILogger<ProcessUploadFunction> logger)
    {
        _processor = processor;
        _logger = logger;
    }

    [Function(nameof(ProcessUploadFunction))]
    [BlobOutput("processed/{name}.processed.json", Connection = "AzureWebJobsStorage")]
    public string Run(
        [BlobTrigger("uploads/{name}", Connection = "AzureWebJobsStorage")] string content,
        string name)
    {
        var output = _processor.TransformToJson(content, name);
        _logger.LogInformation("Processed uploaded blob {Name} and wrote transformed output.", name);
        return output;
    }
}
