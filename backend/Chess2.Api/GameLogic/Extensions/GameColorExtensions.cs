using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.GameLogic.Extensions;

public static class GameColorExtensions
{
    public static GameColor Invert(this GameColor color) => (GameColor)(1 ^ (int)color);
}
