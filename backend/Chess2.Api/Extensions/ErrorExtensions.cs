using ErrorOr;
using System.Diagnostics;

namespace Chess2.Api.Extensions;

public static class ErrorExtensions
{
    public static IResult ToProblemDetails(this IEnumerable<Error> errors)
    {
        var errorType = errors.First().Type;
        var formattedErrors = errors.Select(error => new Dictionary<string, object?>
        {
            { "code", error.Code },
            { "detail", error.Description },
        });

        return Results.Problem(
            statusCode: GetStatusCode(errorType),
            title: GetTitle(errorType),
            type: GetType(errorType),
            extensions: new Dictionary<string, object?>
            {
                { "errors",  formattedErrors },
            }
        );
    }

    private static int GetStatusCode(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError,
        };

    private static string GetTitle(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.Validation => "Bad Request",
            ErrorType.Unauthorized => "Unauthorized",
            ErrorType.NotFound => "Not Found",
            ErrorType.Conflict => "Conflict",
            _ => "Internal Server Error",
        };

    private static string GetType(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.Validation => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
            ErrorType.Unauthorized => "https://datatracker.ietf.org/doc/html/rfc7235#section-3.1",
            ErrorType.NotFound => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
            ErrorType.Conflict => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8",
            _ => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
        };
}
