using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.Enum;

namespace osiedle_booking.WebApi.Extensions;

public static class ResultExtensions
{
    public static ActionResult ToActionResult(this Result result)
    {
        return result.IsSuccess
            ? new NoContentResult()
            : ToProblemDetails(result.Error);
    }

    public static ActionResult<T> ToActionResult<T>(this Result<T> result)
    {
        return result.IsSuccess
            ? result.Value
            : ToProblemDetails(result.Error);
    }

    private static ObjectResult ToProblemDetails(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status400BadRequest
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = ReasonPhrases.GetReasonPhrase(statusCode),
            Detail = error.Description,
            Extensions = { ["code"] = error.Code }
        };

        return new ObjectResult(problemDetails) { StatusCode = statusCode };
    }
}
