using Chess2.Api.Shared.Models;
using Microsoft.Extensions.Configuration;

namespace Chess2.Api.TestInfrastructure.Utils;

public static class AppSettingsLoader
{
    public static AppSettings LoadAppSettings()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
        var appSettings =
            configuration.GetSection("AppSettings").Get<AppSettings>()
            ?? throw new NullReferenceException("Could not get appsettings for the tests");
        return appSettings;
    }
}
