using Chess2.Api.Models;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Services;

public interface ITimeControlTranslator
{
    TimeControl FromSeconds(int seconds);
}

public class TimeControlTranslator(IOptions<AppSettings> settings) : ITimeControlTranslator
{
    private readonly Dictionary<TimeControl, int> _secondsToTimeControl = settings
        .Value.Game.SecondsToTimeControl.OrderByDescending(x => x.Value)
        .ToDictionary();

    public TimeControl FromSeconds(int seconds)
    {
        foreach ((var timeControl, var minSeconds) in _secondsToTimeControl)
        {
            if (seconds >= minSeconds)
                return timeControl;
        }
        // should never happen
        return _secondsToTimeControl.Last().Key;
    }
}
