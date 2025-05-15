using Chess2.Api.Errors;
using Chess2.Api.Models;
using Chess2.Api.Models.Entities;
using Chess2.Api.Repositories;
using ErrorOr;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace Chess2.Api.Services.Auth;

public interface IRefreshTokenService
{
    Task<RefreshToken> CreateRefreshTokenAsync(AuthedUser user, CancellationToken token = default);
    Task<ErrorOr<Success>> ConsumeRefreshTokenAsync(
        AuthedUser user,
        string jti,
        CancellationToken token = default
    );
}

public class RefreshTokenService(
    ILogger<RefreshTokenService> logger,
    IOptions<AppSettings> appSettings,
    IRefreshTokenRepository refreshTokenRepository
) : IRefreshTokenService
{
    private readonly ILogger<RefreshTokenService> _logger = logger;
    private readonly JwtSettings _jwtSettings = appSettings.Value.Jwt;
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;

    public async Task<RefreshToken> CreateRefreshTokenAsync(
        AuthedUser user,
        CancellationToken token = default
    )
    {
        var jti = GenerateJti();
        var expiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshExpiresInDays);
        var refreshToken = new RefreshToken()
        {
            User = user,
            Jti = jti,
            ExpiresAt = expiresAt,
        };
        await _refreshTokenRepository.AddRefreshToken(refreshToken, token);

        return refreshToken;
    }

    public async Task<ErrorOr<Success>> ConsumeRefreshTokenAsync(
        AuthedUser user,
        string jti,
        CancellationToken token = default
    )
    {
        var refreshToken = await _refreshTokenRepository.GetTokenByJtiAsync(jti, token);
        if (refreshToken is null)
        {
            _logger.LogInformation(
                "User {UserId} attempted to refresh a token with an invalid jti {Jti}",
                user.Id,
                jti
            );
            return AuthErrors.TokenInvalid;
        }

        if (refreshToken.IsRevoked)
        {
            _logger.LogInformation(
                "User {UserId} attempted to refresh a token with a revoked jti {Jti}",
                user.Id,
                jti
            );
            return AuthErrors.TokenInvalid;
        }

        refreshToken.IsRevoked = true;
        return Result.Success;
    }

    private static string GenerateJti() => Guid.NewGuid().ToString();
}
