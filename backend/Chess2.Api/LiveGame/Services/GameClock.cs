using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;

namespace Chess2.Api.LiveGame.Services;

public interface IGameClock
{
    double CalculateTimeLeft(GameColor color, GameClockState state);
    double CommitTurn(GameColor color, GameClockState state);
    void Reset(TimeControlSettings timeControl, GameClockState state);
    ClockSnapshot ToSnapshot(GameClockState state);
}

public class GameClockState
{
    public Dictionary<GameColor, double> Clocks { get; set; } =
        new() { [GameColor.White] = 0, [GameColor.Black] = 0 };

    public TimeControlSettings TimeControl { get; set; } = new();

    public long LastUpdated { get; set; }
}

public class GameClock(TimeProvider timeProvider) : IGameClock
{
    private readonly TimeProvider _timeProvider = timeProvider;

    public ClockSnapshot ToSnapshot(GameClockState state) =>
        new(state.Clocks[GameColor.White], state.Clocks[GameColor.Black], state.LastUpdated);

    public void Reset(TimeControlSettings timeControl, GameClockState state)
    {
        state.TimeControl = timeControl;
        state.Clocks[GameColor.White] = timeControl.BaseSeconds * 1000;
        state.Clocks[GameColor.Black] = timeControl.BaseSeconds * 1000;
        state.LastUpdated = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
    }

    public double CommitTurn(GameColor color, GameClockState state)
    {
        var timeLeft = CalculateTimeLeft(color, state) + state.TimeControl.IncrementSeconds * 1000;
        state.Clocks[color] = timeLeft;
        state.LastUpdated = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

        return timeLeft;
    }

    public double CalculateTimeLeft(GameColor color, GameClockState state)
    {
        var elapsedMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds() - state.LastUpdated;
        return state.Clocks[color] - elapsedMs;
    }
}
