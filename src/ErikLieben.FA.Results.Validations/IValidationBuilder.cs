using System.Runtime.CompilerServices;
using ErikLieben.FA.Specifications;

namespace ErikLieben.FA.Results.Validations;

public interface IValidationBuilder<T>
{
    /// <summary>
    /// Gets the validation errors
    /// </summary>
    ReadOnlySpan<ValidationError> Errors { get; }

    /// <summary>
    /// Gets whether there are any validation errors
    /// </summary>
    bool HasErrors { get; }

    void EnsureErrorsList();

    /// <summary>
    /// Validates using a specification instance
    /// </summary>
    /// <typeparam name="TSpec">The specification type</typeparam>
    /// <param name="value">The value to validate</param>
    /// <param name="message">The error message if validation fails</param>
    /// <param name="propertyName">Optional property name for the error</param>
    /// <returns>The builder for fluent chaining</returns>
    IValidationBuilder<T> ValidateWith<TSpec>(object value, string message, [CallerArgumentExpression(nameof(value))]string? propertyName = null)
        where TSpec : Specification<object>, new();

    /// <summary>
    /// Validates using a specification instance with strongly typed value
    /// </summary>
    /// <typeparam name="TSpec">The specification type</typeparam>
    /// <typeparam name="TValue">The value type</typeparam>
    /// <param name="value">The value to validate</param>
    /// <param name="message">The error message if validation fails</param>
    /// <param name="propertyName">Optional property name for the error</param>
    /// <returns>The builder for fluent chaining</returns>
    IValidationBuilder<T> ValidateWith<TSpec, TValue>(
        TValue value,
        string message,
        [CallerArgumentExpression(nameof(value))] string? propertyName = null)
        where TSpec : Specification<TValue>, new();

    /// <summary>
    /// Validates using a specification instance (with provided specification)
    /// </summary>
    /// <typeparam name="TSpec">The specification type</typeparam>
    /// <typeparam name="TValue">The value type</typeparam>
    /// <param name="value">The value to validate</param>
    /// <param name="specification">The specification instance</param>
    /// <param name="message">The error message if validation fails</param>
    /// <param name="propertyName">Optional property name for the error</param>
    /// <returns>The builder for fluent chaining</returns>
    IValidationBuilder<T> ValidateWith<TSpec, TValue>(
        TValue value,
        TSpec specification,
        string message,
        [CallerArgumentExpression(nameof(value))] string? propertyName = null)
        where TSpec : Specification<TValue>;

    /// <summary>
    /// Validates using a custom boolean condition
    /// </summary>
    /// <param name="isValid">The validation condition</param>
    /// <param name="message">The error message if validation fails</param>
    /// <param name="propertyName">Optional property name for the error</param>
    /// <returns>The builder for fluent chaining</returns>
    IValidationBuilder<T> ValidateCustom(bool isValid, string message, string? propertyName = null);

    /// <summary>
    /// Validates that a reference type is not null
    /// </summary>
    /// <typeparam name="TValue">The reference type</typeparam>
    /// <param name="value">The value to validate</param>
    /// <param name="message">The error message if validation fails</param>
    /// <param name="propertyName">Optional property name for the error</param>
    /// <returns>The builder for fluent chaining</returns>
    IValidationBuilder<T> ValidateNotNull<TValue>(
        TValue? value,
        string message,
        [CallerArgumentExpression(nameof(value))] string? propertyName = null)
        where TValue : class;

    /// <summary>
    /// Validates that a string is not null or empty
    /// </summary>
    /// <param name="value">The string to validate</param>
    /// <param name="message">The error message if validation fails</param>
    /// <param name="propertyName">Optional property name for the error</param>
    /// <returns>The builder for fluent chaining</returns>
    IValidationBuilder<T> ValidateNotNullOrEmpty(string? value, string message,[CallerArgumentExpression(nameof(value))] string? propertyName = null);

    /// <summary>
    /// Validates that a string is not null, empty, or whitespace
    /// </summary>
    /// <param name="value">The string to validate</param>
    /// <param name="message">The error message if validation fails</param>
    /// <param name="propertyName">Optional property name for the error</param>
    /// <returns>The builder for fluent chaining</returns>
    IValidationBuilder<T> ValidateNotNullOrWhiteSpace(string? value, string message,[CallerArgumentExpression(nameof(value))] string? propertyName = null);

    /// <summary>
    /// Validates that a value is within a specified range
    /// </summary>
    /// <typeparam name="TValue">The comparable type</typeparam>
    /// <param name="value">The value to validate</param>
    /// <param name="min">The minimum value</param>
    /// <param name="max">The maximum value</param>
    /// <param name="message">The error message if validation fails</param>
    /// <param name="propertyName">Optional property name for the error</param>
    /// <returns>The builder for fluent chaining</returns>
    IValidationBuilder<T> ValidateRange<TValue>(TValue value, TValue min, TValue max, string message,[CallerArgumentExpression(nameof(value))] string? propertyName = null)
        where TValue : IComparable<TValue>;

    /// <summary>
    /// Validates string length within a range
    /// </summary>
    /// <param name="value">The string to validate</param>
    /// <param name="minLength">The minimum length</param>
    /// <param name="maxLength">The maximum length</param>
    /// <param name="message">The error message if validation fails</param>
    /// <param name="propertyName">Optional property name for the error</param>
    /// <returns>The builder for fluent chaining</returns>
    IValidationBuilder<T> ValidateStringLength(string? value, int minLength, int maxLength, string message,[CallerArgumentExpression(nameof(value))] string? propertyName = null);

    /// <summary>
    /// Validates that a collection is not null or empty
    /// </summary>
    /// <typeparam name="TItem">The item type</typeparam>
    /// <param name="collection">The collection to validate</param>
    /// <param name="message">The error message if validation fails</param>
    /// <param name="propertyName">Optional property name for the error</param>
    /// <returns>The builder for fluent chaining</returns>
    IValidationBuilder<T> ValidateNotEmpty<TItem>(
        IEnumerable<TItem>? collection,
        string message,
        [CallerArgumentExpression(nameof(collection))] string? propertyName = null);

    /// <summary>
    /// Builds a Result with the provided value
    /// </summary>
    /// <param name="value">The value to wrap in the result</param>
    /// <returns>A Result indicating success or failure</returns>
    Result<T> Build(T value);

    /// <summary>
    /// Builds a Result with a different type
    /// </summary>
    /// <typeparam name="TResult">The result type</typeparam>
    /// <param name="value">The value to wrap in the result</param>
    /// <returns>A Result indicating success or failure</returns>
    Result<TResult> Build<TResult>(TResult value);

    /// <summary>
    /// Builds a Result using a factory function
    /// </summary>
    /// <param name="valueFactory">Factory function to create the value</param>
    /// <returns>A Result indicating success or failure</returns>
    Result<T> Build(Func<T> valueFactory);

    /// <summary>
    /// Builds a Result using a factory function with different return type
    /// </summary>
    /// <typeparam name="TResult">The result type</typeparam>
    /// <param name="valueFactory">Factory function to create the value</param>
    /// <returns>A Result indicating success or failure</returns>
    Result<TResult> Build<TResult>(Func<TResult> valueFactory);

    /// <summary>
    /// Builds a non-generic Result
    /// </summary>
    /// <returns>A Result indicating success or failure</returns>
    Result Build();
}