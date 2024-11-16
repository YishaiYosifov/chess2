namespace Chess2.Api.Models;

public class AppSettings
{
    public required string[] CorsOrigins { get; set; }
    public required DatabaseSettings Database { get; set; }
    public required JwtSettings Jwt { get; set; }
}

public class JwtSettings
{
    public required string SecretKey { get; set; }
    public int AccessExpiresInMinute { get; set; }
    public int RefreshExpiresInDays { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }

    public required string AccessTokenCookieName { get; set; }
    public required string RefreshTokenCookieName { get; set; }
}

public class DatabaseSettings
{
    public required string Host { get; set; }
    public required string Port { get; set; }

    public required string Username { get; set; }
    public required string Password { get; set; }

    public required string Database { get; set; }

    public string GetConnectionString()
    {
        return $"Host={Host};Port={Port};Username={Username};Password={Password};Database={Database}";
    }
}
