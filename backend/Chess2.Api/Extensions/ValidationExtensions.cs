using Chess2.Api.Errors;
using ErrorOr;
using FluentValidation.Results;

namespace Chess2.Api.Extensions;

public static class ValidationExtensions
{
    public static List<Error> ToErrorList(this List<ValidationFailure> errors) =>
        errors.ConvertAll(error => Error.Validation(
                description: error.ErrorMessage,
                metadata: new() { { MetadataFields.RelatedField,  error.PropertyName } }));
}
