using Chess2.Api.Errors;
using ErrorOr;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;

namespace Chess2.Api.Extensions;

public static class ValidationExtensions
{
    public static List<Error> ToErrorList(this IEnumerable<ValidationFailure> errors) =>
        [
            .. errors.Select(error =>
                Error.Validation(code: error.PropertyName, description: error.ErrorMessage)
            ),
        ];

    public static List<Error> ToErrorList(this IEnumerable<IdentityError> errors) =>
        [.. errors.Select(error => Error.Failure(error.Code, error.Description))];
}
