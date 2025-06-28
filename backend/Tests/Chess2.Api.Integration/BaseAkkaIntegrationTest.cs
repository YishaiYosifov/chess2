using Akka.TestKit.Xunit;
using Chess2.Api.TestInfrastructure;

namespace Chess2.Api.Integration;

[Collection(nameof(SharedIntegrationContext))]
public class BaseAkkaIntegrationTest(Chess2WebApplicationFactory factory) : TestKit, IAsyncLifetime
{
    protected BaseIntegrationTest ApiTestBase { get; } = new BaseIntegrationTest(factory);

    public async ValueTask InitializeAsync()
    {
        await ApiTestBase.InitializeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        await ApiTestBase.DisposeAsync();
        Dispose();
    }
}
