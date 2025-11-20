using AnarchyChess.Api.GameSnapshot.Models;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class IntermediateSquarePathFaker : RecordFaker<IntermediateSquarePath>
{
    public IntermediateSquarePathFaker()
    {
        StrictMode(true);
        RuleFor(x => x.PosIdx, f => (byte)f.Random.Number(0, 99));
        RuleFor(x => x.IsCapture, f => f.Random.Bool());
    }
}
