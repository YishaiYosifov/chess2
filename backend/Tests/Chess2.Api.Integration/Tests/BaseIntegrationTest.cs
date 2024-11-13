using Chess2.Api.Integration.Collections;
using Chess2.Api.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Integration.Tests;

[Collection(nameof(SharedWebApplication))]
public class BaseIntegrationTest : IAsyncLifetime
{
    private readonly IServiceScope _scope;

    protected readonly IChess2Api ApiClient;
    protected readonly Chess2DbContext DbContext;
    protected readonly Chess2WebApplicationFactory Factory;

    protected BaseIntegrationTest(Chess2WebApplicationFactory factory)
    {
        Factory = factory;
        _scope = Factory.Services.CreateScope();

        ApiClient = Factory.CreateTypedClient();
        DbContext = _scope.ServiceProvider.GetRequiredService<Chess2DbContext>();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await Factory.ResetDatabaseAsync();
        _scope?.Dispose();
        DbContext?.Dispose();
    }
}
