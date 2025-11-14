using AnarchyChess.Api.GameLogic.Extensions;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;

namespace AnarchyChess.Api.Game.Services;

public interface IGameResultDescriber
{
    GameEndStatus KingSelfCapture(GameColor by);
    GameEndStatus KingCaptured(GameColor by);
    GameEndStatus Resignation(GameColor by);
    GameEndStatus Timeout(GameColor by);
    GameEndStatus Aborted(GameColor by);

    GameEndStatus DrawByAgreement();
    GameEndStatus FiftyMoves();
    GameEndStatus ThreeFold();
    GameEndStatus KingTouch();
}

public class GameResultDescriber : IGameResultDescriber
{
    public GameEndStatus KingCaptured(GameColor by) =>
        new(GetResultByWinner(by), $"{by} Captured the King");

    public GameEndStatus KingSelfCapture(GameColor by) =>
        new(GetResultByLoser(by), $"{by} Captured Their Own King");

    public GameEndStatus Aborted(GameColor by) => new(GameResult.Aborted, $"Game Aborted by {by}");

    public GameEndStatus Resignation(GameColor by) =>
        new(GetResultByLoser(by), $"{by.Invert()} Won by Resignation");

    public GameEndStatus Timeout(GameColor by) =>
        new(GetResultByLoser(by), $"{by.Invert()} Won by Timeout");

    public GameEndStatus ThreeFold() => new(GameResult.Draw, "Draw by 3-Fold Repetition");

    public GameEndStatus FiftyMoves() => new(GameResult.Draw, "Draw by 50 Moves Rule");

    public GameEndStatus DrawByAgreement() => new(GameResult.Draw, "Draw by Agreement");

    public GameEndStatus KingTouch() => new(GameResult.Draw, "Draw by King Touch");

    private static GameResult GetResultByLoser(GameColor loser) =>
        loser.Match(whenWhite: GameResult.BlackWin, whenBlack: GameResult.WhiteWin);

    private static GameResult GetResultByWinner(GameColor winner) =>
        winner.Match(whenWhite: GameResult.WhiteWin, whenBlack: GameResult.BlackWin);
}
