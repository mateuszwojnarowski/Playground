using AzureFunctionsFundamentals.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace AzureFunctionsFundamentals.Modules.AppInsights.Exercise;

public sealed class OrderProcessingFunction(OrderTelemetryService telemetryService)
{
    [Function("ProcessOrder")]
    public async Task<IActionResult> ProcessOrderAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "telemetry/orders")] HttpRequest request,
        CancellationToken cancellationToken)
    {
        var order = await request.ReadFromJsonAsync<Order>(cancellationToken);
        var result = await telemetryService.ProcessAsync(order, cancellationToken);

        if (!result.IsSuccess)
        {
            return new BadRequestObjectResult(new { error = result.Error });
        }

        return new OkObjectResult(new { message = "Order processed." });
    }
}
