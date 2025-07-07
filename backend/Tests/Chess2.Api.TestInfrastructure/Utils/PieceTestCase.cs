using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.TestInfrastructure.Utils;

public class PieceTestCase
{
    public Piece Piece { get; }
    public AlgebraicPoint Origin { get; }

    public List<Move> ExpectedMoves { get; } = [];
    public List<(AlgebraicPoint Position, Piece Piece)> BlockedBy { get; } = [];
    public List<Move> PriorMoves { get; } = [];

    public string TestDecription { get; private set; } = "";

    private PieceTestCase(AlgebraicPoint from, Piece piece)
    {
        Piece = piece;
        Origin = from;
    }

    public static PieceTestCase From(string from, Piece piece) =>
        new(new AlgebraicPoint(from), piece);

    public PieceTestCase GoesTo(
        string to,
        IEnumerable<string>? through = null,
        IEnumerable<string>? captures = null,
        IEnumerable<Move>? sideEffects = null
    )
    {
        ExpectedMoves.Add(
            new Move(
                Origin,
                new AlgebraicPoint(to),
                Piece,
                Through: through?.Select(x => new AlgebraicPoint(x)),
                CapturedSquares: captures?.Select(x => new AlgebraicPoint(x)),
                SideEffects: sideEffects
            )
        );
        return this;
    }

    public PieceTestCase WithBlocker(string position, Piece piece)
    {
        BlockedBy.Add((new AlgebraicPoint(position), piece));
        return this;
    }

    public PieceTestCase WithPriorMove(Move move)
    {
        PriorMoves.Add(move);
        return this;
    }

    public PieceTestCase WithDescription(string testDescription)
    {
        TestDecription = testDescription;
        return this;
    }

    public override string ToString() =>
        string.IsNullOrWhiteSpace(TestDecription)
            ? $"Piece under test at {Origin}"
            : TestDecription;
}
