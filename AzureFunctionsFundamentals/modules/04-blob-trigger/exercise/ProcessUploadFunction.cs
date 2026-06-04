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

    // TODO: Implement the ProcessUploadFunction.
    // Hints:
    // - Trigger: Use [BlobTrigger("uploads/{name}", Connection = "AzureWebJobsStorage")] to bind the blob contents as a string parameter, and accept `string name` as a binding parameter.
    // - Output: Use [BlobOutput("processed/{name}.processed.json", Connection = "AzureWebJobsStorage")] on the return value or method.
    // - Signature: Returns string which is written as the processed blob.
    // - Logic: Invoke _processor.TransformToJson(content, name), log processing, and return the result.
    [Function(nameof(ProcessUploadFunction))]
    [BlobOutput("processed/{name}.processed.json", Connection = "AzureWebJobsStorage")]
    public string Run(
        [BlobTrigger("uploads/{name}", Connection = "AzureWebJobsStorage")] string content,
        string name)
    {
        throw new NotImplementedException("TODO: Implement the Blob-triggered function with Blob Output according to the exercise guidelines.");
    }
}
