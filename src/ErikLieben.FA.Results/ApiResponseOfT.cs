namespace ErikLieben.FA.Results;

/// <summary>
/// Standard API response wrapper
/// </summary>
/// <typeparam name="T">The data type</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Indicates if the operation was successful
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// The data payload (only present on success)
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// Error details (only present on failure)
    /// </summary>
    public ApiError[]? Errors { get; init; }

    /// <summary>
    /// Optional message for additional context
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Response timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// Creates a successful API response
    /// </summary>
    public static ApiResponse<T> Success(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            IsSuccess = true,
            Data = data,
            Message = message,
            Timestamp = ApiResponseTimeProvider.SharedTimeProvider.GetUtcNow()
        };
    }

    /// <summary>
    /// Creates a failed API response
    /// </summary>
    public static ApiResponse<T> Failure(IEnumerable<ApiError> errors, string? message = null)
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            Errors = errors.ToArray(),
            Message = message,
            Timestamp = ApiResponseTimeProvider.SharedTimeProvider.GetUtcNow()
        };
    }
}
