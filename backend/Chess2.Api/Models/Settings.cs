namespace Chess2.Api.Models;

public class AppConfig
{
    public required string SecretKey { get; set; }
    public required DatabaseConfig Database { get; set; }
}

public class DatabaseConfig
{
    public required string Host { get; set; }
    public required string Port { get; set; }

    public required string Username { get; set; }
    public required string Password { get; set; }

    public required string Database { get; set; }

    public string GetConnectionString()
    {
        var encodedPassword = Uri.EscapeDataString(Password);
        return $"postgresql://{Username}:{encodedPassword}@{Host}:{Port}/{Database}";
    }
}
