using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class PieceFaker : RecordFaker<Piece>
{
    public PieceFaker()
    {
        StrictMode(true)
            .RuleFor(x => x.Type, f => f.Random.Enum<PieceType>())
            .RuleFor(x => x.Color, f => f.Random.Enum<Color>())
            .RuleFor(x => x.TimesMoved, 0);
    }
}
