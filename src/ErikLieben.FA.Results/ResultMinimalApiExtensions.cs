using Microsoft.AspNetCore.Http;
using static Microsoft.AspNetCore.Http.Results;

namespace ErikLieben.FA.Results;

public static class ResultMinimalApiExtensions
{
    /// <summary>
    /// Converts Result<T> to IResult (200 OK or 400 Bad Request) for Minimal APIs using the ApiResponse<T> shape.
    /// </summary>
    public static IResult ToHttpResult<T>(this Result<T> result, string? successMessage = null, string? failureMessage = null)
    {
        var apiResponse = result.ToApiResponse(successMessage, failureMessage);
        return result.IsSuccess ? Ok(apiResponse) : BadRequest(apiResponse);
    }

    /// <summary>
    /// Converts Result<T> to IResult with custom status codes for Minimal APIs using the ApiResponse<T> shape.
    /// </summary>
    public static IResult ToHttpResult<T>(this Result<T> result,
        int successStatusCode,
        int failureStatusCode,
        string? successMessage = null,
        string? failureMessage = null)
    {
        var apiResponse = result.ToApiResponse(successMessage, failureMessage);
        var statusCode = result.IsSuccess ? successStatusCode : failureStatusCode;
        return Json(apiResponse, statusCode: statusCode);
    }

    /// <summary>
    /// Converts Result<T> to IResult Created (201) or BadRequest (400) for Minimal APIs using the ApiResponse<T> shape.
    /// </summary>
    public static IResult ToCreatedHttpResult<T>(this Result<T> result,
        string location,
        string? successMessage = null,
        string? failureMessage = null)
    {
        var apiResponse = result.ToApiResponse(successMessage, failureMessage);
        return result.IsSuccess ? Created(location, apiResponse) : BadRequest(apiResponse);
    }

    /// <summary>
    /// Converts Result<T> to IResult CreatedAtRoute (201) or BadRequest (400) for Minimal APIs using the ApiResponse<T> shape.
    /// </summary>
    public static IResult ToCreatedAtRouteHttpResult<T>(this Result<T> result,
        string routeName,
        object? routeValues = null,
        string? successMessage = null,
        string? failureMessage = null)
    {
        var apiResponse = result.ToApiResponse(successMessage, failureMessage);
        return result.IsSuccess ? CreatedAtRoute(routeName, routeValues, apiResponse) : BadRequest(apiResponse);
    }

    /// <summary>
    /// Maps Result<T> to Result<TContract> using the provided mapper and converts it to IResult
    /// using the library's ApiResponse<T> shape.
    /// </summary>
    public static IResult ToHttpResult<T, TContract>(
        this Result<T> source,
        Func<T, TContract> mapper,
        string successMessage,
        string failedMessage)
    {
        var mapped = source.Map(mapper);
        return mapped.ToHttpResult(successMessage, failedMessage);
    }
}
