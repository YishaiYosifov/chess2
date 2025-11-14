using AnarchyChess.Api.Shared.Models;
using Microsoft.Extensions.Configuration;

namespace AnarchyChess.Api.TestInfrastructure.Utils;

public static class AppSettingsLoader
{
    private static AppSettings? _appSettings;

    public static AppSettings LoadAppSettings()
    {
        if (_appSettings is not null)
            return _appSettings;

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
        var appSettings =
            configuration.GetSection("AppSettings").Get<AppSettings>()
            ?? throw new NullReferenceException("Could not get appsettings for the tests");
        _appSettings = appSettings;

        return appSettings;
    }
}
