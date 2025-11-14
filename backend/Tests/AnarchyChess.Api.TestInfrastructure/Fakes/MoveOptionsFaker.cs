using AnarchyChess.Api.GameSnapshot.Models;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class MoveOptionsFaker : RecordFaker<MoveOptions>
{
    public MoveOptionsFaker()
    {
        StrictMode(true);
        RuleFor(
            x => x.LegalMoves,
            f =>
            {
                var count = f.Random.Number(1, 10);
                return new MovePathFaker().Generate(count);
            }
        );
        RuleFor(x => x.HasForcedMoves, f => f.Random.Bool());
    }
}
