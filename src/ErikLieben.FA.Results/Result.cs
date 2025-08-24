using System.Collections.Generic;

namespace ErikLieben.FA.Results;

/// <summary>
/// Non-generic Result for operations that don't return a value
/// </summary>
public sealed class Result
{
    private readonly ValidationError[]? errors;

    private Result(ValidationError[] errors)
    {
        this.errors = errors;
        IsSuccess = false;
    }

    private Result(bool success)
    {
        errors = null;
        IsSuccess = success;
    }

    /// <summary>
    /// Indicates whether the operation was successful
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Indicates whether the operation failed
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the validation errors. Empty span if the result is successful.
    /// </summary>
    public ReadOnlySpan<ValidationError> Errors =>
        IsSuccess ? ReadOnlySpan<ValidationError>.Empty : errors.AsSpan();

    /// <summary>
    /// Creates a successful result
    /// </summary>
    public static Result Success() => new(true);

    /// <summary>
    /// Creates a failed result with a single error
    /// </summary>
    public static Result Failure(ValidationError error) => new([error]);

    /// <summary>
    /// Creates a failed result with multiple errors
    /// </summary>
    public static Result Failure(ValidationError[] errors) => new(errors);

    /// <summary>
    /// Creates a failed result with a simple error message
    /// </summary>
    public static Result Failure(string message) =>
        new([ValidationError.Create(message)]);

    /// <summary>
    /// Transforms to a Result&lt;T&gt; with the provided value if successful
    /// </summary>
    public Result<T> Map<T>(T value) =>
        IsSuccess ? Result<T>.Success(value) : Result<T>.Failure(errors!);

    /// <summary>
    /// Chains operations that return Results
    /// </summary>
    public Result Bind(Func<Result> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);
        return IsSuccess ? binder() : this;
    }

    /// <summary>
    /// Pattern match with actions; returns the same instance.
    /// </summary>
    public Result Match(Action onSuccess, Action<ReadOnlySpan<ValidationError>> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);
        if (IsSuccess) onSuccess(); else onFailure(Errors);
        return this;
    }


    /// <summary>
    /// Chains operations that return Result&lt;T&gt;
    /// </summary>
    public Result<T> Bind<T>(Func<Result<T>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);
        return IsSuccess ? binder() : Result<T>.Failure(errors!);
    }

    // Static helpers migrated from ResultFunctional

    /// <summary>
    /// switch: Lift a pure function into the Result world.
    /// Transforms f: T -> TResult into f': T -> Result<TResult>
    /// </summary>
    public static Func<T, Result<TResult>> Switch<T, TResult>(Func<T, TResult> f)
    {
        ArgumentNullException.ThrowIfNull(f);
        return t => Result<TResult>.Success(f(t));
    }

    /// <summary>
    /// tryCatch: Wrap a function that may throw into a Result-producing function.
    /// </summary>
    public static Func<T, Result<TResult>> Try<T, TResult>(Func<T, TResult> f, Func<Exception, ValidationError> errorMapper)
    {
        ArgumentNullException.ThrowIfNull(f);
        ArgumentNullException.ThrowIfNull(errorMapper);
        return t =>
        {
            try
            {
                return Result<TResult>.Success(f(t));
            }
            catch (Exception ex)
            {
                return Result<TResult>.Failure(errorMapper(ex));
            }
        };
    }

    /// <summary>
    /// Kleisli composition: compose two functions that return Result.
    /// g ∘K f where f: A -> Result<B> and g: B -> Result<C> yields A -> Result<C>.
    /// </summary>
    public static Func<TA, Result<TC>> Compose<TA, TB, TC>(Func<TA, Result<TB>> f, Func<TB, Result<TC>> g)
    {
        ArgumentNullException.ThrowIfNull(f);
        ArgumentNullException.ThrowIfNull(g);
        return a => f(a).Bind(g);
    }

    /// <summary>
    /// apply (ap): Apply a Result-wrapped function to a Result-wrapped value.
    /// Accumulates validation errors when either or both are failures.
    /// </summary>
    public static Result<TResult> Apply<T, TResult>(Result<Func<T, TResult>> rf, Result<T> rx)
    {
        if (rf.IsSuccess && rx.IsSuccess)
        {
            var f = rf.Value;
            return Result<TResult>.Success(f(rx.Value));
        }

        // One or both are failures: accumulate errors
        var errors = new List<ValidationError>();
        if (rf.IsFailure)
            errors.AddRange(rf.Errors.ToArray());
        if (rx.IsFailure)
            errors.AddRange(rx.Errors.ToArray());

        return Result<TResult>.Failure(errors.ToArray());
    }

    /// <summary>
    /// Lift a two-arg pure function into the Result world while accumulating errors.
    /// </summary>
    public static Result<TResult> Lift<T1, T2, TResult>(Func<T1, T2, TResult> f, Result<T1> r1, Result<T2> r2)
    {
        ArgumentNullException.ThrowIfNull(f);
        if (r1.IsSuccess && r2.IsSuccess)
        {
            return Result<TResult>.Success(f(r1.Value, r2.Value));
        }
        var errors = new List<ValidationError>();
        if (r1.IsFailure) errors.AddRange(r1.Errors.ToArray());
        if (r2.IsFailure) errors.AddRange(r2.Errors.ToArray());
        return Result<TResult>.Failure(errors.ToArray());
    }

    /// <summary>
    /// Lift a three-arg pure function into the Result world while accumulating errors.
    /// </summary>
    public static Result<TResult> Lift<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> f, Result<T1> r1, Result<T2> r2, Result<T3> r3)
    {
        ArgumentNullException.ThrowIfNull(f);
        if (r1.IsSuccess && r2.IsSuccess && r3.IsSuccess)
        {
            return Result<TResult>.Success(f(r1.Value, r2.Value, r3.Value));
        }
        var errors = new List<ValidationError>();
        if (r1.IsFailure) errors.AddRange(r1.Errors.ToArray());
        if (r2.IsFailure) errors.AddRange(r2.Errors.ToArray());
        if (r3.IsFailure) errors.AddRange(r3.Errors.ToArray());
        return Result<TResult>.Failure(errors.ToArray());
    }
}
