using Chess2.Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Refit;
using Respawn;
using System.Data.Common;
using System.Net;
using Testcontainers.PostgreSql;

namespace Chess2.Api.TestInfrastructure;

public class Chess2WebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("chess2")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private DbConnection _dbConnection = null!;
    private Respawner _respawner = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // remove the existing database context, use the one in a test container instead
            services.RemoveAll(typeof(DbContextOptions<Chess2DbContext>));
            services.AddDbContextPool<Chess2DbContext>(options =>
                options
                    .UseNpgsql(_dbContainer.GetConnectionString())
                    .UseSnakeCaseNamingConvention());
        });
    }

    /// <summary>
    /// Create a client that follows the chess2 api schema
    /// and has specific authentication tokens
    /// </summary>
    public IChess2Api CreateTypedClientWithTokens(string? accessToken = null, string? refreshToken = null)
    {
        var cookieContainer = new CookieContainer();
        if (!string.IsNullOrEmpty(accessToken))
            cookieContainer.Add(Server.BaseAddress, new Cookie("accessToken", accessToken));
        if (!string.IsNullOrEmpty(refreshToken))
            cookieContainer.Add(Server.BaseAddress, new Cookie("refreshToken", refreshToken));

        var handler = new CookieContainerHandler(cookieContainer);
        return RestService.For<IChess2Api>(CreateDefaultClient(handler));
    }

    /// <summary>
    /// Create an http client that follows the chess2 api schema
    /// </summary>
    public IChess2Api CreateTypedClient() =>
        RestService.For<IChess2Api>(CreateClient());

    public Task ResetDatabaseAsync() => _respawner.ResetAsync(_dbConnection);

    public async Task InitializeAsync()
    {
        await InitializeDbContainer();
        await InitializeRespawner();
    }

    public new Task DisposeAsync() => _dbContainer.StopAsync();

    private async Task InitializeDbContainer()
    {
        await _dbContainer.StartAsync();

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Chess2DbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }

    private async Task InitializeRespawner()
    {
        _dbConnection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await _dbConnection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_dbConnection, new()
        {
            DbAdapter = DbAdapter.Postgres,
        });
    }
}
