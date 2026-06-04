using AzureFunctionsFundamentals.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace AzureFunctionsFundamentals.Modules.AppInsights.Exercise;

public sealed class OrderProcessingFunction(OrderTelemetryService telemetryService)
{
    // TODO: Implement the ProcessOrderAsync function.
    // Hints:
    // - Trigger: Use [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "telemetry/orders")] to expose the POST endpoint.
    // - Signature: Returns a Task<IActionResult> representing the async result (such as OkObjectResult or BadRequestObjectResult on failure).
    // - Logic: Read the Order from JSON request body, pass it to telemetryService.ProcessAsync, and return an appropriate IActionResult based on success/failure.
    [Function("ProcessOrder")]
    public Task<IActionResult> ProcessOrderAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "telemetry/orders")] HttpRequest request,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException("TODO: Implement the OrderProcessing HTTP function according to the exercise guidelines.");
    }
}
