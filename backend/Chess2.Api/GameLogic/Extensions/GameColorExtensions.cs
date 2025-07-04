using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.GameLogic.Extensions;

public static class GameColorExtensions
{
    public static GameColor Invert(this GameColor color) => (GameColor)(1 ^ (int)color);

    public static T Match<T>(this GameColor color, Func<T> whenWhite, Func<T> whenBlack) =>
        color switch
        {
            GameColor.White => whenWhite(),
            GameColor.Black => whenBlack(),
            _ => throw new ArgumentOutOfRangeException(
                nameof(color),
                color,
                "Invalid GameColor value"
            ),
        };

    public static T Match<T>(this GameColor color, T whenWhite, T whenBlack) =>
        color.Match(() => whenWhite, () => whenBlack);
}
