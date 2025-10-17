using System.Text.Json.Serialization;
using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Extensions;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure.Factories;
using Chess2.Api.TestInfrastructure.Fakes;

namespace Chess2.Api.TestInfrastructure.Utils;

public class PieceTestCase
{
    public Piece Piece { get; }
    public AlgebraicPoint Origin { get; }
    public GameColor MovingPlayer { get; private set; }

    public List<Move> ExpectedMoves { get; } = [];
    public List<Move> PriorMoves { get; } = [];

    [JsonIgnore]
    public Dictionary<AlgebraicPoint, Piece> BlockedBy { get; } = [];

    [JsonInclude]
    [JsonPropertyName(nameof(BlockedBy))]
    public Dictionary<string, Piece> BlockedBySurrogate
    {
        get => BlockedBy.ToDictionary(x => x.Key.AsAlgebraic(), x => x.Value);
        set
        {
            BlockedBy.Clear();
            foreach (var kvp in value)
                BlockedBy[new AlgebraicPoint(kvp.Key)] = kvp.Value;
        }
    }

    public string TestDecription { get; private set; } = "";

    private readonly ChessBoard _board;

    private PieceTestCase(AlgebraicPoint from, Piece piece)
    {
        Piece = piece;
        Origin = from;
        MovingPlayer = piece.Color ?? GameColor.White;

        _board = new();
        _board.PlacePiece(from, piece);
    }

    public static PieceTestCase From(string from, Piece piece) =>
        new(new AlgebraicPoint(from), piece);

    public PieceTestCase GoesTo(
        string to,
        IEnumerable<string>? trigger = null,
        IEnumerable<string>? captures = null,
        IEnumerable<string>? intermediates = null,
        IEnumerable<MoveSideEffect>? sideEffects = null,
        IEnumerable<PieceSpawn>? spawns = null,
        SpecialMoveType specialMoveType = SpecialMoveType.None,
        ForcedMovePriority forcedPriority = ForcedMovePriority.None,
        PieceType? promotesTo = null
    )
    {
        ExpectedMoves.Add(
            BuildMove(
                Origin.AsAlgebraic(),
                to,
                trigger,
                captures,
                intermediates,
                sideEffects,
                spawns,
                specialMoveType,
                forcedPriority,
                promotesTo
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
        AlgebraicPoint point = new(position);
        BlockedBy.Add(point, piece);
        _board.PlacePiece(point, piece);
        return this;
    }

    public PieceTestCase WithWhitePieceAt(string position, PieceType? pieceType = null) =>
        WithPieceAt(position, PieceFactory.White(pieceType));

    public PieceTestCase WithBlackPieceAt(string position, PieceType? pieceType = null) =>
        WithPieceAt(position, PieceFactory.Black(pieceType));

    public PieceTestCase WithFriendlyPieceAt(string position, params PieceType[] excludePieces) =>
        WithPieceAt(
            position,
            new PieceFaker(color: Piece.Color)
                .RuleFor(x => x.Type, f => f.PickRandomWithout(excludePieces))
                .Generate()
        );

    public PieceTestCase WithEnemyPieceAt(string position, params PieceType[] excludePieces) =>
        WithPieceAt(
            position,
            new PieceFaker(color: Piece.Color?.Invert())
                .RuleFor(x => x.Type, f => f.PickRandomWithout(excludePieces))
                .Generate()
        );

    public PieceTestCase WithPriorMove(
        string from,
        string to,
        IEnumerable<string>? trigger = null,
        IEnumerable<string>? captures = null,
        IEnumerable<string>? intermediates = null,
        IEnumerable<MoveSideEffect>? sideEffects = null,
        IEnumerable<PieceSpawn>? spawns = null,
        SpecialMoveType specialMoveType = SpecialMoveType.None,
        ForcedMovePriority forcedPriority = ForcedMovePriority.None,
        PieceType? promotesTo = null
    )
    {
        var move = BuildMove(
            from,
            to,
            trigger,
            captures,
            intermediates,
            sideEffects,
            spawns,
            specialMoveType,
            forcedPriority,
            promotesTo
        );
        PriorMoves.Add(move);
        _board.PlayMove(move);
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

    private Move BuildMove(
        string from,
        string to,
        IEnumerable<string>? trigger = null,
        IEnumerable<string>? captures = null,
        IEnumerable<string>? intermediates = null,
        IEnumerable<MoveSideEffect>? sideEffects = null,
        IEnumerable<PieceSpawn>? spawns = null,
        SpecialMoveType specialMoveType = SpecialMoveType.None,
        ForcedMovePriority forcedPriority = ForcedMovePriority.None,
        PieceType? promotesTo = null
    )
    {
        var moveCaptures = captures?.Select(c =>
        {
            AlgebraicPoint pos = new(c);
            return new MoveCapture(
                _board.PeekPieceAt(pos)
                    ?? throw new InvalidOperationException($"No Piece Found at {pos}"),
                pos
            );
        });
        AlgebraicPoint fromPoint = new(from);

        return new Move(
            fromPoint,
            new AlgebraicPoint(to),
            _board.PeekPieceAt(fromPoint)
                ?? throw new InvalidOperationException($"No Piece Found at {fromPoint}"),
            triggerSquares: trigger?.Select(x => new AlgebraicPoint(x)),
            intermediateSquares: intermediates?.Select(x => new AlgebraicPoint(x)),
            captures: moveCaptures,
            sideEffects: sideEffects,
            pieceSpawns: spawns,
            specialMoveType: specialMoveType,
            forcedPriority: forcedPriority,
            promotesTo: promotesTo
        );
    }
}
