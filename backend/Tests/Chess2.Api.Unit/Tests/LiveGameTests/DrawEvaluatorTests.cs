using Chess2.Api.GameLogic.Models;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.TestInfrastructure.Factories;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.LiveGameTests;

public class DrawEvaluatorTests
{
    private readonly GameResultDescriber _describer = new();
    private readonly DrawEvaulator _drawEvaluator;

    public DrawEvaluatorTests()
    {
        _drawEvaluator = new DrawEvaulator(_describer);
    }

    [Fact]
    public void TryEvaluateDraw_returns_false_with_no_draw_condition()
    {
        var move = new Move(from: new("a1"), to: new("a2"), piece: PieceFactory.White());

        var result = _drawEvaluator.TryEvaluateDraw(move, "fen", out var endStatus);

        result.Should().BeFalse();
        endStatus.Should().BeNull();
    }

    [Fact]
    public void TryEvaluateDraw_returns_true_after_three_fold_repetition()
    {
        var move = new Move(from: new("a1"), to: new("a2"), piece: PieceFactory.White());

        _drawEvaluator.TryEvaluateDraw(move, "fen", out _).Should().BeFalse();
        _drawEvaluator.TryEvaluateDraw(move, "fen", out _).Should().BeFalse();
        var result = _drawEvaluator.TryEvaluateDraw(move, "fen", out var endStatus);

        result.Should().BeTrue();
        endStatus.Should().BeEquivalentTo(_describer.ThreeFold());
    }

    [Fact]
    public void TryEvaluateDraw_returns_true_after_50_moves()
    {
        var move = new Move(
            from: new("a1"),
            to: new("a2"),
            piece: PieceFactory.White(PieceType.Horsey)
        );

        for (int i = 0; i < 99; i++)
        {
            _drawEvaluator.TryEvaluateDraw(move, $"fen {i}", out _).Should().BeFalse();
        }

        var result = _drawEvaluator.TryEvaluateDraw(move, $"fen final", out var endStatus);

        result.Should().BeTrue();
        endStatus.Should().BeEquivalentTo(_describer.FiftyMoves());
    }

    [Fact]
    public void TryEvaluateDraw_resets_half_clock_after_pawn_move()
    {
        var pawnMove = new Move(
            from: new("a1"),
            to: new("a2"),
            piece: PieceFactory.White(PieceType.Pawn)
        );
        TestFiftyMoveReset(pawnMove);
    }

    [Fact]
    public void TryEvaluateDraw_ResetsHalfMoveClock_OnCapture()
    {
        var captureMove = new Move(
            from: new("a1"),
            to: new("a2"),
            piece: PieceFactory.White(PieceType.Knook),
            capturedSquares: [new("b2")]
        );
        TestFiftyMoveReset(captureMove);
    }

    private void TestFiftyMoveReset(Move resetMove)
    {
        var regularMove = new Move(
            from: new("a1"),
            to: new("a2"),
            piece: PieceFactory.White(PieceType.Horsey)
        );
        for (int i = 0; i < 49; i++)
        {
            _drawEvaluator.TryEvaluateDraw(regularMove, $"fen {i}", out _).Should().BeFalse();
        }

        _drawEvaluator.TryEvaluateDraw(resetMove, "fen reset", out var _).Should().BeFalse();

        for (int i = 0; i < 50; i++)
        {
            _drawEvaluator
                .TryEvaluateDraw(regularMove, $"fen after reset {i}", out _)
                .Should()
                .BeFalse();
        }
    }
}
