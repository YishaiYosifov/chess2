using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.Hosting;
using Chess2.Api.Auth.Services;
using Chess2.Api.Infrastructure;
using Chess2.Api.LiveGame.Actors;
using Chess2.Api.Matchmaking.Actors;
using Chess2.Api.PlayerSession.Actors;
using Chess2.Api.Shared.Models;
using Chess2.Api.TestInfrastructure.Utils;
using Chess2.Api.Users.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog.Sinks.XUnit.Injectable.Abstract;

namespace Chess2.Api.TestInfrastructure;

public class ApiTestBase : IAsyncLifetime
{
    public Chess2WebApplicationFactory Factory { get; }
    public IServiceScope Scope { get; }
    public ApiClient ApiClient { get; }

    public ApplicationDbContext DbContext { get; }
    public ITokenProvider TokenProvider { get; }
    public AppSettings AppSettings { get; }

    public AuthTestUtils AuthUtils { get; }

    public CancellationToken CT { get; } = TestContext.Current.CancellationToken;

    public ApiTestBase(Chess2WebApplicationFactory factory)
    {
        Factory = factory;
        Factory.Server.PreserveExecutionContext = true;
        var outputSink = Factory.Services.GetRequiredService<IInjectableTestOutputSink>();
        outputSink.Inject(TestContext.Current.TestOutputHelper!);

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
            shardActor.ActorRef.Tell(new ShardingEnvelope(actorId, Kill.Instance));
        }
    }

    public virtual ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public virtual async ValueTask DisposeAsync()
    {
        await ResetShardActors<PlayerSessionActor>();
        await ResetShardActors<RatedMatchmakingActor>();
        await ResetShardActors<CasualMatchmakingActor>();
        await ResetShardActors<GameChatActor>();
        await ResetShardActors<GameActor>();

        await Factory.ResetDatabaseAsync();
        Scope?.Dispose();
        DbContext?.Dispose();

        GC.SuppressFinalize(this);
    }
}
