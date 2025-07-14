using Chess2.Api.Game.Models;
using Chess2.Api.GameLogic.Extensions;
using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.Game.Services;

public interface IGameResultDescriber
{
    GameEndStatus Aborted(GameColor by);
    GameEndStatus FiftyMoves();
    GameEndStatus Resignation(GameColor loser);
    GameEndStatus ThreeFold();
    GameEndStatus Timeout(GameColor loser);
}

public class GameResultDescriber : IGameResultDescriber
{
    public GameEndStatus Aborted(GameColor by) => new(GameResult.Aborted, $"Game Aborted by {by}");

    public GameEndStatus Resignation(GameColor loser) =>
        new(GetWinnerByLoser(loser), $"{loser.Invert()} Won by Resignation");

    public GameEndStatus Timeout(GameColor loser) =>
        new(GetWinnerByLoser(loser), $"{loser.Invert()} Won by Timeout");

    public GameEndStatus ThreeFold() => new(GameResult.Draw, "Draw by 3 Fold Repetition");

    public GameEndStatus FiftyMoves() => new(GameResult.Draw, "Draw by 50 Moves Rule");

    private static GameResult GetWinnerByLoser(GameColor loser) =>
        loser.Match(whenWhite: GameResult.BlackWin, whenBlack: GameResult.WhiteWin);
}
