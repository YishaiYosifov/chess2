using Akka.TestKit.Xunit;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Chess2.Api.TestInfrastructure.Utils;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Unit;

public class BaseActorTest : TestKit
{
    protected static CancellationToken CT => TestContext.Current.CancellationToken;
    protected readonly Fixture Fixture = new();

    public BaseActorTest()
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
