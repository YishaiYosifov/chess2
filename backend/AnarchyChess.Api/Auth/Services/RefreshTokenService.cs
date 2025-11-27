using AnarchyChess.Api.Auth.Entities;
using AnarchyChess.Api.Auth.Errors;
using AnarchyChess.Api.Auth.Repositories;
using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.Shared.Models;
using ErrorOr;
using Microsoft.Extensions.Options;

namespace AnarchyChess.Api.Auth.Services;

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
    IRefreshTokenRepository refreshTokenRepository,
    TimeProvider timeProvider
) : IRefreshTokenService
{
    private readonly ILogger<RefreshTokenService> _logger = logger;
    private readonly AuthSettings _settings = appSettings.Value.Auth;
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<RefreshToken> CreateRefreshTokenAsync(
        AuthedUser user,
        CancellationToken token = default
    )
    {
        var jti = GenerateJti();
        var expiresAt = _timeProvider.GetUtcNow().Add(_settings.RefreshMaxAge);
        var refreshToken = new RefreshToken()
        {
            UserId = user.Id,
            User = user,
            Jti = jti,
            ExpiresAt = expiresAt,
        };
        await _refreshTokenRepository.AddRefreshTokenAsync(refreshToken, token);

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
