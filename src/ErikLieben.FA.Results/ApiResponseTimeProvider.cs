namespace ErikLieben.FA.Results;

/// <summary>
/// Shared, non-generic holder for the TimeProvider
/// </summary>
public static class ApiResponseTimeProvider
{
    private static TimeProvider timeProvider = TimeProvider.System;
    private static readonly AsyncLocal<TimeProvider?> current = new();

    /// <summary>
    /// Shared TimeProvider for all ApiResponse{T} types.
    /// Uses AsyncLocal to avoid cross-test interference.
    /// </summary>
    public static TimeProvider SharedTimeProvider
    {
        get => current.Value ?? timeProvider;
        set => current.Value = value ?? TimeProvider.System;
    }
}
