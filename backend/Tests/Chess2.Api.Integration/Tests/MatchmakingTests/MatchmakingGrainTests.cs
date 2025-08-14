using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Infrastructure;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.Matchmaking.Grains;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services.Pools;
using Chess2.Api.Matchmaking.Stream;
using Chess2.Api.Shared.Models;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.Users.Models;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NSubstitute;
using Orleans.TestKit;
using Orleans.TestKit.Streams;

namespace Chess2.Api.Integration.Tests.MatchmakingTests;

public class TestMatchmakingGrain(
    ILogger<TestMatchmakingGrain> logger,
    IGameStarter gameStarter,
    IOptions<AppSettings> settings,
    TimeProvider timeProvider,
    IMatchmakingPool pool
)
    : AbstractMatchmakingGrain<IMatchmakingPool>(logger, gameStarter, settings, timeProvider, pool),
        IRatedMatchmakingGrain;

public class AbstractMatchmakingGrainTests : BaseOrleansIntegrationTest
{
    private readonly DateTime _fakeNow = DateTime.UtcNow;
    private readonly IGameStarter _gameStarterMock = Substitute.For<IGameStarter>();
    private readonly TimeProvider _timeProviderMock = Substitute.For<TimeProvider>();

    private readonly IMatchmakingPool _pool;

    private readonly PoolKey _testPoolKey = new(
        PoolType.Casual,
        new TimeControlSettings(BaseSeconds: 600, IncrementSeconds: 10)
    );

    public AbstractMatchmakingGrainTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _timeProviderMock.GetUtcNow().Returns(_fakeNow);
        var settings = ApiTestBase.Scope.ServiceProvider.GetRequiredService<
            IOptions<AppSettings>
        >();
        _pool = ApiTestBase.Scope.ServiceProvider.GetRequiredService<ICasualMatchmakingPool>();

