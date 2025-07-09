using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.PieceMovementRules;
using Chess2.Api.TestInfrastructure.Factories;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.GameLogicTests.PieceMovementRuleTests;

public class CastleRuleTests
{
    private readonly CastleRule _rule = new();

    private readonly AlgebraicPoint _kingOrigin = new("f1");

    private readonly AlgebraicPoint _rookKingsideOrigin = new("j1");
    private readonly AlgebraicPoint _kingKingsideDestination = new("h1");
    private readonly AlgebraicPoint _rookKingsideDestination = new("g1");

    private readonly AlgebraicPoint _rookQueensideOrigin = new("a1");
    private readonly AlgebraicPoint _kingQueensideDestination = new("d1");
    private readonly AlgebraicPoint _rookQueensideDestination = new("e1");

    private readonly Piece _king = PieceFactory.White(PieceType.King);
    private readonly Piece _rook = PieceFactory.White(PieceType.Rook);

    [Fact]
    public void Evaluate_returns_nothing_if_king_has_moved()
    {
        var board = new ChessBoard();

        var king = _king with { TimesMoved = 1 };
        board.PlacePiece(_kingOrigin, king);
        board.PlacePiece(_rookQueensideOrigin, _rook);
        board.PlacePiece(_rookKingsideOrigin, _rook);

        var result = _rule.Evaluate(board, _kingOrigin, king);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_returns_nothing_if_rook_has_moved()
    {
        var board = new ChessBoard();

        board.PlacePiece(_kingOrigin, _king);
        board.PlacePiece(_rookQueensideOrigin, _rook with { TimesMoved = 1 });
        board.PlacePiece(_rookKingsideOrigin, _rook with { TimesMoved = 1 });

        var result = _rule.Evaluate(board, _kingOrigin, _king);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_returns_both_castling_moves_if_path_clear()
    {
        var board = new ChessBoard();

        board.PlacePiece(_kingOrigin, _king);
        board.PlacePiece(_rookKingsideOrigin, _rook);
        board.PlacePiece(_rookQueensideOrigin, _rook);

        var result = _rule.Evaluate(board, _kingOrigin, _king).ToList();

        var kingside = new Move(
            _kingOrigin,
            _kingKingsideDestination,
            _king,
            triggerSquares: [new("i1")],
            sideEffects: [new(_rookKingsideOrigin, _rookKingsideDestination, _rook)]
        );
        var queenside = new Move(
            _kingOrigin,
            _kingQueensideDestination,
            _king,
            triggerSquares: [new("c1"), new("b1")],
            sideEffects: [new(_rookQueensideOrigin, _rookQueensideDestination, _rook)]
        );

        result.Should().BeEquivalentTo([kingside, queenside]);
    }

    [Fact]
    public void Evaluate_returns_nothing_if_piece_blocks_castling_path()
    {
        var board = new ChessBoard();

        board.PlacePiece(_kingOrigin, _king);
        board.PlacePiece(_rookQueensideOrigin, _rook);
        board.PlacePiece(_rookKingsideOrigin, _rook);
        board.PlacePiece(new("h1"), PieceFactory.White(PieceType.Queen));
        board.PlacePiece(new("d1"), PieceFactory.White(PieceType.Rook));

        var result = _rule.Evaluate(board, _kingOrigin, _king);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_captures_bishop_if_same_color_and_on_a_landing_square()
    {
        var board = new ChessBoard();
        var bishop = PieceFactory.White(PieceType.Bishop);
        AlgebraicPoint bishopPosition = new("g1");

        board.PlacePiece(_kingOrigin, _king);
        board.PlacePiece(_rookKingsideOrigin, _rook);
        board.PlacePiece(bishopPosition, bishop);

        var result = _rule.Evaluate(board, _kingOrigin, _king).ToList();

        var expectedMove = new Move(
            _kingOrigin,
            _kingKingsideDestination,
            _king,
            triggerSquares: [new("i1")],
            capturedSquares: [bishopPosition],
            sideEffects: [new(_rookKingsideOrigin, _rookKingsideDestination, _rook)]
        );
        result.Should().BeEquivalentTo([expectedMove]);
    }

    [Fact]
    public void Evaluate_disallows_bishop_of_another_color_to_be_captured()
    {
        var board = new ChessBoard();
        var bishop = PieceFactory.Black(PieceType.Bishop);
        AlgebraicPoint opponentBishopPosition = new("g1");

        board.PlacePiece(_kingOrigin, _king);
        board.PlacePiece(_rookKingsideOrigin, _rook);
        board.PlacePiece(opponentBishopPosition, bishop);

        var result = _rule.Evaluate(board, _kingOrigin, _king).ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_disallow_capturing_the_bishop_if_we_dont_land_on_it()
    {
        var board = new ChessBoard();
        var bishop = PieceFactory.White(PieceType.Bishop);
        AlgebraicPoint bishopPosition = new("i1");

        board.PlacePiece(_kingOrigin, _king);
        board.PlacePiece(_rookKingsideOrigin, _rook);
        board.PlacePiece(bishopPosition, bishop);

        var result = _rule.Evaluate(board, _kingOrigin, _king).ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_returns_nothing_if_rook_not_at_expected_edge_file()
    {
        var board = new ChessBoard();

        board.PlacePiece(_kingOrigin, _king);
        board.PlacePiece(new("e1"), _rook); // not at one of the edges

        var result = _rule.Evaluate(board, _kingOrigin, _king);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_returns_nothing_if_rook_is_opponent_color()
    {
        var board = new ChessBoard();

        board.PlacePiece(_kingOrigin, _king);
        board.PlacePiece(_rookKingsideOrigin, PieceFactory.Black(PieceType.Rook));

        var result = _rule.Evaluate(board, _kingOrigin, _king);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_returns_nothing_if_rook_destination_is_blocked()
    {
        var board = new ChessBoard();
        var blocker = PieceFactory.White(PieceType.Horsey);

        board.PlacePiece(_kingOrigin, _king);
        board.PlacePiece(_rookKingsideOrigin, _rook);
        board.PlacePiece(_rookKingsideDestination, blocker);

        var result = _rule.Evaluate(board, _kingOrigin, _king);

        result.Should().BeEmpty();
    }
}
