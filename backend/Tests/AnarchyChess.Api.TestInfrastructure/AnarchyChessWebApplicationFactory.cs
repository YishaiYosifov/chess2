using System.Net;
using System.Text.Json;
using AnarchyChess.Api.Infrastructure;
using FluentStorage;
using FluentStorage.Blobs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Orleans.Providers;
using Refit;
using Respawn;
using Serilog;
using Serilog.Sinks.XUnit.Injectable;
using Serilog.Sinks.XUnit.Injectable.Abstract;
using Serilog.Sinks.XUnit.Injectable.Extensions;
using Testcontainers.Azurite;
using Testcontainers.PostgreSql;

namespace AnarchyChess.Api.TestInfrastructure;

public class AnarchyChessWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("anarchychess")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly AzuriteContainer _azuriteContainer = new AzuriteBuilder()
        .WithImage("mcr.microsoft.com/azure-storage/azurite:latest")
        .Build();

    private NpgsqlConnection _dbConnection = null!;
    private Respawner _respawner = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder
            .ConfigureServices(services =>
            {
                // remove the existing database context, use the one in a test container instead
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.AddDbContextPool<ApplicationDbContext>(options =>
                    options
                        .UseNpgsql(_dbContainer.GetConnectionString())
                        .UseSnakeCaseNamingConvention()
                );
                services.AddAdoNetGrainStorage(
                    ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME,
                    options =>
                    {
                        options.Invariant = "Npgsql";
                        options.ConnectionString = _dbContainer.GetConnectionString();
                    }
                );
                services.UseAdoNetReminderService(services =>
                {
                    services.Configure(options =>
                    {
                        options.Invariant = "Npgsql";
                        options.ConnectionString = _dbContainer.GetConnectionString();
                    });
                });

                services.RemoveAll<IBlobStorage>();
                services.AddSingleton<IBlobStorage>(
                    StorageFactory.Blobs.AzureBlobStorageWithSharedKey(
                        accountName: AzuriteBuilder.AccountName,
                        key: AzuriteBuilder.AccountKey,
                        serviceUri: new(_azuriteContainer.GetBlobEndpoint())
                    )
                );

                InjectableTestOutputSink injectableTestOutputSink = new();
                services.AddSingleton<IInjectableTestOutputSink>(injectableTestOutputSink);
                services.AddSerilog(
                    (_, loggerConfiguration) =>
                    {
                        loggerConfiguration.WriteTo.InjectableTestOutput(injectableTestOutputSink);
                    }
                );
            })
            .ConfigureAppConfiguration(
                (context, configBuilder) =>
                {
                    Dictionary<string, string?> secrets = new()
                    {
                        { "AppSettings:Secrets:GoogleOAuth:ClientId", "test-google-client-id" },
                        {
                            "AppSettings:Secrets:GoogleOAuth:ClientSecret",
                            "test-google-client-secret"
                        },
                        { "AppSettings:Secrets:DiscordOAuth:ClientId", "test-discord-client-id" },
                        {
                            "AppSettings:Secrets:DiscordOAuth:ClientSecret",
                            "test-discord-client-secret"
                        },
                    };
                    configBuilder.AddInMemoryCollection(secrets);
                }
            );
    }

    public ApiClient CreateApiClient()
    {
        CookieContainer cookieContainer = new();
        CookieContainerHandler cookieHandler = new(cookieContainer);
        RefitSettings refitSettings = new()
        {
            ContentSerializer = new SystemTextJsonContentSerializer(
                new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
            ),
        };

        var httpClient = CreateDefaultClient(new Uri("https://localhost"), cookieHandler);
        var apiClient = RestService.For<IAnarchyChessApi>(httpClient, refitSettings);
        return new(apiClient, httpClient, cookieContainer);
    }

    public Task ResetDatabaseAsync() => _respawner.ResetAsync(_dbConnection);

    public async ValueTask InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _azuriteContainer.StartAsync();

        _dbConnection = await InitializeDbContainerAsync();
        _respawner = await InitializeRespawnerAsync(_dbConnection);
    }

    public new async ValueTask DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _azuriteContainer.StopAsync();
        GC.SuppressFinalize(this);
    }

    private async Task<NpgsqlConnection> InitializeDbContainerAsync()
    {
        var connection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await connection.OpenAsync();

        await SetupAdoNetAsync(connection);
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        return connection;
    }

    private static async Task SetupAdoNetAsync(NpgsqlConnection connection)
    {
        var sqlDir = Path.Combine(AppContext.BaseDirectory, "Scripts/Orleans");
        var scripts = Directory.GetFiles(sqlDir, "*.sql", SearchOption.AllDirectories).Order();
        foreach (var file in scripts)
        {
            var sql = await File.ReadAllTextAsync(file);
            await using var cmd = new NpgsqlCommand(sql, connection);
            var result = await cmd.ExecuteNonQueryAsync();
        }
    }

    private static Task<Respawner> InitializeRespawnerAsync(NpgsqlConnection connection) =>
        Respawner.CreateAsync(
            connection,
            new()
            {
                DbAdapter = DbAdapter.Postgres,
                TablesToIgnore = ["orleansquery", "orleansreminderstable", "orleansstorage"],
            }
        );
}
