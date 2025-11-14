using AnarchyChess.Api.Infrastructure.Errors;
using ErrorOr;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;

namespace AnarchyChess.Api.Infrastructure.Extensions;

public static class ValidationExtensions
{
    public static List<Error> ToErrorList(this IEnumerable<ValidationFailure> errors) =>
        [
            .. errors.Select(error =>
                Error.Validation(
                    description: error.ErrorMessage,
                    metadata: new() { [ErroConstants.FieldValidationMeta] = error.PropertyName }
                )
            ),
        ];

    public static List<Error> ToErrorList(this IEnumerable<IdentityError> errors) =>
        [.. errors.Select(error => Error.Failure(error.Code, error.Description))];
}
