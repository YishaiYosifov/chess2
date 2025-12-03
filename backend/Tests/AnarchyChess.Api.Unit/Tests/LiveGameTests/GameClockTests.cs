using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AwesomeAssertions;
using NSubstitute;

namespace AnarchyChess.Api.Unit.Tests.LiveGameTests;

public class GameClockTests
{
    private readonly GameClock _clock;
    private readonly GameClockState _state = new();

    private readonly TimeProvider _timeProviderMock = Substitute.For<TimeProvider>();

    public GameClockTests()
    {
        _clock = new(_timeProviderMock);
    }

    [Fact]
    public void Reset_sets_clocks_to_base_seconds_and_updates_last_updated()
    {
        TimeControlSettings timeControl = new(BaseSeconds: 300, IncrementSeconds: 10);
        var now = DateTimeOffset.UtcNow;
        _timeProviderMock.GetUtcNow().Returns(now);

        _clock.Reset(timeControl, _state);

        _state.Clocks[GameColor.White].Should().Be(300_000);
        _state.Clocks[GameColor.Black].Should().Be(300_000);
        _state.TimeControl.Should().Be(timeControl);
        _state.LastUpdated.Should().Be(now.ToUnixTimeMilliseconds());
        _state.IsFrozen.Should().BeFalse();
    }

    [Fact]
    public void CommitTurn_updates_clock_with_elapsed_and_increment()
    {
        _state.Clocks[GameColor.White] = 120_000;
        _state.Clocks[GameColor.Black] = 120_000;
        _state.TimeControl = new TimeControlSettings(BaseSeconds: 120, IncrementSeconds: 10);
        _state.LastUpdated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        int elapsed = 5_000;
        var now = DateTimeOffset.FromUnixTimeMilliseconds(_state.LastUpdated + elapsed);
        _timeProviderMock.GetUtcNow().Returns(now);

        var result = _clock.CommitTurn(GameColor.White, _state);

        result.Should().Be(125_000); // 120000 - 5000 + 10000
        _state.Clocks[GameColor.White].Should().Be(125_000);
        _state.LastUpdated.Should().Be(now.ToUnixTimeMilliseconds());
    }

    [Fact]
    public void CalculateTimeLeft_returns_clock_minus_elapsed_when_not_frozen()
    {
        _state.Clocks[GameColor.White] = 90_000;
        _state.Clocks[GameColor.Black] = 90_000;
        _state.LastUpdated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        _state.IsFrozen = false;

        int elapsed = 15_000;
        var now = DateTimeOffset.FromUnixTimeMilliseconds(_state.LastUpdated + elapsed);
        _timeProviderMock.GetUtcNow().Returns(now);

        var timeLeft = _clock.CalculateTimeLeft(GameColor.White, _state);

        timeLeft.Should().Be(75_000); // 90000 - 15000
    }

    [Fact]
    public void CalculateTimeLeft_returns_clock_as_is_when_frozen()
    {
        _state.Clocks[GameColor.White] = 50_000;
        _state.IsFrozen = true;
        _state.LastUpdated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Even if time has passed, frozen clock should not decrease
        var now = DateTimeOffset.FromUnixTimeMilliseconds(_state.LastUpdated + 10_000);
        _timeProviderMock.GetUtcNow().Returns(now);

        var timeLeft = _clock.CalculateTimeLeft(GameColor.White, _state);

        timeLeft.Should().Be(50_000);
    }

    [Fact]
    public void ToSnapshot_returns_snapshot_with_correct_values()
    {
        _state.Clocks[GameColor.White] = 50_000;
        _state.Clocks[GameColor.Black] = 60_000;
        _state.LastUpdated = 1234567890;
        _state.IsFrozen = true;

        var snapshot = _clock.ToSnapshot(_state);

        snapshot.WhiteClock.Should().Be(50_000);
        snapshot.BlackClock.Should().Be(60_000);
        snapshot.LastUpdated.Should().Be(1234567890);
        snapshot.IsFrozen.Should().BeTrue();
    }

    [Fact]
    public void CommitTurn_does_not_affect_opponent_clock()
    {
        _state.Clocks[GameColor.White] = 120_000;
        _state.Clocks[GameColor.Black] = 120_000;
        _state.TimeControl = new TimeControlSettings(BaseSeconds: 120, IncrementSeconds: 10);
        _state.LastUpdated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var now = DateTimeOffset.FromUnixTimeMilliseconds(_state.LastUpdated + 3_000);
        _timeProviderMock.GetUtcNow().Returns(now);

        _clock.CommitTurn(GameColor.White, _state);

        _state.Clocks[GameColor.Black].Should().Be(120_000);
    }

    [Fact]
    public void CalculateTimeLeft_returns_negative_if_elapsed_exceeds_clock()
    {
        _state.Clocks[GameColor.White] = 5_000;
        _state.LastUpdated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var now = DateTimeOffset.FromUnixTimeMilliseconds(_state.LastUpdated + 10_000);
        _timeProviderMock.GetUtcNow().Returns(now);

        var timeLeft = _clock.CalculateTimeLeft(GameColor.White, _state);

        timeLeft.Should().Be(-5_000);
    }

    [Fact]
    public void CommitLastTurn_freezes_clock_and_updates_time()
    {
        _state.Clocks[GameColor.White] = 100_000;
        _state.TimeControl = new TimeControlSettings(BaseSeconds: 100, IncrementSeconds: 10);
        _state.LastUpdated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        _state.IsFrozen = false;

        var now = DateTimeOffset.FromUnixTimeMilliseconds(_state.LastUpdated + 2_000);
        _timeProviderMock.GetUtcNow().Returns(now);

        _clock.CommitLastTurn(GameColor.White, _state);

        _state.IsFrozen.Should().BeTrue();
        _state.Clocks[GameColor.White].Should().Be(108_000); // 100000 - 2000 + 10000
        _state.LastUpdated.Should().Be(now.ToUnixTimeMilliseconds());
    }
}
