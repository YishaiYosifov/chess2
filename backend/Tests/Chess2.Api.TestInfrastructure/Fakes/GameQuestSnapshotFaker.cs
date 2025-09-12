using Chess2.Api.GameLogic.Models;
using Chess2.Api.Quests.Models;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class GameQuestSnapshotFaker : RecordFaker<GameQuestSnapshot>
{
    public GameQuestSnapshotFaker()
    {
        StrictMode(true);
        RuleFor(x => x.GameToken, f => f.Random.Guid().ToString()[..16]);
        RuleFor(x => x.PlayerColor, f => f.PickRandom<GameColor>());
        RuleFor(x => x.MoveHistory, f => new MoveFaker().Generate(5));
        RuleFor(x => x.ResultData, f => new GameResultDataFaker().Generate());
    }
}
