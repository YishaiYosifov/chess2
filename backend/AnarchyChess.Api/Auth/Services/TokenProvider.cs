using System.Security.Claims;
using System.Text;
using AnarchyChess.Api.Auth.Errors;
using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.Shared.Models;
using ErrorOr;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AnarchyChess.Api.Auth.Services;

public interface ITokenProvider
{
    ErrorOr<string> GenerateAccessToken(AuthedUser user);
    string GenerateRefreshToken(AuthedUser user, string jti);
    string GenerateGuestToken(string guestId);
}

public class TokenProvider(IOptions<AppSettings> settings, TimeProvider timeProvider)
    : ITokenProvider
{
    private readonly AuthSettings _settings = settings.Value.Auth;
    private readonly TimeProvider _timeProvider = timeProvider;

    private readonly SymmetricSecurityKey _secretKey = new(
        Encoding.UTF8.GetBytes(settings.Value.Secrets.JwtSecret)
    );

    public ErrorOr<string> GenerateAccessToken(AuthedUser user)
    {
        if (user.IsBanned)
            return AuthErrors.UserBanned;

        return GenerateToken(
            new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, user.Id), new Claim("type", "access")]
            ),
            _timeProvider.GetUtcNow().Add(_settings.AccessMaxAge).UtcDateTime
        );
    }

    public string GenerateRefreshToken(AuthedUser user, string jti)
    {
        return GenerateToken(
            new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim("type", "refresh"),
                    new Claim(JwtRegisteredClaimNames.Jti, jti),
                ]
            ),
            _timeProvider.GetUtcNow().Add(_settings.RefreshMaxAge).UtcDateTime
        );
    }

    public string GenerateGuestToken(string guestId)
    {
        return GenerateToken(
            new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, guestId),
                    new Claim(ClaimTypes.Anonymous, "true"),
                    new Claim("type", "access"),
                ]
            ),
            DateTimeOffset.MaxValue.UtcDateTime
        );
    }

    private string GenerateToken(ClaimsIdentity claims, DateTime expires)
    {
        SigningCredentials creds = new(_secretKey, SecurityAlgorithms.HmacSha256);
        SecurityTokenDescriptor tokenDescriptor = new()
        {
            Subject = claims,
            Expires = expires,
            SigningCredentials = creds,
            Issuer = _settings.Jwt.Issuer,
            Audience = _settings.Jwt.Audience,
        };

        JsonWebTokenHandler handler = new();
        return handler.CreateToken(tokenDescriptor);
    }
}
