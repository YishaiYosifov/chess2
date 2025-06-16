using Chess2.Api.Game;
using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure.Factories;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.GameLogicTests;

public class ChessBoardTests : BaseUnitTest
{
    [Fact]
    public void Constructor_initializes_board_correctly()
    {
        var expectedPt = new Point(2, 3);
        var expectedPiece = PieceFactory.Black();
        var outOfBoundsPoint = new Point(2123, 3123);
        var pieces = new Dictionary<Point, Piece>
        {
            [expectedPt] = expectedPiece,
            [outOfBoundsPoint] = PieceFactory.White(),
        };

        var board = new ChessBoard(pieces);

        var squares = board.EnumerateSquares();
        squares.Should().HaveCount(GameConstants.BoardWidth * GameConstants.BoardHeight);
        foreach (var (point, piece) in board.EnumerateSquares())
        {
            if (point != expectedPt)
            {
                piece.Should().BeNull();
                continue;
            }

            piece.Should().NotBeNull().And.BeEquivalentTo(expectedPiece);
        }
    }

    [Fact]
    public void TryGetPieceAt_returns_false_when_the_piece_is_not_found()
    {
        var board = new ChessBoard();
        board.PlacePiece(new Point(3, 5), PieceFactory.White());

        var result = board.TryGetPieceAt(new Point(0, 0), out var resultPiece);

        result.Should().BeFalse();
        resultPiece.Should().BeNull();
    }

    [Fact]
    public void TryGetPieceAt_returns_true_and_the_piece_when_it_is_found()
    {
        var pt = new Point(1, 5);
        var piece = PieceFactory.Black();
        var board = new ChessBoard();
        board.PlacePiece(pt, piece);

        var result = board.TryGetPieceAt(pt, out var resultPiece);

        result.Should().BeTrue();
        resultPiece.Should().NotBeNull();
        resultPiece.Should().BeEquivalentTo(piece);
    }

    [Fact]
    public void PeekPieceAt_returns_the_piece_if_it_exists()
    {
        var pt = new Point(4, 1);
        var piece = PieceFactory.White();
        var board = new ChessBoard();
        board.PlacePiece(pt, piece);

        var result = board.PeekPieceAt(pt);

        result.Should().Be(piece);
    }

    [Fact]
    public void PeekPieceAt_returns_null_if_it_doesnt_exist()
    {
        var board = new ChessBoard();

        var result = board.PeekPieceAt(new Point(1, 2));

        result.Should().BeNull();
    }

