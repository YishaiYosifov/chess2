using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.TestInfrastructure.TestData;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class MovePathFaker : RecordFaker<MovePath>
{
    public MovePathFaker()
    {
        StrictMode(true);
        RuleFor(x => x.FromIdx, f => (byte)f.Random.Number(0, 99));
        RuleFor(x => x.ToIdx, f => (byte)f.Random.Number(0, 99));
        RuleFor(x => x.CapturedIdxs, MoveData.RandomIdxs);
        RuleFor(x => x.TriggerIdxs, MoveData.RandomIdxs);
        RuleFor(
            x => x.SideEffects,
            f => new MoveSideEffectPathFaker().Generate(f.Random.Number(1, 5))
        );
    }
}
