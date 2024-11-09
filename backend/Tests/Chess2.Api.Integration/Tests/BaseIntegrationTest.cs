using Chess2.Api.Integration.Collections;
using Chess2.Api.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Integration.Tests;

[Collection(nameof(SharedWebApplication))]
public class BaseIntegrationTest : IAsyncLifetime
{
    private readonly Chess2WebApplicationFactory _factory;
    private readonly IServiceScope _scope;

    protected readonly IChess2Api ApiClient;
    protected readonly Chess2DbContext DbContext;

    protected BaseIntegrationTest(Chess2WebApplicationFactory factory)
    {
        _factory = factory;
        _scope = _factory.Services.CreateScope();

        ApiClient = _factory.CreateTypedClient();
        DbContext = _scope.ServiceProvider.GetRequiredService<Chess2DbContext>();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _scope?.Dispose();
        DbContext?.Dispose();
    }
}
