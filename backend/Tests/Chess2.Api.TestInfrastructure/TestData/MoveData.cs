using Bogus;

namespace Chess2.Api.TestInfrastructure.TestData;

public static class MoveData
{
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

    public static byte[] RandomIdxs(Faker faker)
    {
        var length = faker.Random.Number(0, 5);
        byte[] idxs = new byte[length];
        for (int i = 0; i < length; i++)
        {
            idxs[i] = (byte)faker.Random.Number(0, 99);
        }
        return idxs;
    }
}
