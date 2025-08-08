using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Matchmaking.Grains;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services;
using Chess2.Api.PlayerSession.Grains;
using Chess2.Api.TestInfrastructure.Fakes;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Orleans.TestKit;

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

    [Fact]
    public async Task CreateSeek_sends_seek_to_the_correct_pool()
    {
        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);
        PoolKey poolKey = new(PoolType.Rated, new TimeControlSettings(300, 5));
        var seeker = new RatedSeekerFaker().Generate();

        _ratedPoolGrainMock.TryCreateSeekAsync(seeker, grain).Returns(true);

        await grain.CreateSeekAsync("conn1", seeker, poolKey);

        await _ratedPoolGrainMock.Received(1).TryCreateSeekAsync(seeker, grain);
    }

    [Fact]
    public async Task CreateSeek_cancels_previous_seek_when_connection_reused()
    {
        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);
        PoolKey casualPoolKey = new(PoolType.Casual, new TimeControlSettings(60, 0));
        PoolKey ratedPoolKey = new(PoolType.Rated, new TimeControlSettings(300, 5));

        var casualSeeker = new SeekerFaker().Generate();
        var ratedSeeker = new RatedSeekerFaker().Generate();

        _casualPoolGrainMock.TryCreateSeekAsync(casualSeeker, grain).Returns(Task.FromResult(true));
        _ratedPoolGrainMock.TryCreateSeekAsync(ratedSeeker, grain).Returns(Task.FromResult(true));
        _casualPoolGrainMock.TryCancelSeekAsync(UserId).Returns(Task.FromResult(true));

        await grain.CreateSeekAsync("conn1", casualSeeker, casualPoolKey);
        await _casualPoolGrainMock.Received(1).TryCreateSeekAsync(casualSeeker, grain);

        await grain.CreateSeekAsync("conn1", ratedSeeker, ratedPoolKey);
        await _casualPoolGrainMock.Received(1).TryCancelSeekAsync(UserId);
        await _ratedPoolGrainMock.Received(1).TryCreateSeekAsync(ratedSeeker, grain);
    }

    [Fact]
    public async Task MatchFound_notifies_all_connections_listening_to_same_pool()
    {
        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);
        PoolKey poolKey = new(PoolType.Rated, new TimeControlSettings(180, 2));
        var seeker = new RatedSeekerFaker().Generate();

        _ratedPoolGrainMock.TryCreateSeekAsync(seeker, grain).Returns(Task.FromResult(true));

        await grain.CreateSeekAsync("connA", seeker, poolKey);
        await grain.CreateSeekAsync("connB", seeker, poolKey);

        var gameToken = "game 123";
        await grain.MatchFoundAsync(gameToken, poolKey);

        await _matchmakingNotifierMock.Received(1).NotifyGameFoundAsync("connA", gameToken);
        await _matchmakingNotifierMock.Received(1).NotifyGameFoundAsync("connB", gameToken);

        _matchmakingNotifierMock.ClearReceivedCalls();

        await grain.MatchFoundAsync(gameToken, poolKey);
        _matchmakingNotifierMock.DidNotReceive();
    }

    [Fact]
    public async Task CancelSeek_cleans_up_seek()
    {
        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);
        PoolKey poolKey = new(PoolType.Casual, new TimeControlSettings(10, 1));
        var seeker = new SeekerFaker().Generate();

        _casualPoolGrainMock.TryCreateSeekAsync(seeker, grain).Returns(Task.FromResult(true));
        _casualPoolGrainMock.TryCancelSeekAsync(UserId).Returns(Task.FromResult(true));

        await grain.CreateSeekAsync("conn1", seeker, poolKey);
        await grain.CancelSeekAsync("conn1");

        await _casualPoolGrainMock.Received(1).TryCancelSeekAsync(UserId);

        await grain.MatchFoundAsync("game789", poolKey);
        _matchmakingNotifierMock.DidNotReceive();
    }

    [Fact]
    public async Task CancelSeek_only_removes_correct_seek()
    {
        var grain = await Silo.CreateGrainAsync<PlayerSessionGrain>(UserId);
        PoolKey poolKey = new(PoolType.Rated, new TimeControlSettings(600, 10));
        var seeker = new RatedSeekerFaker().Generate();

        _ratedPoolGrainMock.TryCreateSeekAsync(seeker, grain).Returns(Task.FromResult(true));
        _ratedPoolGrainMock.TryCancelSeekAsync(UserId).Returns(Task.FromResult(true));

        await grain.CreateSeekAsync("connX", seeker, poolKey);
        await grain.CreateSeekAsync("connY", seeker, poolKey);

        await grain.CancelSeekAsync("connX");

        await _ratedPoolGrainMock.Received(1).TryCancelSeekAsync(UserId);

        await grain.MatchFoundAsync("gameY", poolKey);

        await _matchmakingNotifierMock.Received(1).NotifyGameFoundAsync("connY", "gameY");
    }
}
