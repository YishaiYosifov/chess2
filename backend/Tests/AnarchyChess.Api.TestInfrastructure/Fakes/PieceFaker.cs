using Bogus;
using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class PieceFaker : RecordFaker<Piece>
{
    public PieceFaker(GameColor? color, PieceType? piece = null, int? timesMoved = null)
    {
        UseSeed(Faker.GlobalUniqueIndex++);

        StrictMode(true);
        RuleFor(x => x.Type, f => piece ?? f.PickRandom<PieceType>());
        RuleFor(x => x.Color, color);
        RuleFor(x => x.TimesMoved, f => timesMoved ?? f.Random.Number(0, 10));
    }
}
