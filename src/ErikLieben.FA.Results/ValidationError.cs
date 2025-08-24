namespace ErikLieben.FA.Results;

/// <summary>
/// Represents a validation error with message and optional context
/// </summary>
public readonly struct ValidationError(string message, string? propertyName = null)
{
    /// <summary>
    /// The error message
    /// </summary>
    public string Message { get; } = message ??
                                     throw new ArgumentNullException(nameof(message));

    /// <summary>
    /// The property name associated with the error (optional)
    /// </summary>
    public string? PropertyName { get; } = propertyName;

    /// <summary>
    /// Returns a formatted string representation of the error
    /// </summary>
    public override string ToString() =>
        PropertyName is null ? Message : $"{PropertyName}: {Message}";

    /// <summary>
    /// Creates a validation error with just a message
    /// </summary>
    public static ValidationError Create(string message) => new(message);

    /// <summary>
    /// Creates a validation error with message and property name
    /// </summary>
    public static ValidationError Create(string message, string propertyName) =>
        new(message, propertyName);
}
