using Chess2.Api.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.TestInfrastructure;

public class ApiTestBase : IAsyncLifetime
{
    protected readonly IServiceScope Scope;
    protected readonly IChess2Api ApiClient;
    protected readonly Chess2DbContext DbContext;
    protected readonly Chess2WebApplicationFactory Factory;

    protected ApiTestBase(Chess2WebApplicationFactory factory)
    {
        Factory = factory;
        Scope = Factory.Services.CreateScope();

        ApiClient = Factory.CreateTypedClient();
        DbContext = Scope.ServiceProvider.GetRequiredService<Chess2DbContext>();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await Factory.ResetDatabaseAsync();
        Scope?.Dispose();
        DbContext?.Dispose();
    }
}