        Silo.ServiceProvider.AddService(Substitute.For<ILogger<TestMatchmakingGrain>>());
        Silo.ServiceProvider.AddService(settings);
        Silo.ServiceProvider.AddService(_timeProviderMock);
        Silo.ServiceProvider.AddService(_gameStarterMock);
        Silo.ServiceProvider.AddService(_pool);
    }

    private TestStream<PlayerSeekEndedEvent> ProbeSeekEndedStream(UserId userId) =>
        Silo.AddStreamProbe<PlayerSeekEndedEvent>(
            MatchmakingStreamKey.SeekStream(userId, _testPoolKey),
            MatchmakingStreamConstants.PlayerSeekEndedStream,
            Streaming.StreamProvider
        );

    private TestStream<OpenSeekCreatedEvent> ProbeOpenSeekCreatedStream() =>
        Silo.AddStreamProbe<OpenSeekCreatedEvent>(
            MatchmakingStreamConstants.OpenSeekCreatedStream,
            streamNamespace: null,
            Streaming.StreamProvider
        );

    private TestStream<OpenSeekRemovedEvent> ProbeOpenSeekRemovedStream() =>
        Silo.AddStreamProbe<OpenSeekRemovedEvent>(
            MatchmakingStreamConstants.OpenSeekRemovedStream,
            streamNamespace: null,
            Streaming.StreamProvider
        );

    private async Task<TestMatchmakingGrain> CreateGrainAsync() =>
        await Silo.CreateGrainAsync<TestMatchmakingGrain>(_testPoolKey.ToGrainKey());

    [Fact]
    public async Task TryCreateSeekAsync_adds_the_seek_and_doesnt_broadcast()
    {
        var stream = ProbeOpenSeekCreatedStream();
        var seeker = new CasualSeekerFaker().Generate();
        var grain = await CreateGrainAsync();

        await grain.AddSeekAsync(seeker);

        _pool.Seekers.Should().ContainSingle().Which.Should().Be(seeker);
        // we only broadcast after a missed wave
        stream.Sends.Should().Be(0);
    }

    [Fact]
    public async Task TryCancelSeekAsync_removes_seek_and_notifies_when_seek_exists()
    {
        var seeker = new CasualSeekerFaker().Generate();
        var seekEndStream = ProbeSeekEndedStream(seeker.UserId);
        var seekRemovedStream = ProbeOpenSeekRemovedStream();
        var grain = await CreateGrainAsync();

        await grain.AddSeekAsync(seeker);
        var result = await grain.TryCancelSeekAsync(seeker.UserId);

        result.Should().BeTrue();
        _pool.Seekers.Should().BeEmpty();
        seekEndStream.VerifySend(e => e.GameToken == null);
        seekRemovedStream.Sends.Should().Be(0);
    }

    [Fact]
    public async Task TryCancelSeekAsync_returns_false_when_no_seek_exists()
    {
        const string userId = "random user id";
        var seekEndStream = ProbeSeekEndedStream(userId);
        var grain = await CreateGrainAsync();

        var result = await grain.TryCancelSeekAsync(userId);

        result.Should().BeFalse();
        seekEndStream.Sends.Should().Be(0);
    }

    [Fact]
    public async Task TryCancelSeekAsync_broadcasts_cancel_event_after_a_missed_wave()
    {
        var seeker = new CasualSeekerFaker().Generate();
        var seekRemovedStream = ProbeOpenSeekRemovedStream();
        var grain = await CreateGrainAsync();

        await grain.AddSeekAsync(seeker);
        await Silo.FireTimerAsync(AbstractMatchmakingGrain<IMatchmakingPool>.WaveTimer);

        await grain.TryCancelSeekAsync(seeker.UserId);

        seekRemovedStream.VerifySend(e => e.UserId == seeker.UserId && e.Pool == _testPoolKey);
    }

    [Fact]
    public async Task ExecuteWaveAsync_starts_games_and_notify_stream_for_matches()
    {
        var seeker1 = new CasualSeekerFaker().Generate();
        var seeker2 = new CasualSeekerFaker().Generate();
        var seeker1Stream = ProbeSeekEndedStream(seeker1.UserId);
        var seeker2Stream = ProbeSeekEndedStream(seeker2.UserId);
        var removedStream = ProbeOpenSeekRemovedStream();
        var createdStream = ProbeOpenSeekCreatedStream();

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
        removedStream.Sends.Should().Be(0);
        createdStream.Sends.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteWaveAsync_starts_rated_games_when_both_seekers_are_rated()
    {
        var seeker1 = new RatedSeekerFaker(rating: 1200).Generate();
        var seeker2 = new RatedSeekerFaker(rating: 1200).Generate();
        var seeker1Stream = ProbeSeekEndedStream(seeker1.UserId);
        var seeker2Stream = ProbeSeekEndedStream(seeker2.UserId);

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

    [Fact]
    public async Task ExecuteWaveAsync_broadcasts_seek_creation_when_a_seeker_is_not_matched_once()
    {
        var seeker = new CasualSeekerFaker().Generate();
        var createdStream = ProbeOpenSeekCreatedStream();
        var grain = await CreateGrainAsync();
        await grain.AddSeekAsync(seeker);

        await Silo.FireTimerAsync(AbstractMatchmakingGrain<IMatchmakingPool>.WaveTimer);

        createdStream.VerifySend(e => e.Seeker == seeker && e.Pool == _testPoolKey);
        createdStream.VerifySendBatch();

        await Silo.FireTimerAsync(AbstractMatchmakingGrain<IMatchmakingPool>.WaveTimer);

        createdStream.Sends.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteWaveAsync_broadcasts_seek_removed_after_seek_is_broadcasted_as_created()
    {
        var seeker1 = new CasualSeekerFaker().Generate();
        var seeker2 = new CasualSeekerFaker().Generate();
        var removedStream = ProbeOpenSeekCreatedStream();
        var grain = await CreateGrainAsync();
        await grain.AddSeekAsync(seeker1);

        // notify seeker1 created
        await Silo.FireTimerAsync(AbstractMatchmakingGrain<IMatchmakingPool>.WaveTimer);

        await grain.AddSeekAsync(seeker2);

        await Silo.FireTimerAsync(AbstractMatchmakingGrain<IMatchmakingPool>.WaveTimer);

        removedStream.VerifySend(e => e.Seeker == seeker1 && e.Pool == _testPoolKey);
    }

    [Fact]
    public async Task TimeoutSeeksAsync_ends_seeks_that_have_timed_out()
    {
        var timedOutSeeker = new CasualSeekerFaker()
            .RuleFor(x => x.CreatedAt, _fakeNow - TimeSpan.FromMinutes(5))
            .Generate();
        var otherSeeker = new CasualSeekerFaker().RuleFor(x => x.CreatedAt, _fakeNow).Generate();
        var timedOutSeekerStream = ProbeSeekEndedStream(timedOutSeeker.UserId);
        var otherSeekerStream = ProbeSeekEndedStream(otherSeeker.UserId);

        var grain = await CreateGrainAsync();
        await grain.AddSeekAsync(timedOutSeeker);
        await grain.AddSeekAsync(otherSeeker);

        await Silo.FireTimerAsync(AbstractMatchmakingGrain<IMatchmakingPool>.TimeoutTimer);

        timedOutSeekerStream.VerifySend(e => e.GameToken == null);
        otherSeekerStream.Sends.Should().Be(0);
        _pool.Seekers.Should().ContainSingle().Which.Should().Be(otherSeeker);

        var context = Silo.GetContextFromGrain(grain);
        Silo.GrainRuntime.Mock.Verify(
            x => x.DelayDeactivation(context, TimeSpan.FromMinutes(5)),
            Times.Once
        );
        Silo.GrainRuntime.Mock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task TimeoutSeeksAsync_deactivates_when_no_seeks()
    {
        var grain = await CreateGrainAsync();

        await Silo.FireTimerAsync(AbstractMatchmakingGrain<IMatchmakingPool>.TimeoutTimer);

        var context = Silo.GetContextFromGrain(grain);
        Silo.GrainRuntime.Mock.Verify(x => x.DeactivateOnIdle(context), Times.Once);
        Silo.GrainRuntime.Mock.VerifyNoOtherCalls();
    }
}
