using System.Text;

namespace Chess2Backend.Models;

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
