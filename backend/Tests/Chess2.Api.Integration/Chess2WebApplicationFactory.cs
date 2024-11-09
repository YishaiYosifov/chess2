using Chess2.Api.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Refit;
using Respawn;
using System.Data.Common;
using Testcontainers.PostgreSql;

namespace Chess2.Api.Integration;

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
    /// and is authenticated with the api
    /// </summary>
    public IChess2Api CreateTypedAuthedClient(TestClaimsProvider claimsProvider)
    {
        var httpClient = WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices((services) =>
            {
                services.AddAuthentication(defaultScheme: "TestScheme")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        "TestScheme", options => { });
                services.AddScoped(_ => claimsProvider);
            });
        }).CreateClient();

        return RestService.For<IChess2Api>(httpClient);
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
