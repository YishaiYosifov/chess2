using Chess2.Api.GameLogic.Extensions;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;

namespace Chess2.Api.LiveGame.Services;

public interface IGameResultDescriber
{
    GameEndStatus KingCaptured(GameColor by);
    GameEndStatus Resignation(GameColor by);
    GameEndStatus Timeout(GameColor by);
    GameEndStatus Aborted(GameColor by);

    GameEndStatus DrawByAgreement();
    GameEndStatus FiftyMoves();
    GameEndStatus ThreeFold();
}

public class GameResultDescriber : IGameResultDescriber
{
    public GameEndStatus KingCaptured(GameColor by) =>
        new(GetResultByWinner(by), $"{by} Won by King Capture");

    public GameEndStatus Aborted(GameColor by) => new(GameResult.Aborted, $"Game Aborted by {by}");

    public GameEndStatus Resignation(GameColor by) =>
        new(GetResultByLoser(by), $"{by.Invert()} Won by Resignation");

    public GameEndStatus Timeout(GameColor by) =>
        new(GetResultByLoser(by), $"{by.Invert()} Won by Timeout");

    public GameEndStatus ThreeFold() => new(GameResult.Draw, "Draw by 3 Fold Repetition");

    public GameEndStatus FiftyMoves() => new(GameResult.Draw, "Draw by 50 Moves Rule");

    public GameEndStatus DrawByAgreement() => new(GameResult.Draw, "Draw by Agreement");

    private static GameResult GetResultByLoser(GameColor loser) =>
        loser.Match(whenWhite: GameResult.BlackWin, whenBlack: GameResult.WhiteWin);

    private static GameResult GetResultByWinner(GameColor winner) =>
        winner.Match(whenWhite: GameResult.WhiteWin, whenBlack: GameResult.BlackWin);
}
