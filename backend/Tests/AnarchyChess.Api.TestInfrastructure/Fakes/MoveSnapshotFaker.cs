using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.TestInfrastructure.TestData;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class MoveSnapshotFaker : RecordFaker<MoveSnapshot>
{
    public MoveSnapshotFaker()
    {
        StrictMode(true);
        RuleFor(x => x.Path, f => new MovePathFaker().Generate());
        RuleFor(x => x.San, f => f.PickRandom(MoveData.SanMoves));
        RuleFor(x => x.TimeLeft, f => f.Random.Double(1000, 10000));
    }
}
