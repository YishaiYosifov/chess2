using Chess2.Api.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Integration.Tests;

public class BaseIntegrationTest : IClassFixture<Chess2WebApplicationFactory>, IAsyncLifetime
{
    protected readonly Chess2WebApplicationFactory _factory;
    protected readonly IChess2Api _apiClient;

    protected readonly IServiceScope _serviceScope;
    protected readonly Chess2DbContext _dbContext;

    public BaseIntegrationTest(Chess2WebApplicationFactory factory)
    {
        _factory = factory;
        _apiClient = factory.CreateTypedClient();

        _serviceScope = _factory.Server.Services.CreateScope();
        _dbContext = _serviceScope.ServiceProvider.GetRequiredService<Chess2DbContext>();
    }

    public async Task InitializeAsync()
    {
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.Database.EnsureDeletedAsync();
        _serviceScope.Dispose();
    }
}
