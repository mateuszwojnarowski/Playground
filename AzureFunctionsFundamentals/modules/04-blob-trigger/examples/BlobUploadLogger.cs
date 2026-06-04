using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BlobTriggerExample;

public sealed class BlobUploadLogger(ILogger<BlobUploadLogger> logger)
{
    [Function(nameof(BlobUploadLogger))]
    public void Run(
        [BlobTrigger("uploads/{name}", Connection = "AzureWebJobsStorage")] Stream blob,
        string name)
    {
        logger.LogInformation("Blob upload received for {BlobName} with size {BlobSizeBytes} bytes.", name, blob.Length);
    }
}
