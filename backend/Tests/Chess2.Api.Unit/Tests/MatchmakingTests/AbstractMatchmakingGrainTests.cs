using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Infrastructure;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.Matchmaking.Grains;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services.Pools;
using Chess2.Api.Matchmaking.Stream;
using Chess2.Api.Shared.Models;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.Utils;
using Chess2.Api.Users.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Orleans.TestKit;
using Orleans.TestKit.Streams;

namespace Chess2.Api.Unit.Tests.MatchmakingTests;

public class TestMatchmakingGrain(
    ILogger<TestMatchmakingGrain> logger,
    IGameStarter gameStarter,
    IOptions<AppSettings> settings,
    TimeProvider timeProvider,
    IMatchmakingPool pool
)
    : AbstractMatchmakingGrain<IMatchmakingPool>(logger, gameStarter, settings, timeProvider, pool),
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

    private TestStream<SeekEndedEvent> ProbeSeekEndedStream(UserId userId) =>
        Silo.AddStreamProbe<SeekEndedEvent>(
            MatchmakingStreamKey.SeekStream(userId, _testPoolKey),
            MatchmakingStreamConstants.EndedStream,
            Streaming.StreamProvider
        );

    private async Task<TestMatchmakingGrain> CreateGrainAsync() =>
        await Silo.CreateGrainAsync<TestMatchmakingGrain>(_testPoolKey.ToGrainKey());

    [Fact]
    public async Task TryCreateSeekAsync_adds_the_seek()
    {
        var seeker = new SeekerFaker().Generate();
        var grain = await CreateGrainAsync();

        await grain.AddSeekAsync(seeker);

        _poolMock.Received(1).AddSeek(seeker);
    }

    [Fact]
    public async Task TryCancelSeekAsync_removes_seek_and_notifies_when_seek_exists()
    {
        UserId userId = "user1";
        var stream = ProbeSeekEndedStream(userId);
        var grain = await CreateGrainAsync();

        _poolMock.RemoveSeek(userId).Returns(true);

        var result = await grain.TryCancelSeekAsync(userId);

        result.Should().BeTrue();
        _poolMock.Received(1).RemoveSeek(userId);
        stream.VerifySend(e => e.GameToken == null);
    }

    [Fact]
    public async Task TryCancelSeekAsync_returns_false_when_no_seek_exists()
    {
        UserId userId = "user1";
        var stream = ProbeSeekEndedStream(userId);
        var grain = await CreateGrainAsync();

        _poolMock.RemoveSeek(userId).Returns(false);

        var result = await grain.TryCancelSeekAsync(userId);

        result.Should().BeFalse();
        stream.Sends.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteWaveAsync_starts_games_and_notify_stream_for_matches()
    {
        var seeker1 = new SeekerFaker().Generate();
        var seeker2 = new RatedSeekerFaker().Generate();
        var seeker1Stream = ProbeSeekEndedStream(seeker1.UserId);
        var seeker2Stream = ProbeSeekEndedStream(seeker2.UserId);

        _poolMock.CalculateMatches().Returns([(seeker1, seeker2)]);

        var grain = await CreateGrainAsync();
        await grain.AddSeekAsync(seeker1);
        await grain.AddSeekAsync(seeker2);

        var testGameToken = "test game token 123";
        _gameStarterMock
            .StartGameAsync(
                seeker1.UserId,
                seeker2.UserId,
                _testPoolKey.TimeControl,
                isRated: false
            )
            .Returns(Task.FromResult(testGameToken));

        await Silo.FireTimerAsync(AbstractMatchmakingGrain<IMatchmakingPool>.WaveTimer);

        seeker1Stream.VerifySend(e => e.GameToken == testGameToken);
        seeker2Stream.VerifySend(e => e.GameToken == testGameToken);
    }

    [Fact]
    public async Task ExecuteWaveAsync_starts_rated_games_when_both_seekers_are_rated()
    {
        var seeker1 = new RatedSeekerFaker().Generate();
        var seeker2 = new RatedSeekerFaker().Generate();
        var seeker1Stream = ProbeSeekEndedStream(seeker1.UserId);
        var seeker2Stream = ProbeSeekEndedStream(seeker2.UserId);

        _poolMock.CalculateMatches().Returns([(seeker1, seeker2)]);

        var grain = await CreateGrainAsync();
        await grain.AddSeekAsync(seeker1);
        await grain.AddSeekAsync(seeker2);

        var testGameToken = "test game token 123";
        _gameStarterMock
            .StartGameAsync(seeker1.UserId, seeker2.UserId, _testPoolKey.TimeControl, isRated: true)
            .Returns(Task.FromResult(testGameToken));

        await Silo.FireTimerAsync(AbstractMatchmakingGrain<IMatchmakingPool>.WaveTimer);

        seeker1Stream.VerifySend(e => e.GameToken == testGameToken);
        seeker2Stream.VerifySend(e => e.GameToken == testGameToken);
    }
}
