using Chess2.Api.GameLogic.Models;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.TestInfrastructure.Factories;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.LiveGameTests;

public class DrawEvaluatorTests
{
    private readonly GameResultDescriber _describer = new();
    private readonly DrawEvaulator _drawEvaluator;
    private readonly AutoDrawState _state;

    public DrawEvaluatorTests()
    {
        _drawEvaluator = new DrawEvaulator(_describer);
        _state = new AutoDrawState();
    }

    [Fact]
    public void TryEvaluateDraw_returns_false_with_no_draw_condition()
    {
        Move move = new(from: new("a1"), to: new("a2"), piece: PieceFactory.White());

        var result = _drawEvaluator.TryEvaluateDraw(move, "fen", _state, out var endStatus);

        result.Should().BeFalse();
        endStatus.Should().BeNull();
    }

    [Fact]
    public void TryEvaluateDraw_returns_true_after_three_fold_repetition()
    {
        Move move = new(from: new("a1"), to: new("a2"), piece: PieceFactory.White());

        _drawEvaluator.TryEvaluateDraw(move, "fen", _state, out _).Should().BeFalse();
        _drawEvaluator.TryEvaluateDraw(move, "fen", _state, out _).Should().BeFalse();
        var result = _drawEvaluator.TryEvaluateDraw(move, "fen", _state, out var endStatus);

        result.Should().BeTrue();
        endStatus.Should().BeEquivalentTo(_describer.ThreeFold());
    }

    [Fact]
    public void TryEvaluateDraw_returns_true_after_50_moves()
    {
        Move move = new(
            from: new("a1"),
            to: new("a2"),
            piece: PieceFactory.White(PieceType.Horsey)
        );

        for (int i = 0; i < 99; i++)
        {
            _drawEvaluator.TryEvaluateDraw(move, $"fen {i}", _state, out _).Should().BeFalse();
        }

        var result = _drawEvaluator.TryEvaluateDraw(move, $"fen final", _state, out var endStatus);

        result.Should().BeTrue();
        endStatus.Should().BeEquivalentTo(_describer.FiftyMoves());
    }

    [Fact]
    public void TryEvaluateDraw_resets_half_clock_after_pawn_move()
    {
        Move pawnMove = new(
            from: new("a1"),
            to: new("a2"),
            piece: PieceFactory.White(PieceType.Pawn)
        );
        TestFiftyMoveReset(pawnMove);
    }

    [Fact]
    public void TryEvaluateDraw_ResetsHalfMoveClock_OnCapture()
    {
        Move captureMove = new(
            from: new("a1"),
            to: new("a2"),
            piece: PieceFactory.White(PieceType.Knook),
            captures: [new MoveCapture(PieceFactory.Black(), new("b2"))]
        );
        TestFiftyMoveReset(captureMove);
    }

    private void TestFiftyMoveReset(Move resetMove)
    {
        Move regularMove = new(
            from: new("a1"),
            to: new("a2"),
            piece: PieceFactory.White(PieceType.Horsey)
        );
        for (int i = 0; i < 49; i++)
        {
            _drawEvaluator
                .TryEvaluateDraw(regularMove, $"fen {i}", _state, out _)
                .Should()
                .BeFalse();
        }

        _drawEvaluator.TryEvaluateDraw(resetMove, "fen reset", _state, out _).Should().BeFalse();

        for (int i = 0; i < 50; i++)
        {
            _drawEvaluator
                .TryEvaluateDraw(regularMove, $"fen after reset {i}", _state, out _)
                .Should()
                .BeFalse();
        }
    }
}
