
using System.Diagnostics;

namespace Chess2.Api.Middlewares;

public class ExceptionHandlerMiddleware(ILogger<ExceptionHandlerMiddleware> logger) : IMiddleware
{
    private readonly ILogger<ExceptionHandlerMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var traceId = Activity.Current?.Id;
            _logger.LogError(
                ex,
                "Could not process a request for path {Path}. TraceId: {TraceId}",
                context.Request.Path,
                traceId);

            await Results.Problem(
                title: "Internal Server Error",
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?>()
                {
                    {"traceId", traceId }
                }).ExecuteAsync(context);
        }
    }
}
