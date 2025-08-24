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

        var mappedErrors = new ValidationError[result.Errors.Length];
        for (int i = 0; i < result.Errors.Length; i++)
        {
            mappedErrors[i] = errorMapper(result.Errors[i]);
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

        var filteredErrors = new List<ValidationError>();
        foreach (var error in result.Errors)
        {
            if (predicate(error))
                filteredErrors.Add(error);
        }

        return filteredErrors.Count == 0
            ? Result<T>.Success(default!) // This is a bit odd, but maintains the pattern
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

        return string.Join(separator, result.Errors.ToArray().Select(e => e.ToString()));
    }

    /// <summary>
    /// Gets all error messages as a single string (non-generic)
    /// </summary>
    public static string GetErrorMessages(this Result result, string separator = "; ")
    {
        if (result.IsSuccess)
            return string.Empty;

        return string.Join(separator, result.Errors.ToArray().Select(e => e.ToString()));
    }
}
