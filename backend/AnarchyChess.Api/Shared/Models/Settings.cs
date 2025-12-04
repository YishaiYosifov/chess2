namespace AnarchyChess.Api.Shared.Models;

public class AppSettings
{
    public string[] CorsOrigins { get; set; } = [];

    public required AuthSettings Auth { get; set; }
    public required LobbySettings Lobby { get; set; }
    public required GameSettings Game { get; set; }
    public required ChallengeSettings Challenge { get; set; }

    // Set via dotnet user-secrets
    public required SecretSettings Secrets { get; set; }

    public TimeSpan UsernameEditCooldown { get; set; }
}

public class SecretSettings
{
    public required string JwtSecret { get; set; }

    public required string DatabaseConnString { get; set; }
    public required string BlobStorageConnString { get; set; }

    public required OAuthClientSettings GoogleOAuth { get; set; }
    public required OAuthClientSettings DiscordOAuth { get; set; }

    public required OpenIddictSettings OpenIddict { get; set; }
}

public class OAuthClientSettings
{
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
}

public class OpenIddictSettings
{
    public required string SigningCertB64 { get; set; }
    public required string SigningCertPassword { get; set; }

    public required string EncryptionCertB64 { get; set; }
    public required string EncryptionCertPassword { get; set; }
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
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
}
