using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Matchmaking.Models;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class PoolKeyFaker : RecordFaker<PoolKey>
{
    public PoolKeyFaker(PoolType? poolType = null)
    {
        StrictMode(true);
        RuleFor(x => x.PoolType, f => poolType ?? f.PickRandom<PoolType>());
        RuleFor(
            x => x.TimeControl,
            f => new TimeControlSettings(
                BaseSeconds: f.PickRandom<int>(TimeControlSettings.AllowedBaseSeconds),
                IncrementSeconds: f.PickRandom<int>(TimeControlSettings.AllowedIncrementSeconds)
            )
        );
    }
}
