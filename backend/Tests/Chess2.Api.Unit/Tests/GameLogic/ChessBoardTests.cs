using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Errors;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.GameLogic;

public class ChessBoardTests : BaseUnitTest
{
    [Fact]
    public void Constructor_initializes_board_correctly()
    {
        var expectedPt = new Point(2, 3);
        var expetedPiece = new PieceFaker().Generate();
        var outOfBoundsPoint = new Point(2123, 3123);
        var pieces = new Dictionary<Point, Piece>
        {
            [expectedPt] = expetedPiece,
            [outOfBoundsPoint] = new PieceFaker().Generate(),
        };

        var board = new ChessBoard(pieces);

        var squares = board.GetSquares().ToList();
        squares.Should().HaveCount(100); // 10 * 10
        foreach (var (point, piece) in board.GetSquares())
        {
            if (point != expectedPt)
            {
                piece.Should().BeNull();
                continue;
            }

            piece.Should().NotBeNull().And.BeEquivalentTo(expetedPiece);
        }
    }

    [Fact]
    public void TryGetPieceAt_returns_false_when_the_piece_is_not_found()
    {
        var board = new ChessBoard(new() { [new Point(1, 1)] = new PieceFaker().Generate() });

        var result = board.TryGetPieceAt(new Point(0, 0), out var resultPiece);

        result.Should().BeFalse();
        resultPiece.Should().BeNull();
    }

    [Fact]
    public void TryGetPieceAt_returns_true_and_the_piece_when_it_is_found()
    {
        var pt = new Point(1, 1);
        var piece = new PieceFaker().Generate();
        var board = new ChessBoard(new() { [pt] = piece });

        var result = board.TryGetPieceAt(pt, out var resultPiece);

        result.Should().BeTrue();
        resultPiece.Should().NotBeNull();
        resultPiece.Should().BeEquivalentTo(piece);
    }

    [Fact]
    public void PeekPieceAt_returns_the_piece_if_it_exists()
    {
        var pt = new Point(1, 1);
        var piece = new PieceFaker().Generate();
        var board = new ChessBoard(new() { [pt] = piece });

        var result = board.PeekPieceAt(pt);

        result.Should().Be(piece);
    }

    [Fact]
    public void PeekPieceAt_returns_null_if_it_doesnt_exist()
    {
        var board = new ChessBoard([]);

        var result = board.PeekPieceAt(new Point(1, 1));

        result.Should().BeNull();
    }

    [Fact]
    public void IsEmpty_returns_true_when_the_square_is_empty()
    {
        var board = new ChessBoard(new() { [new Point(5, 5)] = new PieceFaker().Generate() });
        board.IsEmpty(new Point(4, 4)).Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_returns_false_when_the_square_is_not_empty()
    {
        var pt = new Point(4, 4);
        var board = new ChessBoard(new() { [pt] = new PieceFaker().Generate() });

        board.IsEmpty(pt).Should().BeFalse();
    }

    [Fact]
    public void MovePiece_updates_the_board_and_increments_moved_count()
    {
        var from = new Point(1, 1);
        var to = new Point(2, 2);
        var piece = new PieceFaker().Generate();
        var board = new ChessBoard(new Dictionary<Point, Piece> { [from] = piece });

        var result = board.MovePiece(from, to);

        result.IsError.Should().BeFalse();

        board.IsEmpty(from).Should().BeTrue();
        board.TryGetPieceAt(to, out var movedPiece).Should().BeTrue();
        movedPiece.Should().NotBeNull();
        movedPiece.TimesMoved.Should().Be(1);
    }

    [Fact]
    public void MovePiece_returns_an_error_when_there_is_no_piece_at_origin_point()
    {
        var board = new ChessBoard([]);

        var result = board.MovePiece(new Point(1, 1), new Point(2, 2));

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(GameErrors.PieceNotFound);
    }

    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(9, 9, true)]
    [InlineData(-1, 5, false)]
    [InlineData(5, 10, false)]
    public void IsWithinBoundaries_checks_boundaries_correctly(int x, int y, bool expected)
    {
        var board = new ChessBoard([]);
        var point = new Point(x, y);

        board.IsWithinBoundaries(point).Should().Be(expected);
    }

    [Fact]
    public void GetSquares_returns_all_squares()
    {
        var board = new ChessBoard([]);

        var squares = board.GetSquares();

        squares.Should().HaveCount(100);
    }
}
