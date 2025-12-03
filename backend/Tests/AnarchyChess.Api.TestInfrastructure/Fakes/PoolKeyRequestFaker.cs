using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Matchmaking.Models;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class PoolKeyRequestFaker : RecordFaker<PoolKeyRequest>
{
    public PoolKeyRequestFaker(PoolType? poolType = null)
    {
        StrictMode(true);
        RuleFor(x => x.PoolType, f => poolType ?? f.PickRandom<PoolType>());
        RuleFor(
            x => x.TimeControl,
            f => new TimeControlSettingsRequest(
                BaseSeconds: f.PickRandom<int>(TimeControlSettingsRequest.AllowedBaseSeconds),
                IncrementSeconds: f.PickRandom<int>(
                    TimeControlSettingsRequest.AllowedIncrementSeconds
                )
            )
        );
    }
}
