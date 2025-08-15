using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Infrastructure;
using Chess2.Api.Lobby.Errors;
using Chess2.Api.Lobby.Grains;
using Chess2.Api.Lobby.Services;
using Chess2.Api.Matchmaking.Grains;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Stream;
using Chess2.Api.Shared.Models;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.Utils;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Orleans.TestKit;
using Orleans.TestKit.Streams;

namespace Chess2.Api.Unit.Tests.PlayerTests;

public class PlayerSessionGrainTests : BaseGrainTest
{
    private readonly ILobbyNotifier _matchmakingNotifierMock = Substitute.For<ILobbyNotifier>();
    private readonly LobbySettings _settings;

    private readonly IRatedMatchmakingGrain _ratedPoolGrainMock =
        Substitute.For<IRatedMatchmakingGrain>();
    private readonly ICasualMatchmakingGrain _casualPoolGrainMock =
        Substitute.For<ICasualMatchmakingGrain>();

    private const string UserId = "test-user-id";

    public PlayerSessionGrainTests()
    {
        Silo.AddProbe(_ => _ratedPoolGrainMock);
        Silo.AddProbe(_ => _casualPoolGrainMock);

        var settings = AppSettingsLoader.LoadAppSettings();
        _settings = settings.Lobby;

        Silo.ServiceProvider.AddService(Substitute.For<ILogger<PlayerSessionGrain>>());
        Silo.ServiceProvider.AddService(_matchmakingNotifierMock);
        Silo.ServiceProvider.AddService(Options.Create(settings));
    }

    private TestStream<PlayerSeekEndedEvent> ProbeSeekEndedStream(PoolKey pool) =>
        Silo.AddStreamProbe<PlayerSeekEndedEvent>(
            MatchmakingStreamKey.SeekStream(UserId, pool),
            MatchmakingStreamConstants.PlayerSeekEndedStream,
            Streaming.StreamProvider
        );

    [Fact]
    public async Task CreateSeekAsync_sends_seek_and_subscribes_to_stream()
    {
        PoolKey pool = new(PoolType.Rated, new TimeControlSettings(300, 5));
        var seeker = new RatedSeekerFaker(UserId).Generate();
        var stream = ProbeSeekEndedStream(pool);

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);
        var result = await grain.CreateSeekAsync("conn1", seeker, pool);

