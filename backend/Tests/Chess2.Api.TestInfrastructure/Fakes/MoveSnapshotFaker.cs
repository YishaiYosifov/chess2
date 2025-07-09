using Chess2.Api.Game.Models;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class MoveSnapshotFaker : RecordFaker<MoveSnapshot>
{
    private readonly string[] _encodedMoves =
    [
        "e2e4",
        "g1f3",
        "e1g1-h1f1",
        "b8c6",
        "e7e8q",
        "e1c1-a1d1!d7",
        "d2d4",
        "f1c4",
        "a7a8r",
        "e5d6!c7",
    ];

    private readonly string[] _sanMoves =
    [
        "e4",
        "Nf3",
        "O-O",
        "Nc6",
        "e8=Q",
        "O-O-O",
        "d4",
        "Bc4",
        "a8=R",
        "exd6",
    ];

    public MoveSnapshotFaker()
    {
        StrictMode(true);
        RuleFor(x => x.EncodedMove, f => f.PickRandom(_encodedMoves));
        RuleFor(x => x.San, f => f.PickRandom(_sanMoves));
        RuleFor(x => x.TimeLeft, f => f.Random.Double(1000, 10000));
    }
}
