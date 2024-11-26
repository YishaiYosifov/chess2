using Chess2.Api.Extensions;
using ErrorOr;
using Microsoft.AspNetCore.Diagnostics;

namespace Chess2.Api;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var error = Error.Failure(description: "Internal Server Error");
        await error.ToProblemDetails()
            .ExecuteResultAsync(new()
            {
                HttpContext = httpContext
            });
        return true;
    }
}
