using Chess2.Api.Game.Models;
using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.Game.Services;

public class GameClock
{
    private readonly Dictionary<GameColor, float> _clocks = new()
    {
        [GameColor.White] = 0,
        [GameColor.Black] = 0,
    };
    private TimeControlSettings _timeControl = new();
    private long _lastMoveAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();


    public void Reset(TimeControlSettings timeControl)
    {
        _timeControl = timeControl;
        _clocks[GameColor.White] = timeControl.BaseSeconds;
        _clocks[GameColor.Black] = timeControl.BaseSeconds;
    }


    public void TickMove(GameColor color)
    {
        _lastMoveAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var timeLeft = CalculateTimeLeft(color) + _timeControl.IncrementSeconds;
        _clocks[color] = timeLeft;
    }

    public float CalculateTimeLeft(GameColor color)
    {
        var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var timeSinceLastMove = currentTimestamp - _lastMoveAt;

        var timeLeftAtLastMove = _clocks[color];
        var currentTimeLeft = timeLeftAtLastMove - timeSinceLastMove;
        return currentTimeLeft;
    }
}
