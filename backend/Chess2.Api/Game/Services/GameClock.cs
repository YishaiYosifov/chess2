using System.Diagnostics;
using Chess2.Api.Game.Models;
using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.Game.Services;

public class GameClock
{
    private readonly Dictionary<GameColor, double> _clocks = new()
    {
        [GameColor.White] = 0,
        [GameColor.Black] = 0,
    };
    private TimeControlSettings _timeControl = new();
    private readonly Stopwatch _stopwatch = new();
    private double _lastUpdated;

    public ClockDto Value => new(_clocks[GameColor.White], _clocks[GameColor.Black], _lastUpdated);

    public void Reset(TimeControlSettings timeControl)
    {
        _timeControl = timeControl;
        _clocks[GameColor.White] = timeControl.BaseSeconds * 1000;
        _clocks[GameColor.Black] = timeControl.BaseSeconds * 1000;
        _stopwatch.Restart();
        _lastUpdated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public void TickMove(GameColor color)
    {
        var timeLeft = CalculateTimeLeft(color) + _timeControl.IncrementSeconds * 1000;
        _clocks[color] = timeLeft;
        _stopwatch.Restart();
        _lastUpdated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public double CalculateTimeLeft(GameColor color)
    {
        var elapsedMs = _stopwatch.Elapsed.TotalMilliseconds;
        var timeLeftAtLastMove = _clocks[color];
        var currentTimeLeft = timeLeftAtLastMove - elapsedMs;
        return currentTimeLeft;
    }
}
