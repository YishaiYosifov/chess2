using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.TestInfrastructure.TestData;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class MovePathFaker : RecordFaker<MovePath>
{
    public MovePathFaker()
    {
        StrictMode(true);
        RuleFor(x => x.FromIdx, f => (byte)f.Random.Number(0, 99));
        RuleFor(x => x.ToIdx, f => (byte)f.Random.Number(0, 99));
        RuleFor(x => x.MoveKey, f => f.Random.String2(10));
        RuleFor(x => x.CapturedIdxs, MoveData.RandomIdxs);
        RuleFor(x => x.TriggerIdxs, MoveData.RandomIdxs);
        RuleFor(
            x => x.IntermediateSquares,
            f => new IntermediateSquarePathFaker().Generate(f.Random.Number(1, 5))
        );
        RuleFor(
            x => x.SideEffects,
            f => new MoveSideEffectPathFaker().Generate(f.Random.Number(1, 5))
        );
        RuleFor(x => x.PieceSpawns, f => new PieceSpawnPathFaker().Generate(f.Random.Number(1, 5)));
        RuleFor(x => x.PromotesTo, f => f.PickRandom<PieceType>());
    }
}
