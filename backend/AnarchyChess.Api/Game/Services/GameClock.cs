using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;

namespace AnarchyChess.Api.Game.Services;

public interface IGameClock
{
    double CalculateTimeLeft(GameColor color, GameClockState state);
    void CommitLastTurn(GameColor color, GameClockState state);
    double CommitTurn(GameColor color, GameClockState state);
    void Reset(GameClockState state);
    ClockSnapshot ToSnapshot(GameClockState state);
}

[GenerateSerializer]
[Alias("AnarchyChess.Api.Game.Services.GameClockState")]
public class GameClockState
{
    [Id(0)]
    public Dictionary<GameColor, double> Clocks { get; set; } =
        new() { [GameColor.White] = 0, [GameColor.Black] = 0 };

    [Id(1)]
    public required TimeControlSettings TimeControl { get; set; }

    [Id(2)]
    public long LastUpdated { get; set; }

    [Id(3)]
    public bool IsFrozen { get; set; }
}

public class GameClock(TimeProvider timeProvider) : IGameClock
{
    private readonly TimeProvider _timeProvider = timeProvider;

    public ClockSnapshot ToSnapshot(GameClockState state) =>
        new(
            state.Clocks[GameColor.White],
            state.Clocks[GameColor.Black],
            state.LastUpdated,
            state.IsFrozen
        );

    public void Reset(GameClockState state)
    {
        state.Clocks[GameColor.White] = state.TimeControl.BaseSeconds * 1000;
        state.Clocks[GameColor.Black] = state.TimeControl.BaseSeconds * 1000;
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
        if (state.IsFrozen)
            return state.Clocks[color];

        var elapsedMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds() - state.LastUpdated;
        return state.Clocks[color] - elapsedMs;
    }

    public void CommitLastTurn(GameColor color, GameClockState state)
    {
        CommitTurn(color, state);
        state.IsFrozen = true;
    }
}
