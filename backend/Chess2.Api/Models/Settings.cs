namespace Chess2.Api.Models;

public class AppSettings
{
    public string[] CorsOrigins { get; set; } = [];

    public required GameSettings Game { get; set; }
    public required JwtSettings Jwt { get; set; }

    public required string OAuthRedirectUrl { get; set; }

    public required string RedisConnString { get; set; }
    public required string DatabaseConnString { get; set; }
    public TimeSpan UsernameEditCooldown { get; set; }
}

public class GameSettings
{
    public int MaxMatchRatingDifference { get; set; }
}

public class JwtSettings
{
    public required string SecretKey { get; set; }
    public TimeSpan AccessMaxAge { get; set; }
    public TimeSpan RefreshMaxAge { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }

    public required string AccessTokenCookieName { get; set; }
    public required string RefreshTokenCookieName { get; set; }
    public required string IsAuthedTokenCookieName { get; set; }
}
