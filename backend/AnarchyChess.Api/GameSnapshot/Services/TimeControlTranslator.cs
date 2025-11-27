using AnarchyChess.Api.GameSnapshot.Models;

namespace AnarchyChess.Api.GameSnapshot.Services;

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
            < 180 => TimeControl.Bullet, // less than 3 minutes
            <= 300 => TimeControl.Blitz, // 5 minutes or less
            <= 1200 => TimeControl.Rapid, // 20 minutes or less
            _ => TimeControl.Classical,
        };
}
