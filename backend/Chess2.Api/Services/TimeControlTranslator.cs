using Chess2.Api.Models;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Services;

public interface ITimeControlTranslator
{
    TimeControl FromSeconds(int seconds);
}

public class TimeControlTranslator(IOptions<AppSettings> settings) : ITimeControlTranslator
{
    private readonly GameSettings _gameSettings = settings.Value.Game;

    public TimeControl FromSeconds(int seconds)
    {
        foreach ((var timeControl, var minSeconds) in _gameSettings.SecondsToTimeControl)
        {
            if (seconds < minSeconds)
                return timeControl;
        }
        // should never happen
        return _gameSettings.SecondsToTimeControl.Last().Key;
    }
}
