using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AnarchyChess.Api.TestInfrastructure.Utils;
using Microsoft.Extensions.Options;

namespace AnarchyChess.Api.Unit;

public class BaseUnitTest
{
    protected readonly Fixture Fixture = new();

    protected static CancellationToken CT => TestContext.Current.CancellationToken;

    public BaseUnitTest()
    {
        Fixture.Customize(new AutoNSubstituteCustomization());
        AddAppSettings(Fixture);
    }

    private static void AddAppSettings(Fixture fixture)
    {
        var appSettings = AppSettingsLoader.LoadAppSettings();
        fixture.Register(() => Options.Create(appSettings));
    }
}
