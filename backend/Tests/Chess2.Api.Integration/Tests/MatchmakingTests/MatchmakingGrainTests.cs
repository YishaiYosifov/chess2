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
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NSubstitute;
using Orleans.TestKit;
using Orleans.TestKit.Streams;

namespace Chess2.Api.Integration.Tests.MatchmakingTests;

public class MatchmakingGrainTests : BaseOrleansIntegrationTest
{
    private readonly DateTime _fakeNow = DateTime.UtcNow;
    private readonly IGameStarter _gameStarterMock = Substitute.For<IGameStarter>();
    private readonly TimeProvider _timeProviderMock = Substitute.For<TimeProvider>();
    private readonly ICasualMatchmakingPool _pool;
    private readonly PoolKey _testPoolKey = new(
        PoolType.Casual,
        new TimeControlSettings(BaseSeconds: 600, IncrementSeconds: 10)
    );

    public MatchmakingGrainTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _timeProviderMock.GetUtcNow().Returns(_fakeNow);
        var settings = ApiTestBase.Scope.ServiceProvider.GetRequiredService<
            IOptions<AppSettings>
        >();
        _pool = ApiTestBase.Scope.ServiceProvider.GetRequiredService<ICasualMatchmakingPool>();

        Silo.ServiceProvider.AddService(
            Substitute.For<ILogger<MatchmakingGrain<ICasualMatchmakingPool>>>()
        );
        Silo.ServiceProvider.AddService(settings);
        Silo.ServiceProvider.AddService(_timeProviderMock);
        Silo.ServiceProvider.AddService(_gameStarterMock);
        Silo.ServiceProvider.AddService(_pool);
    }

    private TestStream<OpenSeekCreatedEvent> ProbeOpenSeekCreatedStream() =>
        Silo.AddStreamProbe<OpenSeekCreatedEvent>(
            MatchmakingStreamConstants.OpenSeekCreatedStream,
            null,
            Streaming.StreamProvider
        );

    private TestStream<OpenSeekRemovedEvent> ProbeOpenSeekRemovedStream() =>
        Silo.AddStreamProbe<OpenSeekRemovedEvent>(
            MatchmakingStreamConstants.OpenSeekRemovedStream,
            null,
            Streaming.StreamProvider
        );

    private async Task<MatchmakingGrain<ICasualMatchmakingPool>> CreateGrainAsync() =>
        await Silo.CreateGrainAsync<MatchmakingGrain<ICasualMatchmakingPool>>(
            _testPoolKey.ToGrainKey()
        );

    [Fact]
    public async Task AddSeekAsync_adds_the_seek_and_doesnt_broadcast()
    {
        var createdStream = ProbeOpenSeekCreatedStream();
        var grain = await CreateGrainAsync();
        var seeker = new CasualSeekerFaker().Generate();
        var observer = Substitute.For<ISeekObserver>();

        await grain.AddSeekAsync(seeker, observer);

        _pool.Seekers.Should().ContainSingle().Which.Should().Be(seeker);
        createdStream.Sends.Should().Be(0);
    }

    [Fact]
    public async Task TryCancelSeekAsync_removes_seek_and_notifies_when_seek_exists()
    {
        var removedStream = ProbeOpenSeekRemovedStream();
        var grain = await CreateGrainAsync();
        var seeker = new CasualSeekerFaker().Generate();
        var observer = Substitute.For<ISeekObserver>();

        await grain.AddSeekAsync(seeker, observer);

        var result = await grain.TryCancelSeekAsync(seeker.UserId);

        result.Should().BeTrue();
        _pool.Seekers.Should().BeEmpty();
        await observer.Received(1).SeekRemovedAsync(_testPoolKey);
        removedStream.Sends.Should().Be(0); // no broadcast if it was never pending
    }

    [Fact]
    public async Task TryCancelSeekAsync_returns_false_when_no_seek_exists()
    {
        var grain = await CreateGrainAsync();
        var result = await grain.TryCancelSeekAsync("nonexistent");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task TryCancelSeekAsync_broadcasts_cancel_event_after_a_missed_wave()
    {
        var seeker = new CasualSeekerFaker().Generate();
        var observer = Substitute.For<ISeekObserver>();
        var removedStream = ProbeOpenSeekRemovedStream();
        var grain = await CreateGrainAsync();

        await grain.AddSeekAsync(seeker, observer);

        // simulate a wave to broadcast the seeker
        await Silo.FireTimerAsync(MatchmakingGrain<IMatchmakingPool>.WaveTimer);

        await grain.TryCancelSeekAsync(seeker.UserId);

        removedStream.VerifySend(e => e.UserId == seeker.UserId && e.Pool == _testPoolKey);
        await observer.Received(1).SeekRemovedAsync(_testPoolKey);
    }

    [Fact]
    public async Task ExecuteWaveAsync_starts_games_and_notify_observers_for_matches()
    {
        var grain = await CreateGrainAsync();
        var seeker1 = new CasualSeekerFaker().Generate();
        var seeker2 = new CasualSeekerFaker().Generate();
        var observer1 = Substitute.For<ISeekObserver>();
        var observer2 = Substitute.For<ISeekObserver>();

        observer1.TryReserveSeekAsync(_testPoolKey).Returns(Task.FromResult(true));
        observer2.TryReserveSeekAsync(_testPoolKey).Returns(Task.FromResult(true));

        await grain.AddSeekAsync(seeker1, observer1);
        await grain.AddSeekAsync(seeker2, observer2);

        var gameToken = "game123";
        _gameStarterMock
            .StartGameAsync(seeker1.UserId, seeker2.UserId, _testPoolKey)
            .Returns(Task.FromResult(gameToken));

        await Silo.FireTimerAsync(MatchmakingGrain<IMatchmakingPool>.WaveTimer);

        await observer1.Received(1).ReleaseReservationAsync(_testPoolKey);
        await observer2.Received(1).ReleaseReservationAsync(_testPoolKey);
        await observer1.Received(1).SeekMatchedAsync(gameToken, _testPoolKey);
        await observer2.Received(1).SeekMatchedAsync(gameToken, _testPoolKey);

        _pool.Seekers.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteWaveAsync_releases_reservation_if_only_one_seeker_reserves_successfully()
    {
        var grain = await CreateGrainAsync();
        var seeker1 = new CasualSeekerFaker().Generate();
        var seeker2 = new CasualSeekerFaker().Generate();
        var observer1 = Substitute.For<ISeekObserver>();
        var observer2 = Substitute.For<ISeekObserver>();

        observer1.TryReserveSeekAsync(_testPoolKey).Returns(Task.FromResult(true));
        observer2.TryReserveSeekAsync(_testPoolKey).Returns(Task.FromResult(false));

        await grain.AddSeekAsync(seeker1, observer1);
        await grain.AddSeekAsync(seeker2, observer2);

        await Silo.FireTimerAsync(MatchmakingGrain<IMatchmakingPool>.WaveTimer);

        await observer1.Received(1).ReleaseReservationAsync(_testPoolKey);
        await observer2.Received(1).ReleaseReservationAsync(_testPoolKey);

        _pool.Seekers.Should().Contain(seeker1);
        _pool.Seekers.Should().Contain(seeker2);
    }

    [Fact]
    public async Task ExecuteWaveAsync_broadcasts_seek_creation_when_a_seeker_is_not_matched_once()
    {
        var createdStream = ProbeOpenSeekCreatedStream();
        var seeker = new CasualSeekerFaker().Generate();
        var observer = Substitute.For<ISeekObserver>();
        var grain = await CreateGrainAsync();

        await grain.AddSeekAsync(seeker, observer);

        await Silo.FireTimerAsync(MatchmakingGrain<IMatchmakingPool>.WaveTimer);
        createdStream.VerifySend(e => e.Seeker == seeker && e.Pool == _testPoolKey);
        createdStream.VerifySendBatch();

        // second wave does not re-broadcast already cleared pending
        await Silo.FireTimerAsync(MatchmakingGrain<IMatchmakingPool>.WaveTimer);
        createdStream.Sends.Should().Be(1);
    }

    [Fact]
    public async Task TimeoutSeeksAsync_ends_seeks_that_have_timed_out()
    {
        var timedOutSeeker = new CasualSeekerFaker()
            .RuleFor(x => x.CreatedAt, _fakeNow - TimeSpan.FromMinutes(6))
            .Generate();
        var otherSeeker = new CasualSeekerFaker().RuleFor(x => x.CreatedAt, _fakeNow).Generate();
        var observerTimedOut = Substitute.For<ISeekObserver>();
        var observerOther = Substitute.For<ISeekObserver>();

        var grain = await CreateGrainAsync();
        await grain.AddSeekAsync(timedOutSeeker, observerTimedOut);
        await grain.AddSeekAsync(otherSeeker, observerOther);

        await Silo.FireTimerAsync(MatchmakingGrain<IMatchmakingPool>.TimeoutTimer);

        _pool.Seekers.Should().ContainSingle().Which.Should().Be(otherSeeker);
        await observerTimedOut.Received(1).SeekRemovedAsync(_testPoolKey);
        await observerOther.DidNotReceive().SeekRemovedAsync(_testPoolKey);

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

        await Silo.FireTimerAsync(MatchmakingGrain<IMatchmakingPool>.TimeoutTimer);

        var context = Silo.GetContextFromGrain(grain);
        Silo.GrainRuntime.Mock.Verify(x => x.DeactivateOnIdle(context), Times.Once);
        Silo.GrainRuntime.Mock.VerifyNoOtherCalls();
    }
}
