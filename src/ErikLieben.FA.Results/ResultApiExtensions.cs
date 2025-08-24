using Microsoft.AspNetCore.Mvc;

namespace ErikLieben.FA.Results;

/// <summary>
/// Extension methods for converting Result<T> to API responses
/// </summary>
public static class ResultApiExtensions
{
    /// <summary>
    /// Converts Result<T> to ApiResponse<T>
    /// </summary>
    public static ApiResponse<T> ToApiResponse<T>(this Result<T> result, string? successMessage = null, string? failureMessage = null)
    {
        return result.IsSuccess
            ? ApiResponse<T>.Success(result.Value, successMessage)
            : ApiResponse<T>.Failure(
                result.Errors.ToArray().Select(e => new ApiError(e.Message, e.PropertyName)),
                failureMessage);
    }

    /// <summary>
    /// Converts Result<T> to IActionResult (200 OK or 400 Bad Request)
    /// </summary>
    public static IActionResult ToActionResult<T>(this Result<T> result, string? successMessage = null, string? failureMessage = null)
    {
        var apiResponse = result.ToApiResponse(successMessage, failureMessage);

        return result.IsSuccess
            ? new OkObjectResult(apiResponse)
            : new BadRequestObjectResult(apiResponse);
    }

    /// <summary>
    /// Converts Result<T> to IActionResult with custom status codes
    /// </summary>
    public static IActionResult ToActionResult<T>(this Result<T> result,
        int successStatusCode,
        int failureStatusCode,
        string? successMessage = null,
        string? failureMessage = null)
    {
        var apiResponse = result.ToApiResponse(successMessage, failureMessage);

        return result.IsSuccess
            ? new ObjectResult(apiResponse) { StatusCode = successStatusCode }
            : new ObjectResult(apiResponse) { StatusCode = failureStatusCode };
    }

    /// <summary>
    /// Converts Result<T> to CreatedResult for POST operations (201 Created or 400 Bad Request)
    /// </summary>
    public static IActionResult ToCreatedResult<T>(this Result<T> result,
        string location,
        string? successMessage = null,
        string? failureMessage = null)
    {
        var apiResponse = result.ToApiResponse(successMessage, failureMessage);

        return result.IsSuccess
            ? new CreatedResult(location, apiResponse)
            : new BadRequestObjectResult(apiResponse);
    }

    /// <summary>
    /// Converts Result<T> to CreatedAtActionResult for POST operations
    /// </summary>
    public static IActionResult ToCreatedAtActionResult<T>(this Result<T> result,
        string actionName,
        string? controllerName = null,
        object? routeValues = null,
        string? successMessage = null,
        string? failureMessage = null)
    {
        var apiResponse = result.ToApiResponse(successMessage, failureMessage);

        return result.IsSuccess
            ? new CreatedAtActionResult(actionName, controllerName, routeValues, apiResponse)
            : new BadRequestObjectResult(apiResponse);
    }
}
