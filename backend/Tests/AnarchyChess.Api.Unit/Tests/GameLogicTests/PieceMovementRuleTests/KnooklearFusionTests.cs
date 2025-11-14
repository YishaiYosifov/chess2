using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameLogic.PieceMovementRules;
using AnarchyChess.Api.TestInfrastructure.Factories;
using FluentAssertions;
using NSubstitute;

namespace AnarchyChess.Api.Unit.Tests.GameLogicTests.PieceMovementRuleTests;

public class KnooklearFusionTests
{
    private readonly IPieceMovementRule _ruleMock = Substitute.For<IPieceMovementRule>();
    private readonly Piece _movingPiece = PieceFactory.White();
    private readonly ChessBoard _board = new();

    [Fact]
    public void Evaluate_doesnt_fuse_when_fuseWith_is_not_captured()
    {
        AlgebraicPoint origin = new("e4");
        Piece capture = PieceFactory.White(PieceType.Queen);

        Move captureMove = new(
            from: origin,
            to: new AlgebraicPoint("e5"),
            piece: _movingPiece,
            captures: [new MoveCapture(capture, new("e5"))]
        );
        Move regularMove = new(from: origin, to: new AlgebraicPoint("e6"), piece: _movingPiece);
        _ruleMock.Evaluate(_board, origin, _movingPiece).Returns([captureMove, regularMove]);
        _board.PlacePiece(origin, _movingPiece);
        _board.PlacePiece(captureMove.To, capture);

        KnooklearFusionRule rule = new(fuseWith: PieceType.Rook, _ruleMock);

        var result = rule.Evaluate(_board, origin, _movingPiece).ToList();

        result.Should().BeEquivalentTo([captureMove, regularMove]);
    }

    [Fact]
    public void Evaluate_doesnt_fuse_when_fuseWith_is_captured_but_different_color()
    {
        AlgebraicPoint origin = new("e4");
        Piece capture = PieceFactory.Black(PieceType.Rook);
        Move move = new(
            from: origin,
            to: new AlgebraicPoint("e5"),
            piece: _movingPiece,
            captures: [new MoveCapture(capture, new("e5"))]
        );

        _ruleMock.Evaluate(_board, origin, _movingPiece).Returns([move]);
        _board.PlacePiece(origin, _movingPiece);
        _board.PlacePiece(move.To, capture);

        KnooklearFusionRule rule = new(fuseWith: PieceType.Rook, _ruleMock);
        var result = rule.Evaluate(_board, origin, _movingPiece).ToList();
        result.Should().BeEquivalentTo([move]);
    }

    [Fact]
    public void Evaluate_fuses_when_fuseWith_is_captured()
    {
        AlgebraicPoint origin = new("e4");
        Piece capture = PieceFactory.White(PieceType.Rook);

        Move move = new(
            from: origin,
            to: new AlgebraicPoint("e6"),
            piece: _movingPiece,
            captures: [new MoveCapture(capture, new("e6"))]
        );

        _ruleMock.Evaluate(_board, origin, _movingPiece).Returns([move]);
        _board.PlacePiece(origin, _movingPiece);
        _board.PlacePiece(move.To, capture);
        var expectedMove = CreateExpectedExplosionMove(
            move,
            ["d5", "e5", "f5", "d6", "f6", "d7", "e7", "f7"],
            _board
        );

        KnooklearFusionRule rule = new(fuseWith: PieceType.Rook, _ruleMock);

        var result = rule.Evaluate(_board, origin, _movingPiece).ToList();

        result.Should().BeEquivalentTo([expectedMove]);
    }

    [Fact]
    public void Evaluate_skips_squares_out_of_bounds()
    {
        AlgebraicPoint origin = new("a4");
        Piece capture = PieceFactory.White(PieceType.Rook);
        Move move = new(
            from: origin,
            to: new AlgebraicPoint("a1"),
            piece: _movingPiece,
            captures: [new MoveCapture(capture, new("a1"))]
        );
        _ruleMock.Evaluate(_board, origin, _movingPiece).Returns([move]);
        _board.PlacePiece(origin, _movingPiece);
        _board.PlacePiece(move.To, capture);
        var expectedMove = CreateExpectedExplosionMove(move, ["a2", "b2", "b1"], _board);

        KnooklearFusionRule rule = new(fuseWith: PieceType.Rook, _ruleMock);
        var result = rule.Evaluate(_board, origin, _movingPiece).ToList();

        result.Should().BeEquivalentTo([expectedMove]);
    }

    [Fact]
    public void Evaluate_skips_origin_square()
    {
        AlgebraicPoint origin = new("a2");
        Piece capture = PieceFactory.White(PieceType.Rook);
        Move move = new(
            from: origin,
            to: new AlgebraicPoint("a1"),
            piece: _movingPiece,
            captures: [new MoveCapture(capture, new("a1"))]
        );
        _ruleMock.Evaluate(_board, origin, _movingPiece).Returns([move]);
        _board.PlacePiece(origin, _movingPiece);
        _board.PlacePiece(move.To, capture);
        var expectedMove = CreateExpectedExplosionMove(
            move,
            ["b1", "b2"], // a2 skipped becuase it's the origin
            _board
        );

        KnooklearFusionRule rule = new(fuseWith: PieceType.Rook, _ruleMock);
        var result = rule.Evaluate(_board, origin, _movingPiece).ToList();
        result.Should().BeEquivalentTo([expectedMove]);
    }

    [Fact]
    public void Evaluate_skips_squares_with_no_piece()
    {
        AlgebraicPoint origin = new("e4");
        Piece capture = PieceFactory.White(PieceType.Rook);
        Move move = new(
            from: origin,
            to: new AlgebraicPoint("e6"),
            piece: _movingPiece,
            captures: [new MoveCapture(capture, new("e6"))]
        );
        _ruleMock.Evaluate(_board, origin, _movingPiece).Returns([move]);
        _board.PlacePiece(origin, _movingPiece);
        _board.PlacePiece(move.To, capture);

        KnooklearFusionRule rule = new(fuseWith: PieceType.Rook, _ruleMock);
        var result = rule.Evaluate(_board, origin, _movingPiece).ToList();

        Move expectedMove = move with
        {
            SpecialMoveType = SpecialMoveType.KnooklearFusion,
            PromotesTo = PieceType.Knook,
        };
        result.Should().BeEquivalentTo([expectedMove]);
    }

    private static Move CreateExpectedExplosionMove(
        Move fromMove,
        string[] explosionSquares,
        ChessBoard board
    )
    {
        var i = 0;
        List<MoveCapture> expectedExplosionCaptures = [];
        foreach (var pos in explosionSquares)
        {
            AlgebraicPoint algPos = new(pos);
            var piece = i % 2 == 0 ? PieceFactory.White() : PieceFactory.Black();

            board.PlacePiece(algPos, piece);
            expectedExplosionCaptures.Add(new MoveCapture(piece, algPos));
            i++;
        }

        return fromMove with
        {
            Captures = [.. fromMove.Captures, .. expectedExplosionCaptures],
            SpecialMoveType = SpecialMoveType.KnooklearFusion,
            PromotesTo = PieceType.Knook,
        };
    }
}
