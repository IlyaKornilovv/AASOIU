namespace Homework3.Variant18.Models;


public sealed class ErrorViewModel
{
    
    public string? RequestId { get; init; }

    
    public bool ShowRequestId => !string.IsNullOrWhiteSpace(RequestId);
}
