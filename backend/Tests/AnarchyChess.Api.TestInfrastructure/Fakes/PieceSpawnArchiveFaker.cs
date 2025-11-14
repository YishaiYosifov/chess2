using Bogus;
using AnarchyChess.Api.ArchivedGames.Entities;
using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class PieceSpawnArchiveFaker : Faker<PieceSpawnArchive>
{
    public PieceSpawnArchiveFaker()
    {
        StrictMode(true);
        RuleFor(x => x.Id, 0);
        RuleFor(x => x.Type, f => f.PickRandom<PieceType>());
        RuleFor(x => x.Color, f => f.PickRandom<GameColor>());
        RuleFor(x => x.PosIdx, f => (byte)f.Random.Number(0, 99));
    }
}
