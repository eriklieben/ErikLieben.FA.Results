namespace ErikLieben.FA.Results.Validations;

/// <summary>
/// Exception thrown when validation fails (for scenarios that require exceptions)
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// The validation errors that caused the exception
    /// </summary>
    public ValidationError[] Errors { get; }

    /// <summary>
    /// Creates a new ValidationException with the provided errors
    /// </summary>
    /// <param name="errors">The validation errors</param>
    public ValidationException(ValidationError[] errors)
        : base(CreateMessage(errors))
    {
        Errors = errors;
    }

    /// <summary>
    /// Creates a new ValidationException with a single error
    /// </summary>
    /// <param name="error">The validation error</param>
    public ValidationException(ValidationError error)
        : this([error])
    {
    }

    /// <summary>
    /// Creates a new ValidationException with a simple message
    /// </summary>
    /// <param name="message">The error message</param>
    public ValidationException(string message)
        : this([ValidationError.Create(message)])
    {
    }

    private static string CreateMessage(ValidationError[] errors)
    {
        if (errors.Length == 0)
            return "Validation failed";

        if (errors.Length == 1)
            return errors[0].ToString();

        return $"Validation failed with {errors.Length} errors: {string.Join("; ", errors.Select(e => e.ToString()))}";
    }
}
