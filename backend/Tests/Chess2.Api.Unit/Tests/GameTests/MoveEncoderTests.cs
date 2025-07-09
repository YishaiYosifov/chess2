using Chess2.Api.Game.Services;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure.Factories;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.GameTests;

public class MoveEncoderTests : BaseUnitTest
{
    private readonly MoveEncoder _encoder = new();
    private readonly Piece _dummyPiece = PieceFactory.White();

    [Fact]
    public void EncodeMoves_single_basic_move()
    {
        var move = new Move(
            from: new AlgebraicPoint("e2"),
            to: new AlgebraicPoint("e4"),
            piece: _dummyPiece
        );

        var result = _encoder.EncodeMoves([move]);

        result.Should().BeEquivalentTo(["e2e4"]);
    }

    [Fact]
    public void EncodeMoves_multiple_moves()
    {
        var move1 = new Move(
            from: new AlgebraicPoint("e2"),
            to: new AlgebraicPoint("e4"),
            piece: _dummyPiece
        );
        var move2 = new Move(
            from: new AlgebraicPoint("g1"),
            to: new AlgebraicPoint("f3"),
            piece: _dummyPiece
        );

        var result = _encoder.EncodeMoves([move1, move2]);

        result.Should().BeEquivalentTo(["e2e4", "g1f3"]);
    }

    [Fact]
    public void EncodeMoves_single_move_with_trigger_squares()
    {
        var move = new Move(
            from: new AlgebraicPoint("e1"),
            triggerSquares: [new AlgebraicPoint("f1")],
            to: new AlgebraicPoint("g1"),
            piece: _dummyPiece
        );

        var result = _encoder.EncodeMoves([move]);

        result.Should().BeEquivalentTo(["e1f1g1"]);
    }

    [Fact]
    public void EncodeMoves_single_move_with_side_effects()
    {
        var sideEffect = new Move(
            from: new AlgebraicPoint("h1"),
            to: new AlgebraicPoint("f1"),
            piece: _dummyPiece
        );
        var move = new Move(
            from: new AlgebraicPoint("e1"),
            triggerSquares: [new AlgebraicPoint("f1"), new AlgebraicPoint("g1")],
            to: new AlgebraicPoint("h1"),
            piece: _dummyPiece,
            sideEffects: [sideEffect]
        );

        var result = _encoder.EncodeMoves([move]);

        result.Should().BeEquivalentTo(["e1f1g1h1-h1f1"]);
    }

    [Fact]
    public void EncodeMoves_single_move_with_captured_squares()
    {
        var move = new Move(
            from: new AlgebraicPoint("e4"),
            to: new AlgebraicPoint("d5"),
            piece: _dummyPiece,
            capturedSquares: [new AlgebraicPoint("d5")]
        );

        var result = _encoder.EncodeMoves([move]);

        result.Should().BeEquivalentTo(["e4d5!d5"]);
    }

    [Fact]
    public void EncodeMoves_both_move_and_side_effect_with_captures()
    {
        var capture = new Move(
            from: new AlgebraicPoint("b2"),
            to: new AlgebraicPoint("b3"),
            piece: _dummyPiece,
            capturedSquares: [new AlgebraicPoint("f3"), new AlgebraicPoint("b3")]
        );
        var move = new Move(
            from: new AlgebraicPoint("e4"),
            to: new AlgebraicPoint("d5"),
            piece: _dummyPiece,
            sideEffects: [capture],
            capturedSquares: [new AlgebraicPoint("d7")]
        );

        var result = _encoder.EncodeMoves([move]);

        result.Should().BeEquivalentTo(["e4d5!d7-b2b3!f3!b3"]);
    }

    [Fact]
    public void EncodeSingleMove_returns_expected_basic_path()
    {
        var move = new Move(new AlgebraicPoint("e2"), new AlgebraicPoint("e4"), _dummyPiece);
        _encoder.EncodeSingleMove(move).Should().Be("e2e4");
    }

    [Fact]
    public void EncodeSingleMove_handles_side_effects_and_captures()
    {
        var sideEffect = new Move(
            new AlgebraicPoint("b2"),
            new AlgebraicPoint("b3"),
            _dummyPiece,
            capturedSquares: [new AlgebraicPoint("f3")]
        );
        var move = new Move(
            new AlgebraicPoint("e4"),
            new AlgebraicPoint("d5"),
            _dummyPiece,
            sideEffects: [sideEffect],
            capturedSquares: [new AlgebraicPoint("d7")]
        );

        _encoder.EncodeSingleMove(move).Should().Be("e4d5!d7-b2b3!f3");
    }
}
