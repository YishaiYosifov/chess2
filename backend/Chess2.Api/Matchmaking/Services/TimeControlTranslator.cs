using Chess2.Api.Game.Models;

namespace Chess2.Api.Matchmaking.Services;

public interface ITimeControlTranslator
{
    TimeControl FromSeconds(int seconds);
}

public class TimeControlTranslator : ITimeControlTranslator
{
    public TimeControl FromSeconds(int seconds) =>
        seconds switch
        {
            <= 0 => throw new ArgumentOutOfRangeException(nameof(seconds)),
            <= 180 => TimeControl.Bullet, // 3 minutes or less
            <= 300 => TimeControl.Blitz, // 5 minutes or less
            <= 1200 => TimeControl.Rapid, // 20 minutes or less
            _ => TimeControl.Classical,
        };
}
