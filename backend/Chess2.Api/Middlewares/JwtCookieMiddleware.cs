
using Chess2.Api.Models;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Middlewares;

public class JwtCookieMiddleware(IOptions<AppSettings> appSettings) : IMiddleware
{
    private readonly JwtSettings _jwtSettings = appSettings.Value.Jwt;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Cookies.TryGetValue(_jwtSettings.AccessTokenCookieName, out var accessToken)
            && !string.IsNullOrEmpty(accessToken))
        {
            var bearerToken = $"Bearer {accessToken}";
            context.Request.Headers.Append("Authorization", bearerToken);
        }
        await next(context);
    }
}
