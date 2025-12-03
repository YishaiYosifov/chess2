using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameLogic.PieceMovementRules;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AwesomeAssertions;

namespace AnarchyChess.Api.Unit.Tests.GameLogicTests.PieceMovementRuleTests;

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

    private readonly Piece _kingWhite = PieceFactory.White(PieceType.King, timesMoved: 0);
    private readonly Piece _rookWhite = PieceFactory.White(PieceType.Rook, timesMoved: 0);

    private readonly Piece _kingBlack = PieceFactory.Black(PieceType.King, timesMoved: 0);
    private readonly Piece _rookBlack = PieceFactory.Black(PieceType.Rook, timesMoved: 0);

    [Fact]
    public void Evaluate_returns_nothing_if_king_has_moved()
    {
        ChessBoard board = new();

        var king = _kingWhite with { TimesMoved = 1 };
        board.PlacePiece(_kingOrigin, king);
        board.PlacePiece(_rookQueensideOrigin, _rookWhite);
        board.PlacePiece(_rookKingsideOrigin, _rookWhite);

        var result = _rule.Evaluate(board, _kingOrigin, king);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_returns_nothing_if_rook_has_moved()
    {
        ChessBoard board = new();

        board.PlacePiece(_kingOrigin, _kingWhite);
        board.PlacePiece(_rookQueensideOrigin, _rookWhite with { TimesMoved = 1 });
        board.PlacePiece(_rookKingsideOrigin, _rookWhite with { TimesMoved = 1 });

        var result = _rule.Evaluate(board, _kingOrigin, _kingWhite);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_returns_both_castling_moves_if_path_clear()
    {
        ChessBoard board = new();

        board.PlacePiece(_kingOrigin, _kingWhite);
        board.PlacePiece(_rookKingsideOrigin, _rookWhite);
        board.PlacePiece(_rookQueensideOrigin, _rookWhite);

        var result = _rule.Evaluate(board, _kingOrigin, _kingWhite).ToList();

        Move kingside = new(
            _kingOrigin,
            _kingKingsideDestination,
            _kingWhite,
            triggerSquares: [new("i1")],
            sideEffects: [new(_rookKingsideOrigin, _rookKingsideDestination, _rookWhite)],
            specialMoveType: SpecialMoveType.KingsideCastle
        );
        Move queenside = new(
            _kingOrigin,
            _kingQueensideDestination,
            _kingWhite,
            triggerSquares: [new("c1"), new("b1")],
            sideEffects: [new(_rookQueensideOrigin, _rookQueensideDestination, _rookWhite)],
            specialMoveType: SpecialMoveType.QueensideCastle
        );

        result.Should().BeEquivalentTo([kingside, queenside]);
    }

    [Fact]
    public void Evaluate_returns_nothing_if_piece_blocks_castling_path()
    {
        ChessBoard board = new();

        board.PlacePiece(_kingOrigin, _kingWhite);
        board.PlacePiece(_rookQueensideOrigin, _rookWhite);
        board.PlacePiece(_rookKingsideOrigin, _rookWhite);
        board.PlacePiece(new("h1"), PieceFactory.White(PieceType.Queen));
        board.PlacePiece(new("d1"), PieceFactory.White(PieceType.Rook));

        var result = _rule.Evaluate(board, _kingOrigin, _kingWhite);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_captures_bishop_if_same_color_and_on_a_landing_square()
    {
        ChessBoard board = new();
        var bishop = PieceFactory.White(PieceType.Bishop);
        AlgebraicPoint bishopPosition = new("g1");

        board.PlacePiece(_kingOrigin, _kingWhite);
        board.PlacePiece(_rookKingsideOrigin, _rookWhite);
        board.PlacePiece(bishopPosition, bishop);

        var result = _rule.Evaluate(board, _kingOrigin, _kingWhite).ToList();

        Move expectedMove = new(
            _kingOrigin,
            _kingKingsideDestination,
            _kingWhite,
            triggerSquares: [new("i1")],
            captures: [new MoveCapture(bishop, bishopPosition)],
            sideEffects: [new(_rookKingsideOrigin, _rookKingsideDestination, _rookWhite)],
            specialMoveType: SpecialMoveType.KingsideCastle
        );
        result.Should().BeEquivalentTo([expectedMove]);
    }

    [Fact]
    public void Evaluate_disallows_bishop_of_another_color_to_be_captured()
    {
        ChessBoard board = new();
        var bishop = PieceFactory.Black(PieceType.Bishop);
        AlgebraicPoint opponentBishopPosition = new("g1");

        board.PlacePiece(_kingOrigin, _kingWhite);
        board.PlacePiece(_rookKingsideOrigin, _rookWhite);
        board.PlacePiece(opponentBishopPosition, bishop);

        var result = _rule.Evaluate(board, _kingOrigin, _kingWhite).ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_disallow_capturing_the_bishop_if_we_dont_land_on_it()
    {
        ChessBoard board = new();
        var bishop = PieceFactory.White(PieceType.Bishop);
        AlgebraicPoint bishopPosition = new("i1");

        board.PlacePiece(_kingOrigin, _kingWhite);
        board.PlacePiece(_rookKingsideOrigin, _rookWhite);
        board.PlacePiece(bishopPosition, bishop);

        var result = _rule.Evaluate(board, _kingOrigin, _kingWhite).ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_returns_nothing_if_rook_not_at_expected_edge_file()
    {
        ChessBoard board = new();

        board.PlacePiece(_kingOrigin, _kingWhite);
        board.PlacePiece(new("e1"), _rookWhite); // not at one of the edges

        var result = _rule.Evaluate(board, _kingOrigin, _kingWhite);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_returns_nothing_if_rook_is_opponent_color()
    {
        ChessBoard board = new();

        board.PlacePiece(_kingOrigin, _kingWhite);
        board.PlacePiece(_rookKingsideOrigin, PieceFactory.Black(PieceType.Rook));

        var result = _rule.Evaluate(board, _kingOrigin, _kingWhite);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_returns_nothing_if_rook_destination_is_blocked()
    {
        ChessBoard board = new();
        var blocker = PieceFactory.White(PieceType.Horsey);

        board.PlacePiece(_kingOrigin, _kingWhite);
        board.PlacePiece(_rookKingsideOrigin, _rookWhite);
        board.PlacePiece(_rookKingsideDestination, blocker);

        var result = _rule.Evaluate(board, _kingOrigin, _kingWhite);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_returns_nothing_if_castling_piece_is_not_a_rook()
    {
        ChessBoard board = new();

        board.PlacePiece(_kingOrigin, _kingWhite);
        board.PlacePiece(_rookKingsideOrigin, PieceFactory.White(PieceType.Horsey));

        var result = _rule.Evaluate(board, _kingOrigin, _kingWhite);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_supports_castling_for_black_king()
    {
        ChessBoard board = new();

        AlgebraicPoint blackKingOrigin = new("f10");

        AlgebraicPoint rookKingsideOrigin = new("j10");
        AlgebraicPoint kingKingsideDestination = new("h10");
        AlgebraicPoint rookKingsideDestination = new("g10");

        AlgebraicPoint rookQueensideOrigin = new("a10");
        AlgebraicPoint kingQueensideDestination = new("d10");
        AlgebraicPoint rookQueensideDestination = new("e10");

        board.PlacePiece(blackKingOrigin, _kingBlack);
        board.PlacePiece(rookKingsideOrigin, _rookBlack);
        board.PlacePiece(rookQueensideOrigin, _rookBlack);

        var result = _rule.Evaluate(board, blackKingOrigin, _kingBlack).ToList();

        Move kingside = new(
            blackKingOrigin,
            kingKingsideDestination,
            _kingBlack,
            triggerSquares: [new("i10")],
            sideEffects: [new(rookKingsideOrigin, rookKingsideDestination, _rookBlack)],
            specialMoveType: SpecialMoveType.KingsideCastle
        );

        Move queenside = new(
            blackKingOrigin,
            kingQueensideDestination,
            _kingBlack,
            triggerSquares: [new("c10"), new("b10")],
            sideEffects: [new(rookQueensideOrigin, rookQueensideDestination, _rookBlack)],
            specialMoveType: SpecialMoveType.QueensideCastle
        );

        result.Should().BeEquivalentTo([kingside, queenside]);
    }

    [Fact]
    public void Evaluate_returns_vertical_castling_for_white_king_if_path_clear()
    {
        ChessBoard board = new();
        AlgebraicPoint kingOrigin = new("f1");
        AlgebraicPoint rookOrigin = new("f10");

        board.PlacePiece(kingOrigin, _kingWhite);
        board.PlacePiece(rookOrigin, _rookWhite);

        var result = _rule.Evaluate(board, kingOrigin, _kingWhite).ToList();

        var kingDest = new AlgebraicPoint("f3");
        var rookDest = new AlgebraicPoint("f2");

        Move expectedMove = new(
            kingOrigin,
            kingDest,
            _kingWhite,
            triggerSquares: [new("f4"), new("f5"), new("f6"), new("f7"), new("f8"), new("f9")],
            sideEffects: [new MoveSideEffect(rookOrigin, rookDest, _rookWhite)],
            specialMoveType: SpecialMoveType.VerticalCastle
        );

        result.Should().BeEquivalentTo([expectedMove]);
    }

    [Fact]
    public void Evaluate_returns_vertical_castling_for_black_king_if_path_clear()
    {
        ChessBoard board = new();
        AlgebraicPoint kingOrigin = new("f10");
        AlgebraicPoint rookOrigin = new("f1");

        board.PlacePiece(kingOrigin, _kingBlack);
        board.PlacePiece(rookOrigin, _rookBlack);

        var result = _rule.Evaluate(board, kingOrigin, _kingBlack).ToList();

        var kingDest = new AlgebraicPoint("f8");
        var rookDest = new AlgebraicPoint("f9");

        Move expectedMove = new(
            kingOrigin,
            kingDest,
            _kingBlack,
            triggerSquares: [new("f2"), new("f3"), new("f4"), new("f5"), new("f6"), new("f7")],
            sideEffects: [new MoveSideEffect(rookOrigin, rookDest, _rookBlack)],
            specialMoveType: SpecialMoveType.VerticalCastle
        );

        result.Should().BeEquivalentTo([expectedMove]);
    }
}
