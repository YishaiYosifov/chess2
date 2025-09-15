using Bogus;
using Chess2.Api.GameLogic.Extensions;
using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class MoveFaker : RecordFaker<Move>
{
    public MoveFaker(GameColor? forColor = null, PieceType? pieceType = null)
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
                    color: forColor ?? (f.IndexFaker % 2 == 0 ? GameColor.White : GameColor.Black),
                    piece: pieceType
                ).Generate()
        );

        RuleFor(x => x.TriggerSquares, []);
        RuleFor(x => x.Captures, []);
        RuleFor(x => x.SideEffects, []);
        RuleFor(x => x.SpecialMoveType, SpecialMoveType.None);
        RuleFor(x => x.ForcedPriority, ForcedMovePriority.None);
        RuleFor(x => x.PromotesTo, (PieceType?)null);
    }

    public static Faker<Move> Capture(
        GameColor forColor,
        PieceType? captureType = null,
        PieceType? pieceType = null
    ) =>
        new MoveFaker(forColor, pieceType).RuleFor(
            x => x.Captures,
            f =>
                [
                    new MoveCapture(
                        new PieceFaker(forColor.Invert(), captureType).Generate(),
                        new AlgebraicPoint(X: f.Random.Number(0, 9), Y: f.Random.Number(0, 9))
                    ),
                ]
        );
}
