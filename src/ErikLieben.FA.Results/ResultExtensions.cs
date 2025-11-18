namespace ErikLieben.FA.Results;

/// <summary>
/// Extension methods for Result types
/// </summary>
public static class ResultExtensions
{
    // Instance Match(Action, ...) exist on Result and Result<T> to preserve identity.
    // The transforming Match<TResult>(...) overloads are provided as extensions to avoid ambiguous
    // overload resolution with expression-bodied lambdas which return a value.

    /// <summary>
    /// Transforms the result based on its state (generic Result<T> -> TResult)
    /// </summary>
    public static TResult Match<T, TResult>(this Result<T> result, Func<T, TResult> onSuccess, Func<ReadOnlySpan<ValidationError>, TResult> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);
        return result.IsSuccess ? onSuccess(result.Value) : onFailure(result.Errors);
    }

    /// <summary>
    /// Transforms the result based on its state (non-generic Result -> TResult)
    /// </summary>
    public static TResult Match<TResult>(this Result result, Func<TResult> onSuccess, Func<ReadOnlySpan<ValidationError>, TResult> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);
        return result.IsSuccess ? onSuccess() : onFailure(result.Errors);
    }

    /// <summary>
    /// Adds additional context to validation errors
    /// </summary>
    public static Result<T> MapErrors<T>(this Result<T> result, Func<ValidationError, ValidationError> errorMapper)
    {
        ArgumentNullException.ThrowIfNull(errorMapper);

        if (result.IsSuccess)
            return result;

        // Optimize: iterate span directly and create array with exact size
        var errors = result.Errors;
        var mappedErrors = new ValidationError[errors.Length];
        for (int i = 0; i < errors.Length; i++)
        {
            mappedErrors[i] = errorMapper(errors[i]);
        }

        return Result<T>.Failure(mappedErrors);
    }

    /// <summary>
    /// Filters errors based on a predicate
    /// </summary>
    public static Result<T> FilterErrors<T>(this Result<T> result, Func<ValidationError, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        if (result.IsSuccess)
            return result;

        // Optimize: use List with proper initial capacity to reduce allocations
        var errors = result.Errors;
        var filteredErrors = new List<ValidationError>(errors.Length);
        foreach (var error in errors)
        {
            if (predicate(error))
                filteredErrors.Add(error);
        }

        return filteredErrors.Count == 0
            ? Result<T>.Success(default!)
            : Result<T>.Failure(filteredErrors.ToArray());
    }

    /// <summary>
    /// Converts a Result&lt;T&gt; to a non-generic Result
    /// </summary>
    public static Result ToResult<T>(this Result<T> result)
    {
        return result.IsSuccess ? Result.Success() : Result.Failure(result.Errors.ToArray());
    }

    /// <summary>
    /// Gets all error messages as a single string
    /// </summary>
    public static string GetErrorMessages<T>(this Result<T> result, string separator = "; ")
    {
        if (result.IsSuccess)
            return string.Empty;

        var errors = result.Errors;
        if (errors.Length == 0)
            return string.Empty;

        if (errors.Length == 1)
            return errors[0].ToString();

        // Optimize: iterate span directly without ToArray() + LINQ
        var messages = new string[errors.Length];
        for (int i = 0; i < errors.Length; i++)
        {
            messages[i] = errors[i].ToString();
        }

        return string.Join(separator, messages);
    }

    /// <summary>
    /// Gets all error messages as a single string (non-generic)
    /// </summary>
    public static string GetErrorMessages(this Result result, string separator = "; ")
    {
        if (result.IsSuccess)
            return string.Empty;

        var errors = result.Errors;
        if (errors.Length == 0)
            return string.Empty;

        if (errors.Length == 1)
            return errors[0].ToString();

        // Optimize: iterate span directly without ToArray() + LINQ
        var messages = new string[errors.Length];
        for (int i = 0; i < errors.Length; i++)
        {
            messages[i] = errors[i].ToString();
        }

        return string.Join(separator, messages);
    }
}
