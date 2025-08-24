namespace ErikLieben.FA.Results;

/// <summary>
/// API error with message and optional property context
/// </summary>
public class ApiError(string message, string? propertyName = null)
{
    /// <summary>
    /// The error message
    /// </summary>
    public string Message { get; init; } = message;

    /// <summary>
    /// The property name associated with the error (for field validation)
    /// </summary>
    public string? PropertyName { get; init; } = propertyName;
}