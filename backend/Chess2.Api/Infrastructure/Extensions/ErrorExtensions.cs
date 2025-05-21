using Chess2.Api.Infrastructure.Errors;
using Chess2.Api.Infrastructure.SignalR;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;

namespace Chess2.Api.Infrastructure.Extensions;

public static class ErrorExtensions
{
    public static IEnumerable<SignalRError> ToSignalR(this Error error) =>
        [new SignalRError(error)];

    public static IEnumerable<SignalRError> ToSignalR(this IEnumerable<Error> errors) =>
        errors.Select(error => new SignalRError(error));

    public static ActionResult ToActionResult(this Error error) =>
        new List<Error>() { error }.ToActionResult();

    public static ActionResult ToActionResult(this IEnumerable<Error> errors)
    {
        var errorType = errors.First().Type;
        return errorType switch
        {
            ErrorType.Validation => new ObjectResult(CreateValidationProblemDetails(errors)),
            _ => new ObjectResult(CreateApiProblemDetails(errors)),
        };
    }

    private static ValidationProblemDetails CreateValidationProblemDetails(
        IEnumerable<Error> errors
    )
    {
        var formattedErrors = errors
            .GroupBy(x => x.Code)
            .ToDictionary(
                group => group.Key,
                group => group.Select(err => err.Description).ToArray()
            );

        var problemDetails = new ValidationProblemDetails()
        {
            Status = GetStatusCode(ErrorType.Validation),
            Title = GetTitle(ErrorType.Validation),
            Type = GetType(ErrorType.Validation),
            Errors = formattedErrors,
        };
        return problemDetails;
    }

    private static ApiProblemDetails CreateApiProblemDetails(IEnumerable<Error> errors)
    {
        var errorType = errors.First().Type;
        var formattedErrors = errors.Select(error => new ApiProblemError()
        {
            ErrorCode = error.Code,
            Description = error.Description,
        });

        var problemDetails = new ApiProblemDetails()
        {
            Status = GetStatusCode(errorType),
            Title = GetTitle(errorType),
            Type = GetType(errorType),
            Errors = formattedErrors,
        };
        return problemDetails;
    }

    private static int GetStatusCode(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.Validation or ErrorType.Failure => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError,
        };

    private static string GetTitle(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.Validation or ErrorType.Failure => "Bad Request",
            ErrorType.Unauthorized => "Unauthorized",
            ErrorType.NotFound => "Not Found",
            ErrorType.Conflict => "Conflict",
            ErrorType.Forbidden => "Forbidden",
            _ => "Internal Server Error",
        };

    private static string GetType(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.Validation or ErrorType.Failure =>
                "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
            ErrorType.Unauthorized => "https://datatracker.ietf.org/doc/html/rfc7235#section-3.1",
            ErrorType.NotFound => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
            ErrorType.Conflict => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8",
            ErrorType.Forbidden => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3",
            _ => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
        };
}
