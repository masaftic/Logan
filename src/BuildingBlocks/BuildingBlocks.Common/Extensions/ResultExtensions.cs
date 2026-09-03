using BuildingBlocks.Common.Results;
using Microsoft.AspNetCore.Http;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace BuildingBlocks.Common.Extensions;

public static class ResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result, int successStatusCode = StatusCodes.Status200OK)
    {
        if (result.IsSuccess)
        {
            return successStatusCode switch
            {
                StatusCodes.Status200OK => HttpResults.Ok(result.Value),
                StatusCodes.Status201Created => HttpResults.Created(string.Empty, result.Value),
                StatusCodes.Status202Accepted => HttpResults.Accepted(string.Empty, result.Value),
                _ => HttpResults.StatusCode(successStatusCode)
            };
        }

        return ToProblemDetails(result.FirstError);
    }

    public static IResult ToHttpResult(this Result result, int successStatusCode = StatusCodes.Status200OK)
    {
        if (result.IsSuccess)
        {
            return successStatusCode switch
            {
                StatusCodes.Status200OK => HttpResults.Ok(),
                StatusCodes.Status202Accepted => HttpResults.Accepted(),
                StatusCodes.Status204NoContent => HttpResults.NoContent(),
                _ => HttpResults.StatusCode(successStatusCode)
            };
        }

        return ToProblemDetails(result.FirstError);
    }

    public static IResult ToCreatedResult<T>(this Result<T> result, Func<T, string> uri)
    {
        if (result.IsSuccess)
        {
            return HttpResults.Created(uri(result.Value), result.Value);
        }

        return ToProblemDetails(result.FirstError);
    }

    public static IResult ToAcceptedResult(this Result result, string? uri = null)
    {
        if (result.IsSuccess)
        {
            return HttpResults.Accepted(uri);
        }

        return ToProblemDetails(result.FirstError);
    }

    public static IResult ToNoContentResult(this Result result)
    {
        if (result.IsSuccess)
        {
            return HttpResults.NoContent();
        }

        return ToProblemDetails(result.FirstError);
    }

    public static IResult ToCustomResult<T>(this Result<T> result, Func<T, IResult> onSuccess)
    {
        if (result.IsSuccess)
        {
            return onSuccess(result.Value);
        }

        return ToProblemDetails(result.FirstError);
    }

    public static IResult ToProblemDetails(this Error error)
    {
        var statusCode = error.CustomStatusCode ?? error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.ExternalService => StatusCodes.Status502BadGateway,
            ErrorType.Unexpected => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status400BadRequest
        };

        var titleMsg = error.Type switch
        {
            ErrorType.NotFound => "Resource not found",
            ErrorType.Conflict => "Resource conflict",
            ErrorType.Validation => "Validation error",
            ErrorType.Unauthorized => "Unauthorized access",
            ErrorType.Forbidden => "Forbidden access",
            ErrorType.ExternalService => "External service error",
            ErrorType.Unexpected => "Unexpected error",
            _ => "Validation error"
        };

        var metadata = error.Metadata is not null
            ? new Dictionary<string, object?>(error.Metadata.ToDictionary(kvp => kvp.Key, kvp => (object?)kvp.Value))
            : null;

        return HttpResults.Problem(
            statusCode: statusCode,
            title: titleMsg,
            detail: error.Description,
            extensions: new Dictionary<string, object?>()
            {
                ["code"] = error.Code,
                ["metadata"] = metadata,
            }
        );
    }
}
