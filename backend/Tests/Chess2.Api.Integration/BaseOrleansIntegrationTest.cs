using Chess2.Api.TestInfrastructure;
using Orleans.TestKit;

namespace Chess2.Api.Integration;

[Collection(nameof(SharedIntegrationContext))]
public class BaseOrleansIntegrationTest(Chess2WebApplicationFactory factory)
    : TestKitBase,
        IAsyncLifetime
{
    protected BaseIntegrationTest ApiTestBase { get; } = new BaseIntegrationTest(factory);

    public async ValueTask InitializeAsync()
    {
        await ApiTestBase.InitializeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await ApiTestBase.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
