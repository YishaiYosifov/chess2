using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Matchmaking.Models;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class PoolKeyFaker : RecordFaker<PoolKey>
{
    public PoolKeyFaker(PoolType? poolType = null)
    {
        StrictMode(true);
        RuleFor(x => x.PoolType, f => poolType ?? f.PickRandom<PoolType>());
        RuleFor(
            x => x.TimeControl,
            f => new TimeControlSettings(
                BaseSeconds: f.Random.Number(100, 1000),
                IncrementSeconds: f.Random.Number(10, 100)
            )
        );
    }
}
