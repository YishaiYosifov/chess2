using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.TestInfrastructure.Utils;

public class PieceTestCase
{
    public Piece Piece { get; }
    public AlgebraicPoint Origin { get; }
    public GameColor MovingPlayer { get; private set; }

    public List<Move> ExpectedMoves { get; } = [];
    public List<(AlgebraicPoint Position, Piece Piece)> BlockedBy { get; } = [];
    public List<Move> PriorMoves { get; } = [];

    public string TestDecription { get; private set; } = "";

    private PieceTestCase(AlgebraicPoint from, Piece piece)
    {
        Piece = piece;
        Origin = from;
        MovingPlayer = piece.Color ?? GameColor.White;
    }

    public static PieceTestCase From(string from, Piece piece) =>
        new(new AlgebraicPoint(from), piece);

    public PieceTestCase GoesTo(
        string to,
        IEnumerable<string>? trigger = null,
        IEnumerable<string>? captures = null,
        IEnumerable<MoveSideEffect>? sideEffects = null,
        SpecialMoveType specialMoveType = SpecialMoveType.None,
        ForcedMovePriority forcedPriority = ForcedMovePriority.None,
        PieceType? promotesTo = null
    )
    {
        ExpectedMoves.Add(
            new Move(
                Origin,
                new AlgebraicPoint(to),
                Piece,
                triggerSquares: trigger?.Select(x => new AlgebraicPoint(x)),
                capturedSquares: captures?.Select(x => new AlgebraicPoint(x)),
                sideEffects: sideEffects,
                specialMoveType: specialMoveType,
                forcedPriority: forcedPriority,
                promotesTo: promotesTo
            )
        );
        return this;
    }

    public PieceTestCase GoesTo(params string[] to)
    {
        foreach (var position in to)
        {
            GoesTo(position);
        }
        return this;
    }

    public PieceTestCase WithPieceAt(string position, Piece piece)
    {
        BlockedBy.Add((new AlgebraicPoint(position), piece));
        return this;
    }

    public PieceTestCase WithPriorMove(Move move)
    {
        PriorMoves.Add(move);
        return this;
    }

    public PieceTestCase WithMovingPlayer(GameColor playerColor)
    {
        MovingPlayer = playerColor;
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
