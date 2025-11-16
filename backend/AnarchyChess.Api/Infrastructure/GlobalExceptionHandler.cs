using AnarchyChess.Api.ErrorHandling.Extensions;
using ErrorOr;
using Microsoft.AspNetCore.Diagnostics;

namespace AnarchyChess.Api.Infrastructure;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        var error = Error.Failure(description: "Internal Server Error");
        await error.ToActionResult().ExecuteResultAsync(new() { HttpContext = httpContext });
        return true;
    }
}
