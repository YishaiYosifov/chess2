using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class MoveFaker : RecordFaker<Move>
{
    public MoveFaker()
    {
        RuleFor(
            x => x.From,
            f => new AlgebraicPoint(X: f.Random.Number(0, 9), Y: f.Random.Number(0, 9))
        );
        RuleFor(
            x => x.To,
            f => new AlgebraicPoint(X: f.Random.Number(0, 9), Y: f.Random.Number(0, 9))
        );
        RuleFor(
            x => x.Piece,
            f =>
                new PieceFaker(
                    color: f.IndexFaker % 2 == 0 ? GameColor.White : GameColor.Black
                ).Generate()
        );
    }
}
