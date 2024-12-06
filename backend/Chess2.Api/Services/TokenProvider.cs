using Chess2.Api.Models;
using Chess2.Api.Models.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace Chess2.Api.Services;

public interface ITokenProvider
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken(User user);
}

public class TokenProvider(IOptions<AppSettings> settings) : ITokenProvider
{
    private readonly JwtSettings _jwtSettings = settings.Value.Jwt;

    public string GenerateAccessToken(User user)
    {
        return GenerateToken(
            new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim("type", "access"),
            ]), DateTime.UtcNow.AddMinutes(_jwtSettings.AccessExpiresInMinute));
    }

    public string GenerateRefreshToken(User user)
    {
        return GenerateToken(
            new ClaimsIdentity([
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim("type", "refresh"),
            ]), DateTime.UtcNow.AddDays(_jwtSettings.RefreshExpiresInDays));
    }

    public string GenerateGuestToken(string guestId)
    {
        return GenerateToken(
            new ClaimsIdentity([
                new Claim(ClaimTypes.NameIdentifier, guestId),
                new Claim(ClaimTypes.Anonymous, "1"),
                new Claim("type", "access"),
            ]), DateTime.UtcNow.AddMinutes(_jwtSettings.AccessExpiresInMinute));
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
