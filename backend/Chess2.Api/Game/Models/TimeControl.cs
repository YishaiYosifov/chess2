namespace Chess2.Api.Game.Models;

public enum TimeControl
{
    Bullet,
    Blitz,
    Rapid,
    Classical,
}

public record TimeControlInfo(int BaseMinutes, int Increment);
