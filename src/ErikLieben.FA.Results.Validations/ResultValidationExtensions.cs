using ErikLieben.FA.Specifications;

namespace ErikLieben.FA.Results.Validations;

/// <summary>
/// Extension methods for Result types in validation scenarios
/// </summary>
public static class ResultValidationExtensions
{
    /// <summary>
    /// Validates the success value of a Result using a specification
    /// </summary>
    /// <typeparam name="T">The type being validated</typeparam>
    /// <typeparam name="TSpec">The specification type</typeparam>
    /// <param name="result">The result to validate</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <param name="propertyName">Optional property name for the error</param>
    /// <returns>A Result that combines the original errors with validation errors</returns>
    public static Result<T> ValidateWith<T, TSpec>(this Result<T> result, string errorMessage, string? propertyName = null)
        where TSpec : Specification<T>, new()
    {
        if (result.IsFailure)
            return result;

        var spec = new TSpec();
        return spec.IsSatisfiedBy(result.Value)
            ? result
            : Result<T>.Failure(result.Errors.ToArray().Concat([new ValidationError(errorMessage, propertyName)]).ToArray());
    }

    /// <summary>
    /// Validates the success value of a Result using a custom condition
    /// </summary>
    /// <typeparam name="T">The type being validated</typeparam>
    /// <param name="result">The result to validate</param>
    /// <param name="condition">The validation condition</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <param name="propertyName">Optional property name for the error</param>
    /// <returns>A Result that combines the original errors with validation errors</returns>
    public static Result<T> ValidateWith<T>(this Result<T> result, Func<T, bool> condition, string errorMessage, string? propertyName = null)
    {
        ArgumentNullException.ThrowIfNull(condition);

        if (result.IsFailure)
            return result;

        return condition(result.Value)
            ? result
            : Result<T>.Failure(result.Errors.ToArray().Concat([new ValidationError(errorMessage, propertyName)]).ToArray());
    }

    /// <summary>
    /// Validates the success value of a Result and transforms it
    /// </summary>
    /// <typeparam name="T">The input type</typeparam>
    /// <typeparam name="TResult">The output type</typeparam>
    /// <param name="result">The result to validate</param>
    /// <param name="validator">Function that validates and transforms the value</param>
    /// <returns>A Result with the transformed value or validation errors</returns>
    public static Result<TResult> ValidateAndMap<T, TResult>(this Result<T> result, Func<T, Result<TResult>> validator)
    {
        ArgumentNullException.ThrowIfNull(validator);

        if (result.IsFailure)
            return Result<TResult>.Failure(result.Errors.ToArray());

        var validationResult = validator(result.Value);
        if (validationResult.IsFailure)
        {
            var combinedErrors = result.Errors.ToArray().Concat(validationResult.Errors.ToArray()).ToArray();
            return Result<TResult>.Failure(combinedErrors);
        }

        return validationResult;
    }

    /// <summary>
    /// Throws an exception if the Result is a failure (for scenarios where exceptions are needed)
    /// </summary>
    /// <typeparam name="T">The type</typeparam>
    /// <param name="result">The result</param>
    /// <param name="exceptionFactory">Optional factory for creating custom exceptions</param>
    /// <returns>The success value</returns>
    /// <exception cref="ValidationException">Thrown if the result is a failure</exception>
    public static T ThrowIfFailure<T>(this Result<T> result, Func<ReadOnlySpan<ValidationError>, Exception>? exceptionFactory = null)
    {
        if (result.IsSuccess)
            return result.Value;

        if (exceptionFactory is not null)
            throw exceptionFactory(result.Errors);

        throw new ValidationException(result.Errors.ToArray());
    }
}
