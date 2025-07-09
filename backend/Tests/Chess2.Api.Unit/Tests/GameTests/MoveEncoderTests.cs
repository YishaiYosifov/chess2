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
            From: new AlgebraicPoint("e2"),
            To: new AlgebraicPoint("e4"),
            Piece: _dummyPiece
        );

        var result = _encoder.EncodeMoves([move]);

        result.Should().BeEquivalentTo(["e2e4"]);
    }

    [Fact]
    public void EncodeMoves_multiple_moves()
    {
        var move1 = new Move(
            From: new AlgebraicPoint("e2"),
            To: new AlgebraicPoint("e4"),
            Piece: _dummyPiece
        );
        var move2 = new Move(
            From: new AlgebraicPoint("g1"),
            To: new AlgebraicPoint("f3"),
            Piece: _dummyPiece
        );

        var result = _encoder.EncodeMoves([move1, move2]);

        result.Should().BeEquivalentTo(["e2e4", "g1f3"]);
    }

    [Fact]
    public void EncodeMoves_single_move_with_trigger_squares()
    {
        var move = new Move(
            From: new AlgebraicPoint("e1"),
            TriggerSquares: [new AlgebraicPoint("f1")],
            To: new AlgebraicPoint("g1"),
            Piece: _dummyPiece
        );

        var result = _encoder.EncodeMoves([move]);

        result.Should().BeEquivalentTo(["e1f1g1"]);
    }

    [Fact]
    public void EncodeMoves_single_move_with_side_effects()
    {
        var sideEffect = new Move(
            From: new AlgebraicPoint("h1"),
            To: new AlgebraicPoint("f1"),
            Piece: _dummyPiece
        );
        var move = new Move(
            From: new AlgebraicPoint("e1"),
            TriggerSquares: [new AlgebraicPoint("f1"), new AlgebraicPoint("g1")],
            To: new AlgebraicPoint("h1"),
            Piece: _dummyPiece,
            SideEffects: [sideEffect]
        );

        var result = _encoder.EncodeMoves([move]);

        result.Should().BeEquivalentTo(["e1f1g1h1-h1f1"]);
    }

    [Fact]
    public void EncodeMoves_single_move_with_captured_squares()
    {
        var move = new Move(
            From: new AlgebraicPoint("e4"),
            To: new AlgebraicPoint("d5"),
            Piece: _dummyPiece,
            CapturedSquares: [new AlgebraicPoint("d5")]
        );

        var result = _encoder.EncodeMoves([move]);

        result.Should().BeEquivalentTo(["e4d5!d5"]);
    }

    [Fact]
    public void EncodeMoves_both_move_and_side_effect_with_captures()
    {
        var capture = new Move(
            From: new AlgebraicPoint("b2"),
            To: new AlgebraicPoint("b3"),
            Piece: _dummyPiece,
            CapturedSquares: [new AlgebraicPoint("f3"), new AlgebraicPoint("b3")]
        );
        var move = new Move(
            From: new AlgebraicPoint("e4"),
            To: new AlgebraicPoint("d5"),
            Piece: _dummyPiece,
            SideEffects: [capture],
            CapturedSquares: [new AlgebraicPoint("d7")]
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
            CapturedSquares: [new AlgebraicPoint("f3")]
        );
        var move = new Move(
            new AlgebraicPoint("e4"),
            new AlgebraicPoint("d5"),
            _dummyPiece,
            SideEffects: [sideEffect],
            CapturedSquares: [new AlgebraicPoint("d7")]
        );

        _encoder.EncodeSingleMove(move).Should().Be("e4d5!d7-b2b3!f3");
    }
}
