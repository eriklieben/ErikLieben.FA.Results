using ErikLieben.FA.Specifications;

namespace ErikLieben.FA.Results.Validations;

/// <summary>
/// Static helper methods for creating validation builders
/// </summary>
public static class ValidationBuilder
{
    /// <summary>
    /// Creates a new validation builder for the specified type
    /// </summary>
    /// <typeparam name="T">The type being validated</typeparam>
    /// <returns>A new validation builder</returns>
    public static ValidationBuilder<T> For<T>() => new();

    /// <summary>
    /// Creates a new validation builder for the specified type (with value for context)
    /// </summary>
    /// <typeparam name="T">The type being validated</typeparam>
    /// <param name="value">The value being validated (for context)</param>
    /// <returns>A new validation builder</returns>
    public static ValidationBuilder<T> For<T>(T value) => new();

    /// <summary>
    /// Validates a single value using a specification
    /// </summary>
    /// <typeparam name="T">The value type</typeparam>
    /// <typeparam name="TSpec">The specification type</typeparam>
    /// <param name="value">The value to validate</param>
    /// <param name="message">The error message if validation fails</param>
    /// <param name="propertyName">Optional property name for the error</param>
    /// <returns>A Result indicating success or failure</returns>
    public static Result<T> ValidateSingle<T, TSpec>(T value, string message, string? propertyName = null)
        where TSpec : Specification<T>, new()
    {
        var spec = new TSpec();
        return spec.IsSatisfiedBy(value)
            ? Result<T>.Success(value)
            : Result<T>.Failure(message, propertyName ?? string.Empty);
    }

    /// <summary>
    /// Validates that a reference type is not null
    /// </summary>
    /// <typeparam name="T">The reference type</typeparam>
    /// <param name="value">The value to validate</param>
    /// <param name="message">The error message if validation fails</param>
    /// <param name="propertyName">Optional property name for the error</param>
    /// <returns>A Result indicating success or failure</returns>
    public static Result<T> ValidateNotNull<T>(T? value, string message, string? propertyName = null)
        where T : class
    {
        return value is not null
            ? Result<T>.Success(value)
            : Result<T>.Failure(message, propertyName ?? string.Empty);
    }

    /// <summary>
    /// Validates that a string is not null or empty
    /// </summary>
    /// <param name="value">The string to validate</param>
    /// <param name="message">The error message if validation fails</param>
    /// <param name="propertyName">Optional property name for the error</param>
    /// <returns>A Result indicating success or failure</returns>
    public static Result<string> ValidateNotNullOrEmpty(string? value, string message, string? propertyName = null)
    {
        return !string.IsNullOrEmpty(value)
            ? Result<string>.Success(value)
            : Result<string>.Failure(message, propertyName ?? string.Empty);
    }

    /// <summary>
    /// Validates that a string is not null, empty, or whitespace
    /// </summary>
    /// <param name="value">The string to validate</param>
    /// <param name="message">The error message if validation fails</param>
    /// <param name="propertyName">Optional property name for the error</param>
    /// <returns>A Result indicating success or failure</returns>
    public static Result<string> ValidateNotNullOrWhiteSpace(string? value, string message, string? propertyName = null)
    {
        return !string.IsNullOrWhiteSpace(value)
            ? Result<string>.Success(value)
            : Result<string>.Failure(message, propertyName ?? string.Empty);
    }

    /// <summary>
    /// Validates that a value is within a specified range
    /// </summary>
    /// <typeparam name="T">The comparable type</typeparam>
    /// <param name="value">The value to validate</param>
    /// <param name="min">The minimum value</param>
    /// <param name="max">The maximum value</param>
    /// <param name="message">The error message if validation fails</param>
    /// <param name="propertyName">Optional property name for the error</param>
    /// <returns>A Result indicating success or failure</returns>
    public static Result<T> ValidateRange<T>(T value, T min, T max, string message, string? propertyName = null)
        where T : IComparable<T>
    {
        return value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0
            ? Result<T>.Success(value)
            : Result<T>.Failure(message, propertyName ?? string.Empty);
    }
}
