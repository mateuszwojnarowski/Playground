namespace AzureFunctionsFundamentals.Modules.ServiceBusQueue;

public sealed class OrderValidationException : InvalidOperationException
{
    public OrderValidationException(IEnumerable<string> errors)
        : base($"Order failed validation: {string.Join("; ", errors)}")
    {
        Errors = errors.ToArray();
    }

    public IReadOnlyList<string> Errors { get; }
}
