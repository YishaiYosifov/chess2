using Bogus;
using Chess2.Api.GameLogic.Extensions;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.QuestLogic.Models;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class GameQuestSnapshotFaker : RecordFaker<GameQuestSnapshot>
{
    public GameQuestSnapshotFaker(GameColor? playerColor = null)
    {
        StrictMode(true);
        RuleFor(x => x.GameToken, f => f.Random.Guid().ToString()[..16]);
        RuleFor(x => x.PlayerColor, f => playerColor ?? f.PickRandom<GameColor>());
        RuleFor(x => x.MoveHistory, f => new MoveFaker().Generate(5));
        RuleFor(x => x.ResultData, f => new GameResultDataFaker().Generate());
    }

    public static Faker<GameQuestSnapshot> Win(GameColor playerColor) =>
        new GameQuestSnapshotFaker(playerColor).RuleFor(
            x => x.ResultData,
            f => new GameResultDataFaker(
                playerColor.Match(whenWhite: GameResult.WhiteWin, whenBlack: GameResult.BlackWin)
            )
        );

    public static Faker<GameQuestSnapshot> Loss(GameColor playerColor) =>
        new GameQuestSnapshotFaker(playerColor).RuleFor(
            x => x.ResultData,
            f => new GameResultDataFaker(
                playerColor.Match(whenWhite: GameResult.BlackWin, whenBlack: GameResult.WhiteWin)
            )
        );
}
