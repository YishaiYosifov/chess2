using AnarchyChess.Api.TestInfrastructure;
using Orleans.TestKit;

namespace AnarchyChess.Api.Integration;

[Collection(nameof(SharedIntegrationContext))]
public class BaseOrleansIntegrationTest(AnarchyChessWebApplicationFactory factory)
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
