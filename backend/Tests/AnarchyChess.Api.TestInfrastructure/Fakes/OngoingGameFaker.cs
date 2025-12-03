using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Matchmaking.Models;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class OngoingGameFaker : RecordFaker<OngoingGame>
{
    public OngoingGameFaker(PoolKey? poolKey = null)
    {
        StrictMode(true);
        RuleFor(x => x.GameToken, f => (GameToken)f.Random.AlphaNumeric(16));
        RuleFor(x => x.Pool, f => poolKey ?? new PoolKeyFaker().Generate());
        RuleFor(x => x.Opponent, f => new MinimalProfileFaker().Generate());
    }
}
