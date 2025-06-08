using Chess2.Api.Game.Services;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.GameTests;

public class LegalMoveEncoderTests : BaseUnitTest
{
    private readonly LegalMoveEncoder _encoder = new();
    private readonly Piece _dummyPiece = new PieceFaker().Generate();

    [Fact]
    public void EncodeLegalMoves_single_basic_move()
    {
        var move = new Move(
            From: new Point(4, 1), // e2
            To: new Point(4, 3), // e4
            Piece: _dummyPiece
        );

        var result = _encoder.EncodeLegalMoves([move]);

        result.Should().Be("e2e4");
    }

    [Fact]
    public void EncodeLegalMoves_multiple_moves()
    {
        var move1 = new Move(
            From: new Point(4, 1), // e2
            To: new Point(4, 3), // e4
            Piece: _dummyPiece
        );
        var move2 = new Move(
            From: new Point(6, 0), // g1
            To: new Point(5, 2), // f3
            Piece: _dummyPiece
        );

        var result = _encoder.EncodeLegalMoves([move1, move2]);

        result.Should().Be("e2e4 g1f3");
    }

    [Fact]
    public void EncodeLegalMoves_single_move_with_through_points()
    {
        var move = new Move(
            From: new Point(4, 0), // e1
            Through: [new Point(5, 0)], // f1
            To: new Point(6, 0), // g1,
            Piece: _dummyPiece
        );

        var result = _encoder.EncodeLegalMoves([move]);

        result.Should().Be("e1>f1g1");
    }

    [Fact]
    public void EncodeLegalMoves_single_move_with_side_effects()
    {
        var sideEffect = new Move(
            From: new Point(7, 0), // h1
            To: new Point(5, 0), // f1
            Piece: _dummyPiece
        );
        var move = new Move(
            From: new Point(4, 0), // e1
            Through: [new Point(5, 0), new Point(6, 0)], // f1, g1
            To: new Point(7, 0), // h1
            Piece: _dummyPiece,
            SideEffects: [sideEffect]
        );

        var result = _encoder.EncodeLegalMoves([move]);

        result.Should().Be("e1>f1>g1h1-h1f1");
    }

    [Fact]
    public void EncodeLegalMoves_single_move_with_captured_squares()
    {
        var move = new Move(
            From: new Point(4, 3), // e4
            To: new Point(3, 4), // d5
            Piece: _dummyPiece,
            CapturedSquares: [new Point(3, 4)] // capture on d5
        );

        var result = _encoder.EncodeLegalMoves([move]);

        result.Should().Be("e4d5!d5");
    }

    [Fact]
    public void EncodeLegalMoves_both_move_and_side_effect_with_captures()
    {
        var capture = new Move(
            From: new Point(1, 1), // b2
            To: new Point(1, 2), // b3
            Piece: _dummyPiece,
            CapturedSquares: [new Point(5, 2), new Point(1, 2)] // f3, b3
        );
        var move = new Move(
            From: new Point(4, 3), // e4
            To: new Point(3, 4), // d5
            Piece: _dummyPiece,
            SideEffects: [capture],
            CapturedSquares: [new Point(3, 6)] // d7
        );

        var result = _encoder.EncodeLegalMoves([move]);

        result.Should().Be("e4d5!d7-b2b3!f3!b3");
    }
}
