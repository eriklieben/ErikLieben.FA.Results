using System;
using System.Collections.Generic;

namespace ErikLieben.FA.Results;

/// <summary>
/// Provides methods for combining multiple Result instances
/// </summary>
public static class ResultCombinators
{
    /// <summary>
    /// Combines multiple Results of the same type, returning the first success or all errors
    /// </summary>
    public static Result<T> Combine<T>(ReadOnlySpan<Result<T>> results)
    {
        var errorList = new List<ValidationError>();
        T? successValue = default;
        bool hasSuccess = false;

        foreach (var result in results)
        {
            if (result.IsSuccess && !hasSuccess)
            {
                successValue = result.Value;
                hasSuccess = true;
            }
            else if (result.IsFailure)
            {
                foreach (var error in result.Errors)
                {
                    errorList.Add(error);
                }
            }
        }

        return errorList.Count > 0
            ? Result<T>.Failure(errorList.ToArray())
            : Result<T>.Success(successValue!);
    }

    // Convenience overload to accept Span<Result<T>> directly
    public static Result<T> Combine<T>(Span<Result<T>> results) => Combine((ReadOnlySpan<Result<T>>)results);

    /// <summary>
    /// Combines multiple Results of the same type (params version)
    /// </summary>
    public static Result<T> Combine<T>(params Result<T>[] results) =>
        Combine((ReadOnlySpan<Result<T>>)results.AsSpan());

    /// <summary>
    /// Combines two Results into a tuple
    /// </summary>
    public static Result<(T1, T2)> Combine<T1, T2>(Result<T1> result1, Result<T2> result2)
    {
        var errorList = new List<ValidationError>();

        if (result1.IsFailure)
        {
            foreach (var error in result1.Errors)
                errorList.Add(error);
        }

        if (result2.IsFailure)
        {
            foreach (var error in result2.Errors)
                errorList.Add(error);
        }

        return errorList.Count > 0
            ? Result<(T1, T2)>.Failure(errorList.ToArray())
            : Result<(T1, T2)>.Success((result1.Value, result2.Value));
    }

    /// <summary>
    /// Combines three Results into a tuple
    /// </summary>
    public static Result<(T1, T2, T3)> Combine<T1, T2, T3>(Result<T1> result1, Result<T2> result2, Result<T3> result3)
    {
        var errorList = new List<ValidationError>();

        if (result1.IsFailure)
        {
            foreach (var error in result1.Errors)
                errorList.Add(error);
        }

        if (result2.IsFailure)
        {
            foreach (var error in result2.Errors)
                errorList.Add(error);
        }

        if (result3.IsFailure)
        {
            foreach (var error in result3.Errors)
                errorList.Add(error);
        }

        return errorList.Count > 0
            ? Result<(T1, T2, T3)>.Failure(errorList.ToArray())
            : Result<(T1, T2, T3)>.Success((result1.Value, result2.Value, result3.Value));
    }

    /// <summary>
    /// Combines four Results into a tuple
    /// </summary>
    public static Result<(T1, T2, T3, T4)> Combine<T1, T2, T3, T4>(
        Result<T1> result1, Result<T2> result2, Result<T3> result3, Result<T4> result4)
    {
        var errorList = new List<ValidationError>();

        if (result1.IsFailure)
        {
            foreach (var error in result1.Errors)
                errorList.Add(error);
        }

        if (result2.IsFailure)
        {
            foreach (var error in result2.Errors)
                errorList.Add(error);
        }

        if (result3.IsFailure)
        {
            foreach (var error in result3.Errors)
                errorList.Add(error);
        }

        if (result4.IsFailure)
        {
            foreach (var error in result4.Errors)
                errorList.Add(error);
        }

        return errorList.Count > 0
            ? Result<(T1, T2, T3, T4)>.Failure(errorList.ToArray())
            : Result<(T1, T2, T3, T4)>.Success((result1.Value, result2.Value, result3.Value, result4.Value));
    }

    /// <summary>
    /// Combines five Results into a tuple
    /// </summary>
    public static Result<(T1, T2, T3, T4, T5)> Combine<T1, T2, T3, T4, T5>(
        Result<T1> result1, Result<T2> result2, Result<T3> result3, Result<T4> result4, Result<T5> result5)
    {
        var errorList = new List<ValidationError>();

        if (result1.IsFailure)
        {
            foreach (var error in result1.Errors)
                errorList.Add(error);
        }

        if (result2.IsFailure)
        {
            foreach (var error in result2.Errors)
                errorList.Add(error);
        }

        if (result3.IsFailure)
        {
            foreach (var error in result3.Errors)
                errorList.Add(error);
        }

        if (result4.IsFailure)
        {
            foreach (var error in result4.Errors)
                errorList.Add(error);
        }

        if (result5.IsFailure)
        {
            foreach (var error in result5.Errors)
                errorList.Add(error);
        }

        return errorList.Count > 0
            ? Result<(T1, T2, T3, T4, T5)>.Failure(errorList.ToArray())
            : Result<(T1, T2, T3, T4, T5)>.Success((result1.Value, result2.Value, result3.Value, result4.Value,
                result5.Value));
    }

    /// <summary>
    /// Combines multiple non-generic Results
    /// </summary>
    public static Result Combine(ReadOnlySpan<Result> results)
    {
        var errorList = new List<ValidationError>();

        foreach (var result in results)
        {
            if (result.IsFailure)
            {
                foreach (var error in result.Errors)
                    errorList.Add(error);
            }
        }

        return errorList.Count > 0 ? Result.Failure(errorList.ToArray()) : Result.Success();
    }

    // Convenience overload to accept Span<Result> directly
    public static Result Combine(Span<Result> results) => Combine((ReadOnlySpan<Result>)results);

    /// <summary>
    /// Combines multiple non-generic Results (params version)
    /// </summary>
    public static Result Combine(params Result[] results) => Combine(results.AsSpan());
}
