using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.TestInfrastructure.TestData;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class MoveSnapshotFaker : RecordFaker<MoveSnapshot>
{
    public MoveSnapshotFaker()
    {
        StrictMode(true);
        RuleFor(x => x.EncodedMove, f => f.PickRandom(MoveData.EncodedMoves));
        RuleFor(x => x.San, f => f.PickRandom(MoveData.SanMoves));
        RuleFor(x => x.TimeLeft, f => f.Random.Double(1000, 10000));
    }
}
