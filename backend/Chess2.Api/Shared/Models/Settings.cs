namespace Chess2.Api.Shared.Models;

public class AppSettings
{
    public string[] CorsOrigins { get; set; } = [];

    public required GameSettings Game { get; set; }
    public required JwtSettings Jwt { get; set; }

    public required string OAuthRedirectUrl { get; set; }

    public required string CSRFHeader { get; set; }

    public required string RedisConnString { get; set; }
    public required string DatabaseConnString { get; set; }
    public TimeSpan UsernameEditCooldown { get; set; }
}

public class GameSettings
{
    public int AllowedMatchRatingDifference { get; set; }
    public TimeSpan MatchWaveEvery { get; set; }

    public int DefaultRating { get; set; }
    public int KFactor { get; set; }

    public int DrawCooldown { get; set; }

    public required ChatSettings Chat { get; set; }
}

public class ChatSettings
{
    public int BucketCapacity { get; set; }
    public TimeSpan BucketRefillRate { get; set; }

    public required int MaxMessageLength { get; set; }
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
