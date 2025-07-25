using Chess2.Api.GameSnapshot.Models;

namespace Chess2.Api.TestInfrastructure.Fakes;

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
