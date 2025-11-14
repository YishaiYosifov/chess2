using AnarchyChess.Api.Infrastructure.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace AnarchyChess.Api.Infrastructure;

/// <summary>
/// Automatically apply [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
/// to all endpoints that require authorization
/// </summary>
public class UnauthorizedResponseConvention : IApplicationModelConvention
{
    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            var controllerHasAuthorize = controller.Attributes.OfType<AuthorizeAttribute>().Any();

            foreach (var action in controller.Actions)
            {
                var actionHasAuthorize = action.Attributes.OfType<AuthorizeAttribute>().Any();
                if (!controllerHasAuthorize && !actionHasAuthorize)
                    continue;

                var alreadyHas401 = action
                    .Filters.OfType<ProducesResponseTypeAttribute>()
                    .Any(attr => attr.StatusCode == StatusCodes.Status401Unauthorized);
                if (alreadyHas401)
                    continue;

                action.Filters.Add(
                    new ProducesResponseTypeAttribute<ApiProblemDetails>(
                        StatusCodes.Status401Unauthorized
                    )
                );
            }
        }
    }
}
