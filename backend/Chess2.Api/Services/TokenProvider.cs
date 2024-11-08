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
    string GenerateToken(User user);
}

public class TokenProvider(IOptions<AppSettings> config) : ITokenProvider
{
    private readonly JwtSettings _jwtSettings = config.Value.Jwt;

    public string GenerateToken(User user)
    {
        var claims = new ClaimsIdentity(
        [
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
        ]);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("secret-secret-secret-secret-secret-secret"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = claims,
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresInMinutes),
            SigningCredentials = creds,
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
        };

        var handler = new JsonWebTokenHandler();
        return handler.CreateToken(tokenDescriptor);
    }
}
