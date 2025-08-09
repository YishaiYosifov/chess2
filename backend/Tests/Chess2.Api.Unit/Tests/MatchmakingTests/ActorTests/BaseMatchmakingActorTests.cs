using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.Matchmaking.Grains;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services.Pools;
using Chess2.Api.Shared.Models;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.Utils;
using Chess2.Api.Users.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NSubstitute;
using Orleans.TestKit;

namespace Chess2.Api.Unit.Tests.MatchmakingTests.ActorTests;

public class TestMatchmakingGrain(
    ILogger<TestMatchmakingGrain> logger,
    IGameStarter gameStarter,
    IOptions<AppSettings> settings,
    IMatchmakingPool pool
)
    : AbstractMatchmakingGrain<IMatchmakingPool>(logger, gameStarter, settings, pool),
        IRatedMatchmakingGrain;

public class AbstractMatchmakingGrainTests : BaseGrainTest
{
    private readonly IGameStarter _gameStarterMock = Substitute.For<IGameStarter>();
    private readonly IMatchmakingPool _poolMock = Substitute.For<IMatchmakingPool>();

    private readonly AppSettings _settings = AppSettingsLoader.LoadAppSettings();

    private readonly PoolKey _testPoolKey = new(
        PoolType.Casual,
        new TimeControlSettings(BaseSeconds: 600, IncrementSeconds: 10)
    );

    public AbstractMatchmakingGrainTests()
    {
        Silo.ServiceProvider.AddService(Substitute.For<ILogger<TestMatchmakingGrain>>());
        Silo.ServiceProvider.AddService(Options.Create(_settings));
        Silo.ServiceProvider.AddService(_gameStarterMock);
        Silo.ServiceProvider.AddService(_poolMock);
    }

    private async Task<TestMatchmakingGrain> CreateGrainAsync() =>
        await Silo.CreateGrainAsync<TestMatchmakingGrain>(_testPoolKey.ToGrainKey());

    [Fact]
    public async Task TryCreateSeekAsync_adds_the_seek_and_subscribe_when_pool_accepts()
    {
        var grain = await CreateGrainAsync();

        var seeker = new SeekerFaker().Generate();
        var observer = Substitute.For<IMatchObserver>();

        _poolMock.TryAddSeek(seeker).Returns(true);

        var result = await grain.TryCreateSeekAsync(seeker, observer);

        result.Should().BeTrue();
        _poolMock.Received(1).TryAddSeek(seeker);
    }

    [Fact]
    public async Task TryCreateSeekAsync_returns_false_when_pool_rejects()
    {
        var grain = await CreateGrainAsync();

        var seeker = new SeekerFaker().Generate();
        var observer = Substitute.For<IMatchObserver>();

        _poolMock.TryAddSeek(seeker).Returns(false);

        var result = await grain.TryCreateSeekAsync(seeker, observer);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task TryCancelSeekAsync_removes_seek_and_unsubscribe_when_seek_exists()
    {
        var grain = await CreateGrainAsync();

        UserId userId = "user1";
        _poolMock.RemoveSeek(userId).Returns(true);

        var result = await grain.TryCancelSeekAsync(userId);

        result.Should().BeTrue();
        _poolMock.Received(1).RemoveSeek(userId);
    }

    [Fact]
    public async Task TryCancelSeekAsync_returns_false_when_no_seek_exists()
    {
        var grain = await CreateGrainAsync();

        UserId userId = "user1";
        _poolMock.RemoveSeek(userId).Returns(false);

        var result = await grain.TryCancelSeekAsync(userId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteWaveAsync_starts_games_and_notify_observers_for_matches()
    {
        var grain = await CreateGrainAsync();

        var seeker1 = new SeekerFaker().Generate();
        var seeker2 = new RatedSeekerFaker().Generate();

        _poolMock.CalculateMatches().Returns([(seeker1, seeker2)]);

        var observer1 = Substitute.For<IMatchObserver>();
        var observer2 = Substitute.For<IMatchObserver>();

        _poolMock.TryAddSeek(Arg.Any<Seeker>()).Returns(true);
        await grain.TryCreateSeekAsync(seeker1, observer1);
        await grain.TryCreateSeekAsync(seeker2, observer2);

        var fakeGameToken = Guid.NewGuid().ToString();
        _gameStarterMock
            .StartGameAsync(
                seeker1.UserId,
                seeker2.UserId,
                _testPoolKey.TimeControl,
                isRated: false
            )
            .Returns(Task.FromResult(fakeGameToken));

        await Silo.FireTimerAsync(AbstractMatchmakingGrain<IMatchmakingPool>.WaveTimer);

        await observer1.Received(1).MatchFoundAsync(fakeGameToken, _testPoolKey);
        await observer2.Received(1).MatchFoundAsync(fakeGameToken, _testPoolKey);
    }

    [Fact]
    public async Task ExecuteWaveAsync_starts_rated_games_when_both_seekers_are_rated()
    {
        var grain = await CreateGrainAsync();

        var seeker1 = new RatedSeekerFaker().Generate();
        var seeker2 = new RatedSeekerFaker().Generate();

        _poolMock.CalculateMatches().Returns([(seeker1, seeker2)]);

        var observer1 = Substitute.For<IMatchObserver>();
        var observer2 = Substitute.For<IMatchObserver>();

        _poolMock.TryAddSeek(Arg.Any<Seeker>()).Returns(true);
        await grain.TryCreateSeekAsync(seeker1, observer1);
        await grain.TryCreateSeekAsync(seeker2, observer2);

        var fakeGameToken = Guid.NewGuid().ToString();
        _gameStarterMock
            .StartGameAsync(seeker1.UserId, seeker2.UserId, _testPoolKey.TimeControl, isRated: true)
            .Returns(Task.FromResult(fakeGameToken));

        await Silo.FireTimerAsync(AbstractMatchmakingGrain<IMatchmakingPool>.WaveTimer);

        await observer1.Received(1).MatchFoundAsync(fakeGameToken, _testPoolKey);
        await observer2.Received(1).MatchFoundAsync(fakeGameToken, _testPoolKey);
    }

    [Fact]
    public async Task KeepPoolAlive_delays_deactivation_when_seeker_count_is_greater_than_zero()
    {
        var grain = await CreateGrainAsync();

        _poolMock.SeekerCount.Returns(1);

        await Silo.FireTimerAsync(AbstractMatchmakingGrain<IMatchmakingPool>.ActivationTimer);

        var context = Silo.GetContextFromGrain(grain);
        Silo.GrainRuntime.Mock.Verify(
            i => i.DelayDeactivation(context, TimeSpan.FromMinutes(2)),
            Times.Once
        );
    }
}
