using Chess2.Api.GameSnapshot.Models;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class GameResultDataFaker : RecordFaker<GameResultData>
{
    public GameResultDataFaker(GameResult? result = null)
    {
        StrictMode(true);
        RuleFor(x => x.Result, f => result ?? f.PickRandom<GameResult>());
        RuleFor(x => x.ResultDescription, "test result description");
        RuleFor(x => x.WhiteRatingChange, f => f.Random.Number(-16, 16));
        RuleFor(x => x.BlackRatingChange, f => f.Random.Number(-16, 16));
    }
}
