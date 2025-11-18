using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ErikLieben.FA.Results;

public static class ResultMinimalApiExtensions
{
    /// <summary>
    /// Converts Result<T> to strongly-typed IResult (200 OK or 400 Bad Request) for Minimal APIs using the ApiResponse<T> shape.
    /// </summary>
    public static Results<Ok<ApiResponse<T>>, BadRequest<ApiResponse<T>>> ToHttpResult<T>(
        this Result<T> result,
        string? successMessage = null,
        string? failureMessage = null)
    {
        var apiResponse = result.ToApiResponse(successMessage, failureMessage);
        return result.IsSuccess ? TypedResults.Ok(apiResponse) : TypedResults.BadRequest(apiResponse);
    }

    /// <summary>
    /// Converts Result<T> to strongly-typed IResult with custom status codes for Minimal APIs using the ApiResponse<T> shape.
    /// </summary>
    public static JsonHttpResult<ApiResponse<T>> ToHttpResult<T>(this Result<T> result,
        int successStatusCode,
        int failureStatusCode,
        string? successMessage = null,
        string? failureMessage = null)
    {
        var apiResponse = result.ToApiResponse(successMessage, failureMessage);
        var statusCode = result.IsSuccess ? successStatusCode : failureStatusCode;
        return TypedResults.Json(apiResponse, statusCode: statusCode);
    }

    /// <summary>
    /// Converts Result<T> to strongly-typed IResult Created (201) or BadRequest (400) for Minimal APIs using the ApiResponse<T> shape.
    /// </summary>
    public static Results<Created<ApiResponse<T>>, BadRequest<ApiResponse<T>>> ToCreatedHttpResult<T>(
        this Result<T> result,
        string location,
        string? successMessage = null,
        string? failureMessage = null)
    {
        var apiResponse = result.ToApiResponse(successMessage, failureMessage);
        return result.IsSuccess ? TypedResults.Created(location, apiResponse) : TypedResults.BadRequest(apiResponse);
    }

    /// <summary>
    /// Converts Result<T> to strongly-typed IResult CreatedAtRoute (201) or BadRequest (400) for Minimal APIs using the ApiResponse<T> shape.
    /// </summary>
    public static Results<CreatedAtRoute<ApiResponse<T>>, BadRequest<ApiResponse<T>>> ToCreatedAtRouteHttpResult<T>(
        this Result<T> result,
        string routeName,
        object? routeValues = null,
        string? successMessage = null,
        string? failureMessage = null)
    {
        var apiResponse = result.ToApiResponse(successMessage, failureMessage);
        return result.IsSuccess
            ? TypedResults.CreatedAtRoute(apiResponse, routeName, routeValues)
            : TypedResults.BadRequest(apiResponse);
    }

    /// <summary>
    /// Maps Result<T> to Result<TContract> using the provided mapper and converts it to strongly-typed IResult
    /// using the library's ApiResponse<T> shape.
    /// </summary>
    public static Results<Ok<ApiResponse<TContract>>, BadRequest<ApiResponse<TContract>>> ToHttpResult<T, TContract>(
        this Result<T> source,
        Func<T, TContract> mapper,
        string successMessage,
        string failedMessage)
    {
        var mapped = source.Map(mapper);
        return mapped.ToHttpResult(successMessage, failedMessage);
    }
}
