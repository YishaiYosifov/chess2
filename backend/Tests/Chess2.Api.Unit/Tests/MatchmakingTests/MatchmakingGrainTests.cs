using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Infrastructure;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.Matchmaking.Errors;
using Chess2.Api.Matchmaking.Grains;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services.Pools;
using Chess2.Api.Matchmaking.Stream;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.Utils;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Orleans.TestKit;
using Orleans.TestKit.Storage;
using Orleans.TestKit.Streams;

namespace Chess2.Api.Unit.Tests.MatchmakingTests;

public class MatchmakingGrainTests : BaseGrainTest
{
    private readonly DateTime _fakeNow = DateTime.UtcNow;
    private readonly IGameStarter _gameStarterMock = Substitute.For<IGameStarter>();
    private readonly TimeProvider _timeProviderMock = Substitute.For<TimeProvider>();
    private readonly PoolKey _testPoolKey = new(
        PoolType.Casual,
        new TimeControlSettings(BaseSeconds: 600, IncrementSeconds: 10)
    );

    private readonly MatchmakingGrainState<CasualMatchmakingPool> _state;
    private readonly TestStorageStats _stateStats;

    public MatchmakingGrainTests()
    {
        _timeProviderMock.GetUtcNow().Returns(_fakeNow);
        var settings = Options.Create(AppSettingsLoader.LoadAppSettings());

        Silo.ServiceProvider.AddService(settings);
        Silo.ServiceProvider.AddService(_timeProviderMock);
        Silo.ServiceProvider.AddService(_gameStarterMock);

        _state = Silo
            .StorageManager.GetStorage<MatchmakingGrainState<CasualMatchmakingPool>>(
                MatchmakingGrain<CasualMatchmakingPool>.StateName
            )
            .State;
        _stateStats = Silo.StorageManager.GetStorageStats(
            MatchmakingGrain<CasualMatchmakingPool>.StateName
        )!;
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

    private async Task<MatchmakingGrain<CasualMatchmakingPool>> CreateGrainAsync() =>
        await Silo.CreateGrainAsync<MatchmakingGrain<CasualMatchmakingPool>>(
            _testPoolKey.ToGrainKey()
        );

    private Task FireWave() =>
        Silo.FireTimerAsync(MatchmakingGrain<CasualMatchmakingPool>.WaveTimer);

    private Task TimeoutWave() =>
        Silo.FireTimerAsync(MatchmakingGrain<CasualMatchmakingPool>.TimeoutTimer);

    [Fact]
    public async Task AddSeekAsync_adds_the_seek_and_doesnt_broadcast()
    {
        var createdStream = ProbeOpenSeekCreatedStream();
        var grain = await CreateGrainAsync();
        var seeker = new CasualSeekerFaker().Generate();
        var observer = Substitute.For<ISeekObserver>();

        await grain.AddSeekAsync(seeker, observer);

        _state.Pool.Seekers.Should().ContainSingle().Which.Should().Be(seeker);
        _stateStats.Writes.Should().BeGreaterThanOrEqualTo(1);
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
        _state.Pool.Seekers.Should().BeEmpty();
        _stateStats.Writes.Should().BeGreaterThanOrEqualTo(1);
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
        await FireWave();

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

        await FireWave();

        await observer1.Received(1).ReleaseReservationAsync(_testPoolKey);
        await observer2.Received(1).ReleaseReservationAsync(_testPoolKey);
        await observer1.Received(1).SeekMatchedAsync(gameToken, _testPoolKey);
        await observer2.Received(1).SeekMatchedAsync(gameToken, _testPoolKey);

        _state.Pool.Seekers.Should().BeEmpty();
        _stateStats.Writes.Should().BeGreaterThanOrEqualTo(1);
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

        await FireWave();

        await observer1.Received(1).ReleaseReservationAsync(_testPoolKey);
        await observer2.Received(1).ReleaseReservationAsync(_testPoolKey);

        _state.Pool.Seekers.Should().BeEquivalentTo([seeker1, seeker2]);
        _stateStats.Writes.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task ExecuteWaveAsync_broadcasts_seek_creation_when_a_seeker_is_not_matched_once()
    {
        var createdStream = ProbeOpenSeekCreatedStream();
        var seeker = new CasualSeekerFaker().Generate();
        var observer = Substitute.For<ISeekObserver>();
        var grain = await CreateGrainAsync();

        await grain.AddSeekAsync(seeker, observer);

        await FireWave();
        createdStream.VerifySend(e => e.Seeker == seeker && e.Pool == _testPoolKey);
        createdStream.VerifySendBatch();

        // second wave does not re-broadcast already cleared pending
        await FireWave();
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

        await TimeoutWave();

        _state.Pool.Seekers.Should().ContainSingle().Which.Should().Be(otherSeeker);
        _stateStats.Writes.Should().BeGreaterThanOrEqualTo(1);
        await observerTimedOut.Received(1).SeekRemovedAsync(_testPoolKey);
        await observerOther.DidNotReceiveWithAnyArgs().SeekRemovedAsync(default!);
    }

    [Fact]
    public async Task MatchWithSeekerAsync_starts_game_and_notifies_matched_seeker()
    {
        var removedStream = ProbeOpenSeekRemovedStream();
        var grain = await CreateGrainAsync();

        var initiatingSeeker = new CasualSeekerFaker().Generate();
        var matchWithSeeker = new CasualSeekerFaker().Generate();
        var matchWithObserver = Substitute.For<ISeekObserver>();

        await grain.AddSeekAsync(matchWithSeeker, matchWithObserver);
        matchWithObserver.TryReserveSeekAsync(_testPoolKey).Returns(Task.FromResult(true));

        var gameToken = "direct-game-123";
        _gameStarterMock
            .StartGameAsync(initiatingSeeker.UserId, matchWithSeeker.UserId, _testPoolKey)
            .Returns(Task.FromResult(gameToken));

        var result = await grain.MatchWithSeekerAsync(initiatingSeeker, matchWithSeeker.UserId);

        result.IsError.Should().BeFalse();
        result.Value.Should().Be(gameToken);

        await matchWithObserver.Received(1).SeekMatchedAsync(gameToken, _testPoolKey);
        await matchWithObserver.Received(1).ReleaseReservationAsync(_testPoolKey);
        _state.Pool.Seekers.Should().NotContain(matchWithSeeker);
        _stateStats.Writes.Should().BeGreaterThanOrEqualTo(1);
        removedStream.Sends.Should().Be(0);
    }

    [Fact]
    public async Task MatchWithSeekerAsync_broadcasts_seek_removal_if_seeker_not_pending()
    {
        var removedStream = ProbeOpenSeekRemovedStream();
        var grain = await CreateGrainAsync();

        var initiatingSeeker = new CasualSeekerFaker().Generate();
        var matchWithSeeker = new CasualSeekerFaker().Generate();
        var matchWithObserver = Substitute.For<ISeekObserver>();

        await grain.AddSeekAsync(matchWithSeeker, matchWithObserver);

        // simulate a wave so the seeker is no longer pending
        await FireWave();

        matchWithObserver.TryReserveSeekAsync(_testPoolKey).Returns(Task.FromResult(true));
        _gameStarterMock
            .StartGameAsync(initiatingSeeker.UserId, matchWithSeeker.UserId, _testPoolKey)
            .Returns(Task.FromResult("gameToken"));

        var result = await grain.MatchWithSeekerAsync(initiatingSeeker, matchWithSeeker.UserId);

        removedStream.VerifySend(e => e.UserId == matchWithSeeker.UserId && e.Pool == _testPoolKey);
    }

    [Fact]
    public async Task MatchWithSeekerAsync_returns_error_when_reservation_fails()
    {
        var grain = await CreateGrainAsync();

        var initiatingSeeker = new CasualSeekerFaker().Generate();
        var matchWithSeeker = new CasualSeekerFaker().Generate();
        var matchWithObserver = Substitute.For<ISeekObserver>();

        await grain.AddSeekAsync(matchWithSeeker, matchWithObserver);
        matchWithObserver.TryReserveSeekAsync(_testPoolKey).Returns(false);

        var result = await grain.MatchWithSeekerAsync(initiatingSeeker, matchWithSeeker.UserId);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(MatchmakingErrors.SeekNotFound);
        await _gameStarterMock
            .DidNotReceiveWithAnyArgs()
            .StartGameAsync(default!, default!, default!);
        await matchWithObserver.DidNotReceiveWithAnyArgs().SeekMatchedAsync(default!, default!);
        await matchWithObserver.DidNotReceiveWithAnyArgs().ReleaseReservationAsync(default!);
        _state.Pool.Seekers.Should().Contain(matchWithSeeker);
        _stateStats.Writes.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task MatchWithSeekerAsync_returns_error_when_requested_seeker_not_found()
    {
        var grain = await CreateGrainAsync();
        var initiatingSeeker = new CasualSeekerFaker().Generate();

        var result = await grain.MatchWithSeekerAsync(initiatingSeeker, "nonexistent-user");

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(MatchmakingErrors.SeekNotFound);
        await _gameStarterMock
            .DidNotReceiveWithAnyArgs()
            .StartGameAsync(default!, default!, default!);
    }

    [Fact]
    public async Task MatchWithSeekerAsync_returns_error_when_seekers_are_incompatible()
    {
        var grain = await CreateGrainAsync();

        var casualRequester = new CasualSeekerFaker().Generate();
        var ratedSeeker = new RatedSeekerFaker().Generate();
        var ratedObserver = Substitute.For<ISeekObserver>();

        await grain.AddSeekAsync(ratedSeeker, ratedObserver);

        var result = await grain.MatchWithSeekerAsync(casualRequester, ratedSeeker.UserId);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(MatchmakingErrors.RequestedSeekerNotCompatible);

        await _gameStarterMock
            .DidNotReceiveWithAnyArgs()
            .StartGameAsync(default!, default!, default!);
        _state.Pool.Seekers.Should().Contain(ratedSeeker);
        await ratedObserver.DidNotReceiveWithAnyArgs().SeekMatchedAsync(default!, default!);
    }
}
