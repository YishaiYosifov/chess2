using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Shared.Services;

namespace Chess2.Api.LiveGame.Services;

public interface IGameClock
{
    ClockSnapshot Value { get; }

    double CalculateTimeLeft(GameColor color);
    void Reset(TimeControlSettings timeControl);
    double CommitTurn(GameColor color);
}

public class GameClock(TimeProvider timeProvider, IStopwatchProvider stopwatchProvider) : IGameClock
{
    private readonly Dictionary<GameColor, double> _clocks = new()
    {
        [GameColor.White] = 0,
        [GameColor.Black] = 0,
    };
    private TimeControlSettings _timeControl = new();

    private readonly IStopwatchProvider _stopwatch = stopwatchProvider;
    private readonly TimeProvider _timeProvider = timeProvider;

    private double _lastUpdated;

    public ClockSnapshot Value =>
        new(_clocks[GameColor.White], _clocks[GameColor.Black], _lastUpdated);

    public void Reset(TimeControlSettings timeControl)
    {
        _timeControl = timeControl;
        _clocks[GameColor.White] = timeControl.BaseSeconds * 1000;
        _clocks[GameColor.Black] = timeControl.BaseSeconds * 1000;
        _stopwatch.Restart();
        _lastUpdated = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
    }

    public double CommitTurn(GameColor color)
    {
        var timeLeft = CalculateTimeLeft(color) + _timeControl.IncrementSeconds * 1000;
        _clocks[color] = timeLeft;
        _stopwatch.Restart();
        _lastUpdated = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

        return timeLeft;
    }

    public double CalculateTimeLeft(GameColor color)
    {
        var elapsedMs = _stopwatch.Elapsed.TotalMilliseconds;
        var timeLeftAtLastMove = _clocks[color];
        var currentTimeLeft = timeLeftAtLastMove - elapsedMs;
        return currentTimeLeft;
    }
}
