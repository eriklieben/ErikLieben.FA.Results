using ErikLieben.FA.Specifications;

namespace ErikLieben.FA.Results.Validations;

/// <summary>
/// Extension methods for integrating Specifications with the validation system
/// </summary>
public static class SpecificationValidationExtensions
{
    /// <summary>
    /// Validates a value using this specification and returns a Result
    /// </summary>
    /// <typeparam name="T">The type being validated</typeparam>
    /// <param name="specification">The specification</param>
    /// <param name="value">The value to validate</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <param name="propertyName">Optional property name for the error</param>
    /// <returns>A Result indicating success or failure</returns>
    public static Result<T> ValidateResult<T>(
        this Specification<T> specification,
        T value,
        string errorMessage,
        string? propertyName = null)
    {
        ArgumentNullException.ThrowIfNull(specification);
        return specification.IsSatisfiedBy(value)
            ? Result<T>.Success(value)
            : Result<T>.Failure(errorMessage, propertyName ?? string.Empty);
    }

    /// <summary>
    /// Validates multiple values using this specification
    /// </summary>
    /// <typeparam name="T">The type being validated</typeparam>
    /// <param name="specification">The specification</param>
    /// <param name="values">The values to validate</param>
    /// <param name="errorMessage">The error message template if validation fails</param>
    /// <returns>Results for each value</returns>
    public static IEnumerable<Result<T>> ValidateMany<T>(
        this Specification<T> specification,
        IEnumerable<T> values,
        string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(specification);
        ArgumentNullException.ThrowIfNull(values);

        return values.Select((value, index) =>
            specification.ValidateResult(value, errorMessage, $"[{index}]"));
    }

    /// <summary>
    /// Validates all values and combines results
    /// </summary>
    /// <typeparam name="T">The type being validated</typeparam>
    /// <param name="specification">The specification</param>
    /// <param name="values">The values to validate</param>
    /// <param name="errorMessage">The error message template if validation fails</param>
    /// <returns>A single Result containing all values or all errors</returns>
    public static Result<T[]> ValidateAll<T>(
        this Specification<T> specification,
        IEnumerable<T> values,
        string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(specification);
        ArgumentNullException.ThrowIfNull(values);

        var results = specification.ValidateMany(values, errorMessage).ToArray();
        var errors = results.Where(r => r.IsFailure).SelectMany(r => r.Errors.ToArray()).ToArray();

        return errors.Length > 0
            ? Result<T[]>.Failure(errors)
            : Result<T[]>.Success(results.Select(r => r.Value).ToArray());
    }
}
