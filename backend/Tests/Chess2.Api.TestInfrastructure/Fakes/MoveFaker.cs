using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class MoveFaker : RecordFaker<Move>
{
    public MoveFaker()
    {
        StrictMode(true);

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

        RuleFor(x => x.TriggerSquares, []);
        RuleFor(x => x.Captures, []);
        RuleFor(x => x.SideEffects, []);
        RuleFor(x => x.SpecialMoveType, SpecialMoveType.None);
        RuleFor(x => x.ForcedPriority, ForcedMovePriority.None);
        RuleFor(x => x.PromotesTo, (PieceType?)null);
    }
}
