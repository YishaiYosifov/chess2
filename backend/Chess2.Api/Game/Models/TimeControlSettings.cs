namespace Chess2.Api.Game.Models;

public readonly record struct TimeControlSettings(int BaseSeconds, int IncrementSeconds)
{
    public string ToShortString()
    {
        return $"{BaseSeconds}+{IncrementSeconds}";
    }

    public override string ToString() => ToShortString();

    public static TimeControlSettings FromShortString(string value)
    {
        var parts = value.Split('+');
        if (
            parts.Length != 2
            || !int.TryParse(parts[0], out var baseSeconds)
            || !int.TryParse(parts[1], out var incrementSeconds)
        )
        {
            throw new ArgumentException(
                "Invalid time control format. Expected format is 'base+increment'."
            );
        }
        return new TimeControlSettings(baseSeconds, incrementSeconds);
    }
}