        result.IsError.Should().BeFalse();
        await _ratedPoolGrainMock.Received(1).AddSeekAsync(seeker);
        stream.Subscribed.Should().Be(1);
    }

    [Fact]
    public async Task CreateSeekAsync_allows_multiple_different_connections()
    {
        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);
        PoolKey pool = new(PoolType.Casual, new TimeControlSettings(60, 0));
        var seeker = new CasualSeekerFaker(UserId).Generate();

        (await grain.CreateSeekAsync("conn1", seeker, pool)).IsError.Should().BeFalse();
        (await grain.CreateSeekAsync("conn2", seeker, pool)).IsError.Should().BeFalse();

        await _casualPoolGrainMock.Received(2).AddSeekAsync(seeker);
    }

    [Fact]
    public async Task CreateSeekAsync_rejects_when_too_many_active_games()
    {
        var seeker = new CasualSeekerFaker(UserId).Generate();
        PoolKey pool = new(PoolType.Casual, new TimeControlSettings(60, 0));
        var stream = ProbeSeekEndedStream(pool);
        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);

        var max = _settings.MaxActiveGames;
        var gamesToCreate = max / 2;
        var seeksToCreate = max - gamesToCreate;

        for (var i = 0; i < gamesToCreate; i++)
        {
            (await grain.CreateSeekAsync("conn", seeker, pool)).IsError.Should().BeFalse();
            await stream.OnNextAsync(new($"test{i}"));
        }

        for (var i = 0; i < seeksToCreate; i++)
        {
            (await grain.CreateSeekAsync($"conn{i}", seeker, pool)).IsError.Should().BeFalse();
        }

        var result = await grain.CreateSeekAsync("final conn", seeker, pool);
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(PlayerSessionErrors.TooManyGames);
    }

    [Fact]
    public async Task CleanupConnectionAsync_removes_connection_and_cancels_pools_with_no_other_seekers()
    {
        PoolKey poolToRemove = new(PoolType.Casual, new TimeControlSettings(300, 5));
        PoolKey poolStillActive = new(PoolType.Rated, new TimeControlSettings(300, 5));
        var stream = ProbeSeekEndedStream(poolToRemove);
        var seeker = new RatedSeekerFaker(UserId).Generate();

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);
        await grain.CreateSeekAsync("conn1", seeker, poolToRemove);
        await grain.CreateSeekAsync("conn2", seeker, poolStillActive);
        await grain.CreateSeekAsync("conn3", seeker, poolToRemove); // keeps poolToRemove active

        await grain.CleanupConnectionAsync("conn1");

        await _casualPoolGrainMock.DidNotReceive().TryCancelSeekAsync(UserId);
        await _ratedPoolGrainMock.DidNotReceive().TryCancelSeekAsync(UserId);
        stream.Subscribed.Should().Be(1);
    }

    [Fact]
    public async Task CancelSeekAsync_cancels_seek_and_notifies_match_failed()
    {
        PoolKey poolToRemove = new(PoolType.Rated, new TimeControlSettings(300, 5));
        PoolKey poolStillActive = new(PoolType.Casual, new TimeControlSettings(300, 5));
        var removeStream = ProbeSeekEndedStream(poolToRemove);
        var activeStream = ProbeSeekEndedStream(poolStillActive);
        var seeker = new RatedSeekerFaker(UserId).Generate();

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);
        await grain.CreateSeekAsync("conn1", seeker, poolToRemove);
        await grain.CreateSeekAsync("conn2", seeker, poolToRemove);
        await grain.CreateSeekAsync("conn3", seeker, poolStillActive);

        await grain.CancelSeekAsync(poolToRemove);

        List<ConnectionId> expectedConns = ["conn1", "conn2"];
        _casualPoolGrainMock.DidNotReceive();
        await _ratedPoolGrainMock.Received(1).TryCancelSeekAsync(UserId);
        await _matchmakingNotifierMock
            .Received(1)
            .NotifyMatchFailedAsync(
                Arg.Is<IReadOnlyList<ConnectionId>>(ids => ids.SequenceEqual(expectedConns))
            );
        removeStream.Subscribed.Should().Be(0);
        activeStream.Subscribed.Should().Be(1);
    }

    [Fact]
    public async Task SeekEndedEvent_with_gameToken_notifies_and_cleans_up()
    {
        PoolKey pool = new(PoolType.Rated, new TimeControlSettings(180, 2));
        var seeker = new RatedSeekerFaker(UserId).Generate();
        var stream = ProbeSeekEndedStream(pool);

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);
        await grain.CreateSeekAsync("conn1", seeker, pool);
        await grain.CreateSeekAsync("conn2", seeker, pool);

        var gameToken = "game 123";
        await stream.OnNextAsync(new PlayerSeekEndedEvent(gameToken));

        List<ConnectionId> expectedConns = ["conn1", "conn2"];
        await _matchmakingNotifierMock
            .Received(1)
            .NotifyGameFoundAsync(
                Arg.Is<IReadOnlyList<ConnectionId>>(ids => ids.SequenceEqual(expectedConns)),
                gameToken
            );
        stream.Subscribed.Should().Be(0);
    }

    [Fact]
    public async Task SeekEndedEvent_with_null_gameToken_notifies_match_failed()
    {
        PoolKey pool = new(PoolType.Casual, new TimeControlSettings(10, 1));
        var seeker = new CasualSeekerFaker(UserId).Generate();
        var stream = ProbeSeekEndedStream(pool);

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);
        await grain.CreateSeekAsync("conn1", seeker, pool);
        await grain.CreateSeekAsync("conn2", seeker, pool);

        await stream.OnNextAsync(new PlayerSeekEndedEvent(GameToken: null));

        await _matchmakingNotifierMock
            .Received(1)
            .NotifyMatchFailedAsync(
                Arg.Is<IReadOnlyList<ConnectionId>>(ids =>
                    ids.Contains("conn1") && ids.Contains("conn2")
                )
            );
        stream.Subscribed.Should().Be(0);
    }
}
