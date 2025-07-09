namespace Chess2.Api.TestInfrastructure.TestData;

public static class MoveData
{
    public static readonly string[] EncodedMoves =
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

    public static readonly string[] SanMoves =
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
}
