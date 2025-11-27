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
    private readonly JwtSettings _jwtSettings = settings.Value.Jwt;
    private readonly TimeProvider _timeProvider = timeProvider;

    public ErrorOr<string> GenerateAccessToken(AuthedUser user)
    {
        if (user.IsBanned)
            return AuthErrors.UserBanned;

        return GenerateToken(
            new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, user.Id), new Claim("type", "access")]
            ),
            _timeProvider.GetUtcNow().Add(_jwtSettings.AccessMaxAge).UtcDateTime
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
            _timeProvider.GetUtcNow().Add(_jwtSettings.RefreshMaxAge).UtcDateTime
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
        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha256);
        SecurityTokenDescriptor tokenDescriptor = new()
        {
            Subject = claims,
            Expires = expires,
            SigningCredentials = creds,
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
        };

        JsonWebTokenHandler handler = new();
        return handler.CreateToken(tokenDescriptor);
    }
}