    [Fact]
    public void IsEmpty_returns_true_when_the_square_is_empty()
    {
        var board = new ChessBoard();
        board.PlacePiece(new Point(5, 7), PieceFactory.Black());
        board.IsEmpty(new Point(4, 3)).Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_returns_false_when_the_square_is_not_empty()
    {
        var pt = new Point(4, 2);
        var board = new ChessBoard();
        board.PlacePiece(pt, PieceFactory.White());

        board.IsEmpty(pt).Should().BeFalse();
    }

    [Fact]
    public void PlayMove_with_a_regular_moves_correctly_moves_the_piece()
    {
        var board = new ChessBoard();
        var piece = PieceFactory.White();
        board.PlacePiece(new Point(4, 1), piece); // e2
        var move = new Move(
            From: new Point(4, 1), // e2
            To: new Point(4, 3), // e4
            Piece: piece
        );

        var expectedBoard = board.EnumerateSquares().ToDictionary();
        expectedBoard[move.From] = null;
        expectedBoard[move.To] = piece with { TimesMoved = 1 };

        board.PlayMove(move);

        board.EnumerateSquares().ToDictionary().Should().BeEquivalentTo(expectedBoard);
    }

    [Fact]
    public void PlayMove_with_a_capture_removes_captured_pieces_and_moves_piece()
    {
        var board = new ChessBoard();
        var pieceToMove = PieceFactory.White(PieceType.Pawn);
        var pieceToCapture = PieceFactory.Black(PieceType.Rook);
        board.PlacePiece(new Point(4, 1), pieceToMove); // e2
        board.PlacePiece(new Point(4, 4), pieceToCapture); // e5

        var move = new Move(
            From: new Point(4, 1), // e2
            To: new Point(4, 3), // e4
            Piece: pieceToMove,
            CapturedSquares: [new Point(4, 4)]
        );

        var expectedBoard = board.EnumerateSquares().ToDictionary();
        expectedBoard[move.From] = null;
        expectedBoard[move.To] = pieceToMove with { TimesMoved = 1 };
        expectedBoard[new Point(4, 4)] = null;

        board.PlayMove(move);

        board.EnumerateSquares().ToDictionary().Should().BeEquivalentTo(expectedBoard);
    }

    [Fact]
    public void PlayMove_throws_if_a_side_effect_is_invalid()
    {
        var board = new ChessBoard();
        var mainPiece = PieceFactory.White(PieceType.Pawn);
        var sideEffectPiece1 = PieceFactory.White(PieceType.Bishop);
        var sideEffectPiece2 = PieceFactory.Black(PieceType.Rook);

        board.PlacePiece(new Point(4, 1), mainPiece); // e2
        board.PlacePiece(new Point(0, 0), sideEffectPiece1); // a1
        board.PlacePiece(new Point(1, 0), sideEffectPiece2); // b1

        var sideEffect1 = new Move(
            From: new Point(0, 0), // a1
            To: new Point(0, 1), // a2
            Piece: sideEffectPiece1
        );

        var sideEffect2 = new Move(
            From: new Point(1, 0), // b1
            To: new Point(10, 10), // Invalid (out of bounds)
            Piece: sideEffectPiece2
        );

        var mainMove = new Move(
            From: new Point(4, 1), // e2
            To: new Point(4, 2), // e3
            Piece: mainPiece,
            SideEffects: [sideEffect1, sideEffect2]
        );

        var expectedBoard = board.EnumerateSquares().ToDictionary();

        var act = () => board.PlayMove(mainMove);

        // Should fail due to sideEffect2 invalid move
        act.Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithMessage("Move is out of board boundaries*")
            .WithParameterName("move");
        board.EnumerateSquares().ToDictionary().Should().BeEquivalentTo(expectedBoard);
    }

    [Fact]
    public void PlayMove_with_chained_side_effects_executes_all_successfully()
    {
        var board = new ChessBoard();
        var mainPiece = PieceFactory.White(PieceType.Pawn);
        var sideEffect1 = PieceFactory.White(PieceType.Queen);
        var sideEffect2 = PieceFactory.Black(PieceType.Horsey);

        board.PlacePiece(new Point(4, 1), mainPiece); // e2
        board.PlacePiece(new Point(0, 0), sideEffect1); // a1
        board.PlacePiece(new Point(0, 1), sideEffect2); // a2

        var chainedSideEffect = new Move(
            From: new Point(0, 1), // a2
            To: new Point(0, 2), // a3
            Piece: sideEffect2
        );

        var sideEffectMove = new Move(
            From: new Point(0, 0), // a1
            To: new Point(0, 1), // a2
            Piece: sideEffect1,
            SideEffects: [chainedSideEffect]
        );

        var mainMove = new Move(
            From: new Point(4, 1), // e2
            To: new Point(4, 2), // e3
            Piece: mainPiece,
            SideEffects: [sideEffectMove]
        );

        var expectedBoard = board.EnumerateSquares().ToDictionary();
        expectedBoard[chainedSideEffect.From] = null;
        expectedBoard[chainedSideEffect.To] = sideEffect2 with { TimesMoved = 1 };
        expectedBoard[sideEffectMove.From] = null;
        expectedBoard[sideEffectMove.To] = sideEffect1 with { TimesMoved = 1 };
        expectedBoard[mainMove.From] = null;
        expectedBoard[mainMove.To] = mainPiece with { TimesMoved = 1 };

        board.PlayMove(mainMove);

        board.EnumerateSquares().ToDictionary().Should().BeEquivalentTo(expectedBoard);
    }

    [Fact]
    public void PlayMove_adds_move_to_move_history_when_successful()
    {
        var board = new ChessBoard();
        var piece = PieceFactory.White(PieceType.Pawn);
        var move = new Move(
            From: new Point(4, 1), // e2
            To: new Point(4, 3), // e4
            Piece: piece
        );
        board.PlacePiece(move.From, piece);

        board.PlayMove(move);

        board.Moves.Should().ContainSingle().Which.Should().BeEquivalentTo(move);
    }

    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(9, 9, true)]
    [InlineData(-1, 5, false)]
    [InlineData(5, 10, false)]
    public void IsWithinBoundaries_checks_boundaries_correctly(int x, int y, bool expected)
    {
        var board = new ChessBoard();
        var point = new Point(x, y);

        board.IsWithinBoundaries(point).Should().Be(expected);
    }

    [Fact]
    public void EnumerateSquares_returns_all_squares()
    {
        var board = new ChessBoard();

        var squares = board.EnumerateSquares();

        squares.Should().HaveCount(GameConstants.BoardWidth * GameConstants.BoardHeight);
    }
}
