using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.GameLogic.Extensions;

public static class GameColorExtensions
{
    public static GameColor Invert(this GameColor color) => (GameColor)(1 ^ (int)color);

    public static T Match<T>(this GameColor color, T whenWhite, T whenBlack) =>
        color switch
        {
            GameColor.White => whenWhite,
            GameColor.Black => whenBlack,
            _ => throw new ArgumentOutOfRangeException(
                nameof(color),
                color,
                "Invalid GameColor value"
            ),
        };

    public static T Match<T>(this GameColor? color, T whenWhite, T whenBlack, T whenNeutral) =>
        color switch
        {
            GameColor.White => whenWhite,
            GameColor.Black => whenBlack,
            null => whenNeutral,
            _ => throw new ArgumentOutOfRangeException(
                nameof(color),
                color,
                "Invalid GameColor value"
            ),
        };
}
