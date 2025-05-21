using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Chess2.Api.Shared.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Unit;

public class BaseUnitTest
{
    protected readonly Fixture Fixture = new();

    public BaseUnitTest()
    {
        Fixture.Customize(new AutoNSubstituteCustomization());
        AddAppSettings(Fixture);
    }

    private static void AddAppSettings(Fixture fixture)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
        var appSettings =
            configuration.GetSection("AppSettings").Get<AppSettings>()
            ?? throw new NullReferenceException("Could not get appsettings for the tests");
        fixture.Register(() => Options.Create(appSettings));
    }
}
