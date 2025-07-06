namespace Chess2.Api.Shared.Models;

public class AppSettings
{
    public string[] CorsOrigins { get; set; } = [];

    public required AkkaSettings Akka { get; set; }

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
    public int StartingMatchRatingDifference { get; set; }
    public int MatchRatingDifferenceGrowthPerWave { get; set; }
    public TimeSpan MatchWaveEvery { get; set; }

    public int DefaultRating { get; set; }
    public int KFactor { get; set; }
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

public class AkkaSettings
{
    public required string ActorSystemName { get; set; }
    public required string Hostname { get; set; }
    public int Port { get; set; }
    public required string[] SeedNodes { get; set; }

    public int MatchmakingShardCount { get; set; }
    public int PlayerSessionShardCount { get; set; }
    public int GameShardCount { get; set; }
}
