using System.Security.Claims;
using System.Text;
using Chess2.Api.Models;
using Chess2.Api.Models.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Chess2.Api.Services.Auth;

public interface ITokenProvider
{
    string GenerateAccessToken(AuthedUser user);
    string GenerateRefreshToken(AuthedUser user, string jti);
    string GenerateGuestToken(string guestId);
}

public class TokenProvider(IOptions<AppSettings> settings) : ITokenProvider
{
    private readonly JwtSettings _jwtSettings = settings.Value.Jwt;

    public string GenerateAccessToken(AuthedUser user)
    {
        return GenerateToken(
            new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim("type", "access"),
                ]
            ),
            DateTime.UtcNow.Add(_jwtSettings.AccessMaxAge)
        );
    }

    public string GenerateRefreshToken(AuthedUser user, string jti)
    {
        return GenerateToken(
            new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim("type", "refresh"),
                    new Claim(JwtRegisteredClaimNames.Jti, jti),
                ]
            ),
            DateTime.UtcNow.Add(_jwtSettings.RefreshMaxAge)
        );
    }

    public string GenerateGuestToken(string guestId)
    {
        return GenerateToken(
            new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, guestId),
                    new Claim(ClaimTypes.Anonymous, "1"),
                    new Claim("type", "access"),
                ]
            ),
            DateTime.MaxValue
        );
    }

    private string GenerateToken(ClaimsIdentity claims, DateTime expires)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = claims,
            Expires = expires,
            SigningCredentials = creds,
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
        };

        var handler = new JsonWebTokenHandler();
        return handler.CreateToken(tokenDescriptor);
    }
}
