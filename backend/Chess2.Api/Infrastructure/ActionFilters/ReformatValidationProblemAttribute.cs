using Chess2.Api.Extensions;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Chess2.Api.Infrastructure.ActionFilters;

public class ReformatValidationProblemAttribute : ActionFilterAttribute
{
    public override void OnResultExecuting(ResultExecutingContext context)
    {
        // format json serialization validation errors
        if (
            context.Result is BadRequestObjectResult badRequestObjectResult
            && badRequestObjectResult.Value is ValidationProblemDetails validationProblemDetails
        )
            context.Result = FormatValidationError(validationProblemDetails.Errors);

        base.OnResultExecuting(context);
    }

    private static ActionResult FormatValidationError(IDictionary<string, string[]> errors)
    {
        var formattedErrors = errors.Select(error =>
            Error.Validation(code: error.Key, description: error.Value[0])
        );

        return formattedErrors.ToActionResult();
    }
}
