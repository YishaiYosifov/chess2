using Chess2.Api.Game.Models;
using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.GameLogic.Extensions;

public static class GameColorExtensions
{
    public static GameColor Invert(this GameColor color) => (GameColor)(1 ^ (int)color);

    public static GameResult ToResult(this GameColor color) =>
        color switch
        {
            GameColor.White => GameResult.WhiteWin,
            GameColor.Black => GameResult.BlackWin,
            _ => throw new InvalidOperationException($"Invalid Color {color}?"),
        };
}
