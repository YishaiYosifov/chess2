using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.TestInfrastructure.Factories;
using FluentAssertions;

namespace AnarchyChess.Api.Unit.Tests.GameLogicTests;

public class ChessBoardTests
{
    [Fact]
    public void Constructor_initializes_board_correctly()
    {
        AlgebraicPoint expectedPt = new("c4");
        Piece expectedPiece = PieceFactory.Black();
        AlgebraicPoint outOfBoundsPoint = new(2123, 3123);
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [expectedPt] = expectedPiece,
            [outOfBoundsPoint] = PieceFactory.White(),
        };

        ChessBoard board = new(pieces);

        var squares = board.EnumerateSquares();
        squares.Should().HaveCount(GameLogicConstants.BoardWidth * GameLogicConstants.BoardHeight);
        foreach ((AlgebraicPoint point, Piece? piece) in board.EnumerateSquares())
        {
            if (point != expectedPt)
            {
                piece.Should().BeNull();
                continue;
            }

            piece.Should().NotBeNull().And.Be(expectedPiece);
        }
    }

    [Fact]
    public void TryGetPieceAt_returns_false_when_the_piece_is_not_found()
    {
        ChessBoard board = new();
        board.PlacePiece(new AlgebraicPoint("e6"), PieceFactory.White());

        bool result = board.TryGetPieceAt(new AlgebraicPoint("a1"), out Piece? resultPiece);

        result.Should().BeFalse();
        resultPiece.Should().BeNull();
    }

    [Fact]
    public void TryGetPieceAt_returns_true_and_the_piece_when_it_is_found()
    {
        AlgebraicPoint pt = new("b6");
        Piece piece = PieceFactory.Black();
        ChessBoard board = new();
        board.PlacePiece(pt, piece);

        bool result = board.TryGetPieceAt(pt, out Piece? resultPiece);

        result.Should().BeTrue();
        resultPiece.Should().NotBeNull();
        resultPiece.Should().Be(piece);
    }

    [Fact]
    public void PeekPieceAt_returns_the_piece_if_it_exists()
    {
        AlgebraicPoint pt = new("e2");
        Piece piece = PieceFactory.White();
        ChessBoard board = new();
        board.PlacePiece(pt, piece);

        Piece? result = board.PeekPieceAt(pt);

        result.Should().Be(piece);
    }

    [Fact]
    public void PeekPieceAt_returns_null_if_it_doesnt_exist()
    {
        ChessBoard board = new();

        Piece? result = board.PeekPieceAt(new AlgebraicPoint("b3"));

        result.Should().BeNull();
    }

    [Fact]
    public void IsEmpty_returns_true_when_the_square_is_empty()
    {
        ChessBoard board = new();
        board.PlacePiece(new AlgebraicPoint("f8"), PieceFactory.Black());
        board.IsEmpty(new AlgebraicPoint("e4")).Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_returns_false_when_the_square_is_not_empty()
    {
        AlgebraicPoint pt = new("e3");
        ChessBoard board = new();
        board.PlacePiece(pt, PieceFactory.White());

        board.IsEmpty(pt).Should().BeFalse();
    }

    [Fact]
    public void GetAllPiecesWith_returns_empty_list_when_no_pieces_exist()
    {
        ChessBoard board = new();

        var pieces = board.GetAllPiecesWith(PieceType.Horsey, GameColor.White);

        pieces.Should().BeEmpty();
    }

    [Fact]
    public void GetAllPiecesWith_returns_correct_pieces_by_type_and_color()
    {
        ChessBoard board = new();
        var whitePawn1 = PieceFactory.White(PieceType.Pawn);
        var whitePawn2 = PieceFactory.White(PieceType.Pawn);
        var blackPawn = PieceFactory.Black(PieceType.Pawn);
        var whiteKnight = PieceFactory.White(PieceType.Horsey);

        board.PlacePiece(new("a2"), whitePawn1);
        board.PlacePiece(new("b2"), whitePawn2);
        board.PlacePiece(new("c2"), blackPawn);
        board.PlacePiece(new("d2"), whiteKnight);

        var whitePawns = board.GetAllPiecesWith(PieceType.Pawn, GameColor.White);
        var blackPawns = board.GetAllPiecesWith(PieceType.Pawn, GameColor.Black);
        var whiteKnights = board.GetAllPiecesWith(PieceType.Horsey, GameColor.White);

        whitePawns.Should().BeEquivalentTo([whitePawn1, whitePawn2]);
        blackPawns.Should().ContainSingle().Which.Should().Be(blackPawn);
        whiteKnights.Should().ContainSingle().Which.Should().Be(whiteKnight);
    }

    [Fact]
    public void PlayMove_with_a_regular_moves_correctly_moves_the_piece()
    {
        ChessBoard board = new();
        Piece piece = PieceFactory.White();
        AlgebraicPoint from = new("e2");
        AlgebraicPoint to = new("e4");
        board.PlacePiece(from, piece);
        Move move = new(from, to, piece);

        Dictionary<AlgebraicPoint, Piece?> expectedBoard = board.EnumerateSquares().ToDictionary();
        expectedBoard[move.From] = null;
        expectedBoard[move.To] = piece with { TimesMoved = piece.TimesMoved + 1 };

        board.PlayMove(move);

        board.EnumerateSquares().ToDictionary().Should().BeEquivalentTo(expectedBoard);
    }

    [Fact]
    public void PlayMove_with_a_capture_removes_captured_pieces_and_moves_piece()
    {
        ChessBoard board = new();
        Piece pieceToMove = PieceFactory.White(PieceType.Pawn);
        Piece pieceToCapture = PieceFactory.Black(PieceType.Rook);
        board.PlacePiece(new AlgebraicPoint("e2"), pieceToMove);
        board.PlacePiece(new AlgebraicPoint("e5"), pieceToCapture);

        Move move = new(
            from: new AlgebraicPoint("e2"),
            to: new AlgebraicPoint("e4"),
            piece: pieceToMove,
            captures: [new MoveCapture(pieceToCapture, new AlgebraicPoint("e5"))]
        );

        Dictionary<AlgebraicPoint, Piece?> expectedBoard = board.EnumerateSquares().ToDictionary();
        expectedBoard[move.From] = null;
        expectedBoard[move.To] = pieceToMove with { TimesMoved = pieceToMove.TimesMoved + 1 };
        expectedBoard[new AlgebraicPoint("e5")] = null;

        board.PlayMove(move);

        board.EnumerateSquares().ToDictionary().Should().BeEquivalentTo(expectedBoard);
    }

    [Fact]
    public void PlayMove_throws_if_a_side_effect_is_invalid()
    {
        ChessBoard board = new();
        Piece mainPiece = PieceFactory.White(PieceType.Pawn);
        Piece sideEffectPiece1 = PieceFactory.White(PieceType.Bishop);
        Piece sideEffectPiece2 = PieceFactory.Black(PieceType.Rook);

        board.PlacePiece(new AlgebraicPoint("e2"), mainPiece);
        board.PlacePiece(new AlgebraicPoint("a1"), sideEffectPiece1);
        board.PlacePiece(new AlgebraicPoint("b1"), sideEffectPiece2);

        MoveSideEffect sideEffect1 = new(
            From: new AlgebraicPoint("a1"),
            To: new AlgebraicPoint("a2"),
            Piece: sideEffectPiece1
        );

        MoveSideEffect sideEffect2 = new(
            From: new AlgebraicPoint("b1"),
            To: new AlgebraicPoint(15, 15), // Invalid (out of bounds)
            Piece: sideEffectPiece2
        );

        Move mainMove = new(
            from: new AlgebraicPoint("e2"),
            to: new AlgebraicPoint("e3"),
            piece: mainPiece,
            sideEffects: [sideEffect1, sideEffect2]
        );

        Dictionary<AlgebraicPoint, Piece?> expectedBoard = board.EnumerateSquares().ToDictionary();

        Action act = () => board.PlayMove(mainMove);

        // Should fail due to sideEffect2 invalid move
        act.Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithMessage("Move is out of board boundaries*")
            .WithParameterName("move");

        board.EnumerateSquares().ToDictionary().Should().BeEquivalentTo(expectedBoard);
    }

    [Fact]
    public void PlayMove_adds_move_to_move_history_when_successful()
    {
        ChessBoard board = new();
        Piece piece = PieceFactory.White(PieceType.Pawn);
        Move move = new(from: new AlgebraicPoint("e2"), to: new AlgebraicPoint("e4"), piece: piece);
        board.PlacePiece(move.From, piece);

        board.PlayMove(move);

        board.Moves.Should().ContainSingle().Which.Should().BeEquivalentTo(move);
    }

    [Fact]
    public void PlayMove_with_promotion_replaces_piece_type_and_reset_times_moved()
    {
        ChessBoard board = new();
        Piece pawn = PieceFactory.White(PieceType.Pawn);
        AlgebraicPoint from = new("e7");
        AlgebraicPoint to = new("e8");
        Move move = new(from, to, pawn, promotesTo: PieceType.Queen);

        board.PlacePiece(from, pawn);

        board.PlayMove(move);

        board.PeekPieceAt(from).Should().BeNull();
        var promotedPiece = board.PeekPieceAt(to);
        promotedPiece.Should().Be(new Piece(PieceType.Queen, pawn.Color, TimesMoved: 0));
    }

    [Fact]
    public void PlayMove_with_piece_spawns_places_all_pieces()
    {
        ChessBoard board = new();
        PieceSpawn[] spawns =
        [
            new(PieceType.Pawn, GameColor.White, new AlgebraicPoint("b2")),
            new(PieceType.Bishop, GameColor.Black, new AlgebraicPoint("c3")),
            new(PieceType.Horsey, GameColor.White, new AlgebraicPoint("f6")),
        ];

        Move move = new(
            from: new AlgebraicPoint("a1"),
            to: new AlgebraicPoint("a2"),
            piece: PieceFactory.White(),
            pieceSpawns: spawns
        );
        board.PlacePiece(new("a1"), PieceFactory.White());

        board.PlayMove(move);

        foreach (var spawn in spawns)
        {
            var piece = board.PeekPieceAt(spawn.Position);
            piece.Should().NotBeNull();
            piece.Type.Should().Be(spawn.Type);
            piece.Color.Should().Be(spawn.Color);
        }
    }

    [Fact]
    public void PlayMove_with_two_pieces_swapping_squares_moves_both_correctly()
    {
        ChessBoard board = new();

        Piece piece1 = PieceFactory.White(PieceType.Rook);
        Piece piece2 = PieceFactory.Black(PieceType.Horsey);
        AlgebraicPoint pos1 = new("a1");
        AlgebraicPoint pos2 = new("b1");

        board.PlacePiece(pos1, piece1);
        board.PlacePiece(pos2, piece2);

        // piece1 moves to piece2 position, piece2 moves to piece1 position
        MoveSideEffect sideEffect = new(From: pos2, To: pos1, Piece: piece2);
        Move move = new(from: pos1, to: pos2, piece: piece1, sideEffects: [sideEffect]);

        board.PlayMove(move);

        board.PeekPieceAt(pos1).Should().Be(piece2 with { TimesMoved = piece2.TimesMoved + 1 });
        board.PeekPieceAt(pos2).Should().Be(piece1 with { TimesMoved = piece1.TimesMoved + 1 });
    }

    [Fact]
    public void PlacePiece_adds_piece()
    {
        ChessBoard board = new();
        var piece = PieceFactory.White();
        AlgebraicPoint pt = new("a1");

        board.PlacePiece(pt, piece);

        var pieces = board.GetAllPiecesWith(piece.Type, GameColor.White);
        pieces.Should().ContainSingle().Which.Should().Be(piece);
    }

    [Fact]
    public void RemovePiece_removes_piece()
    {
        ChessBoard board = new();
        var piece = PieceFactory.Black();
        AlgebraicPoint pt = new("d5");

        board.PlacePiece(pt, piece);
        board.RemovePiece(pt);

        var pieces = board.GetAllPiecesWith(piece.Type, GameColor.Black);
        pieces.Should().BeEmpty();
        board.PeekPieceAt(pt).Should().BeNull();
    }

    [Fact]
    public void ModifyPiece_modifies_piece_by_action()
    {
        ChessBoard board = new();
        var pawn = PieceFactory.White(PieceType.Pawn);
        AlgebraicPoint pt = new("e2");

        board.PlacePiece(pt, pawn);
        board.ModifyPiece(pt, piece => piece with { Type = PieceType.Queen });

        var pawns = board.GetAllPiecesWith(PieceType.Pawn, GameColor.White);
        pawns.Should().BeEmpty();

        var queens = board.GetAllPiecesWith(PieceType.Queen, GameColor.White);
        queens.Should().ContainSingle().Which.Type.Should().Be(PieceType.Queen);
    }

    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(9, 9, true)]
    [InlineData(-1, 5, false)]
    [InlineData(5, 10, false)]
    public void IsWithinBoundaries_checks_boundaries_correctly(int x, int y, bool expected)
    {
        ChessBoard board = new();
        AlgebraicPoint point = new(x, y);

        board.IsWithinBoundaries(point).Should().Be(expected);
    }

    [Fact]
    public void EnumerateSquares_returns_all_squares()
    {
        ChessBoard board = new();

        var squares = board.EnumerateSquares();

        squares.Should().HaveCount(GameLogicConstants.BoardWidth * GameLogicConstants.BoardHeight);
    }

    [Fact]
    public void EnumeratePieces_returns_all_pieces_correctly()
    {
        ChessBoard board = new();
        Dictionary<AlgebraicPoint, Piece> piecesToPlace = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.Rook),
            [new("b2")] = PieceFactory.Black(PieceType.Pawn),
            [new("c3")] = PieceFactory.White(PieceType.Horsey),
        };
        foreach (var (pt, piece) in piecesToPlace)
            board.PlacePiece(pt, piece);

        var enumerated = board.EnumeratePieces().ToDictionary();

        enumerated.Should().BeEquivalentTo(piecesToPlace);
    }
}
