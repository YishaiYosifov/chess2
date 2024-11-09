using ErrorOr;
using FluentValidation.Results;

namespace Chess2.Api.Extensions;

public static class ValidationExtensions
{
    public static List<Error> ToErrorList(this List<ValidationFailure> errors) =>
        errors.ConvertAll(error => Error.Validation(code: error.PropertyName, description: error.ErrorMessage));
}
