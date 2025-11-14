using AnarchyChess.Api.GameSnapshot.Models;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class MoveSideEffectPathFaker : RecordFaker<MoveSideEffectPath>
{
    public MoveSideEffectPathFaker()
    {
        StrictMode(true);
        RuleFor(x => x.FromIdx, f => (byte)f.Random.Number(0, 99));
        RuleFor(x => x.ToIdx, f => (byte)f.Random.Number(0, 99));
    }
}
