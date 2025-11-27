namespace AnarchyChess.Api.Shared.Models;

public class AppSettings
{
    public string[] CorsOrigins { get; set; } = [];

    public required AuthSettings Auth { get; set; }
    public required LobbySettings Lobby { get; set; }
    public required GameSettings Game { get; set; }
    public required ChallengeSettings Challenge { get; set; }

    public required string CSRFHeader { get; set; }

    public required string RedisConnString { get; set; }
    public required string DatabaseConnString { get; set; }
    public required string BlobStorageConnString { get; set; }

    public TimeSpan UsernameEditCooldown { get; set; }
}

public class AuthSettings
{
    public required string OAuthRedirectUrl { get; set; }
    public required string LoginPageUrl { get; set; }

    public required JwtSettings Jwt { get; set; }

    public TimeSpan AccessMaxAge { get; set; }
    public TimeSpan RefreshMaxAge { get; set; }

    public required string AccessTokenCookieName { get; set; }
    public required string RefreshTokenCookieName { get; set; }
    public required string IsLoggedInCookieName { get; set; }
    public required string AuthFailureCookieName { get; set; }
}

public class ChallengeSettings
{
    public TimeSpan ChallengeLifetime { get; set; }
}

public class LobbySettings
{
    public int OpenSeekShardCount { get; set; }

    public int MaxActiveGames { get; set; }
    public int AllowedMatchRatingDifference { get; set; }
    public TimeSpan MatchWaveEvery { get; set; }
    public TimeSpan SeekLifetime { get; set; }
}

public class GameSettings
{
    public int DefaultRating { get; set; }
    public int KFactor { get; set; }

    public int DrawCooldown { get; set; }

    public required ChatSettings Chat { get; set; }

    public TimeSpan RematchLifetime { get; set; }
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
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
}
