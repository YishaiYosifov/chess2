using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.Hosting;
using Chess2.Api.Auth.Services;
using Chess2.Api.Game.Actors;
using Chess2.Api.Infrastructure;
using Chess2.Api.Matchmaking.Actors;
using Chess2.Api.Player.Actors;
using Chess2.Api.Shared.Models;
using Chess2.Api.TestInfrastructure.Utils;
using Chess2.Api.Users.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Chess2.Api.TestInfrastructure;

public class ApiTestBase : IAsyncLifetime
{
    protected Chess2WebApplicationFactory Factory { get; }
    protected IServiceScope Scope { get; }
    protected ApiClient ApiClient { get; }

    protected ApplicationDbContext DbContext { get; }
    protected ITokenProvider TokenProvider { get; }
    protected AppSettings AppSettings { get; }

    protected AuthTestUtils AuthUtils { get; }

    protected static CancellationToken CT => TestContext.Current.CancellationToken;

    protected ApiTestBase(Chess2WebApplicationFactory factory)
    {
        Factory = factory;
        Factory.Server.PreserveExecutionContext = true;
        Scope = Factory.Services.CreateScope();
        ApiClient = Factory.CreateApiClient();

        DbContext = Scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        AppSettings = Scope.ServiceProvider.GetRequiredService<IOptions<AppSettings>>().Value;
        TokenProvider = Scope.ServiceProvider.GetRequiredService<ITokenProvider>();
        var refreshTokenService = Scope.ServiceProvider.GetRequiredService<IRefreshTokenService>();

        AuthUtils = new(TokenProvider, refreshTokenService, AppSettings.Jwt, DbContext);

        // postgres can only store up to microsecond percision,
        // while c# DateTime also stores nanoseconds
        AssertionConfiguration.Current.Equivalency.Modify(options =>
            options
                .Using<DateTime>(ctx =>
                    ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromMicroseconds(1))
                )
                .WhenTypeIs<DateTime>()
        );
    }

    protected async Task<HubConnection> ConnectSignalRGuestAsync(string path, string guestId)
    {
        var token = TokenProvider.GenerateGuestToken(guestId);
        var conn = await ConnectSignalRAsync(path, token);
        return conn;
    }

    protected async Task<HubConnection> ConnectSignalRAuthedAsync(string path, AuthedUser user)
    {
        var token = TokenProvider.GenerateAccessToken(user);
        var conn = await ConnectSignalRAsync(path, token);
        return conn;
    }

    protected async Task<HubConnection> ConnectSignalRAsync(string path, string? accessToken = null)
    {
        var baseAddress =
            ApiClient.Client.BaseAddress
            ?? throw new InvalidOperationException("Base address is not set for ApiClient");
        var server = Factory.Server;

        var connection = new HubConnectionBuilder()
            .WithUrl(
                "http://localhost" + path,
                options =>
                {
                    options.HttpMessageHandlerFactory = _ => server.CreateHandler();
                    if (!string.IsNullOrEmpty(accessToken))
                        options.Headers.Add(
                            "Cookie",
                            $"{AppSettings.Jwt.AccessTokenCookieName}={accessToken}"
                        );
                    options.Transports = HttpTransportType.LongPolling;
                }
            )
            .Build();
        await connection.StartAsync();

        return connection;
    }

    protected async Task ResetShardActors<TActor>()
        where TActor : ActorBase
    {
        var shardActor = Scope.ServiceProvider.GetRequiredService<IRequiredActor<TActor>>();
        var shardState = await shardActor.ActorRef.Ask<CurrentShardRegionState>(
            GetShardRegionState.Instance
        );
        var existingActors = shardState.Shards.SelectMany(shard => shard.EntityIds);

        foreach (var actorId in existingActors)
        {
            shardActor.ActorRef.Tell(new ShardingEnvelope(actorId, PoisonPill.Instance));
        }
    }

    public virtual ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public virtual async ValueTask DisposeAsync()
    {
        await ResetShardActors<PlayerActor>();
        await ResetShardActors<RatedMatchmakingActor>();
        await ResetShardActors<CasualMatchmakingActor>();
        await ResetShardActors<GameActor>();

        await Factory.ResetDatabaseAsync();
        Scope?.Dispose();
        DbContext?.Dispose();

        GC.SuppressFinalize(this);
    }
}
