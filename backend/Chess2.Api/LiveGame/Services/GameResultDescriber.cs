using Chess2.Api.GameLogic.Extensions;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;

namespace Chess2.Api.LiveGame.Services;

public interface IGameResultDescriber
{
    GameEndStatus Aborted(GameColor by);
    GameEndStatus DrawByAgreement();
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

    public GameEndStatus DrawByAgreement() => new(GameResult.Draw, "Draw by Agreement");

    private static GameResult GetWinnerByLoser(GameColor loser) =>
        loser.Match(whenWhite: GameResult.BlackWin, whenBlack: GameResult.WhiteWin);
}
