using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Infrastructure;
using Chess2.Api.Matchmaking.Grains;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services;
using Chess2.Api.Matchmaking.Stream;
using Chess2.Api.PlayerSession.Grains;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Orleans.TestKit;
using Orleans.TestKit.Streams;

namespace Chess2.Api.Unit.Tests.PlayerTests;

public class PlayerSessionGrainTests : BaseGrainTest
{
    private readonly IMatchmakingNotifier _matchmakingNotifierMock =
        Substitute.For<IMatchmakingNotifier>();

    private readonly IRatedMatchmakingGrain _ratedPoolGrainMock =
        Substitute.For<IRatedMatchmakingGrain>();
    private readonly ICasualMatchmakingGrain _casualPoolGrainMock =
        Substitute.For<ICasualMatchmakingGrain>();

    private const string UserId = "test-user-id";

    public PlayerSessionGrainTests()
    {
        Silo.AddProbe(_ => _ratedPoolGrainMock);
        Silo.AddProbe(_ => _casualPoolGrainMock);

        Silo.ServiceProvider.AddService(Substitute.For<ILogger<PlayerSessionGrain>>());
        Silo.ServiceProvider.AddService(_matchmakingNotifierMock);
    }

    private TestStream<SeekEndedEvent> ProbeSeekEndedStream(PoolKey pool) =>
        Silo.AddStreamProbe<SeekEndedEvent>(
            MatchmakingStreamKey.SeekStream(UserId, pool),
            MatchmakingStreamConstants.EndedStream,
            Streaming.StreamProvider
        );

    [Fact]
    public async Task CreateSeekAsync_sends_seek_to_the_correct_pool_and_subscrives_to_stream()
    {
        PoolKey pool = new(PoolType.Rated, new TimeControlSettings(300, 5));
        var seeker = new RatedSeekerFaker().Generate();
        var stream = ProbeSeekEndedStream(pool);

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);
        await grain.CreateSeekAsync("conn1", seeker, pool);

        await _ratedPoolGrainMock.Received(1).AddSeekAsync(seeker);
        stream.Subscribed.Should().Be(1);
    }

    [Fact]
    public async Task CreateSeekAsync_cancels_previous_seek_when_connection_reused()
    {
        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);
        PoolKey casualPoolKey = new(PoolType.Casual, new TimeControlSettings(60, 0));
        PoolKey ratedPoolKey = new(PoolType.Rated, new TimeControlSettings(300, 5));

        var casualSeeker = new SeekerFaker().Generate();
        var ratedSeeker = new RatedSeekerFaker().Generate();

        _casualPoolGrainMock.AddSeekAsync(casualSeeker).Returns(Task.FromResult(true));
        _ratedPoolGrainMock.AddSeekAsync(ratedSeeker).Returns(Task.FromResult(true));
        _casualPoolGrainMock.TryCancelSeekAsync(UserId).Returns(Task.FromResult(true));

        await grain.CreateSeekAsync("conn1", casualSeeker, casualPoolKey);
        await _casualPoolGrainMock.Received(1).AddSeekAsync(casualSeeker);

        await grain.CreateSeekAsync("conn1", ratedSeeker, ratedPoolKey);
        await _casualPoolGrainMock.Received(1).TryCancelSeekAsync(UserId);
        await _ratedPoolGrainMock.Received(1).AddSeekAsync(ratedSeeker);
    }

    [Fact]
    public async Task CancelSeekAsync_cancels_the_seek_related_to_the_connection_id()
    {
        PoolKey poolToCancel = new(PoolType.Casual, new TimeControlSettings(300, 5));
        PoolKey anotherPool = new(PoolType.Rated, new TimeControlSettings(300, 5));
        var seeker = new RatedSeekerFaker().Generate();

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);
        await grain.CreateSeekAsync("conn1", seeker, poolToCancel);
        await grain.CreateSeekAsync("conn2", seeker, anotherPool);

        await grain.CancelSeekAsync("conn1");

        await _casualPoolGrainMock.Received(1).TryCancelSeekAsync(UserId);
        _ratedPoolGrainMock.DidNotReceive();
    }

    [Fact]
    public async Task SeekEndedEvent_with_gameToken_notifies_all_connections_listening_to_same_pool()
    {
        PoolKey pool = new(PoolType.Rated, new TimeControlSettings(180, 2));
        var seeker = new RatedSeekerFaker().Generate();
        var stream = ProbeSeekEndedStream(pool);

        _ratedPoolGrainMock.AddSeekAsync(seeker).Returns(Task.FromResult(true));

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);
        await grain.CreateSeekAsync("connA", seeker, pool);
        await grain.CreateSeekAsync("connB", seeker, pool);

        var gameToken = "game 123";
        await stream.OnNextAsync(new SeekEndedEvent(gameToken));

        await _matchmakingNotifierMock.Received(1).NotifyGameFoundAsync("connA", gameToken);
        await _matchmakingNotifierMock.Received(1).NotifyGameFoundAsync("connB", gameToken);
        stream.Subscribed.Should().Be(0);
    }

    [Fact]
    public async Task SeekEndedEvent_with_null_gameToken_notifies_match_failed_and_unsubscribes()
    {
        PoolKey pool = new(PoolType.Casual, new TimeControlSettings(10, 1));
        var seeker = new SeekerFaker().Generate();
        var stream = ProbeSeekEndedStream(pool);

        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);
        await grain.CreateSeekAsync("conn1", seeker, pool);
        await grain.CreateSeekAsync("conn2", seeker, pool);

        await stream.OnNextAsync(new SeekEndedEvent(GameToken: null));

        await _matchmakingNotifierMock.Received(1).NotifyMatchFailedAsync("conn1");
        await _matchmakingNotifierMock.Received(1).NotifyMatchFailedAsync("conn2");
        stream.Subscribed.Should().Be(0);
    }
}
