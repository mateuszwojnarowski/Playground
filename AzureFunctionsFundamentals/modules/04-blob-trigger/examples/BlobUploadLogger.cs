using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BlobTriggerExample;

public sealed class BlobUploadLogger
{
    private readonly ILogger<BlobUploadLogger> _logger;

    public BlobUploadLogger(ILogger<BlobUploadLogger> logger)
    {
        _logger = logger;
    }

    [Function(nameof(BlobUploadLogger))]
    public void Run(
        [BlobTrigger("uploads/{name}", Connection = "AzureWebJobsStorage")] Stream blob,
        string name)
    {
        _logger.LogInformation("Blob uploaded: {Name}, size: {Size} bytes", name, blob.Length);
    }
}
