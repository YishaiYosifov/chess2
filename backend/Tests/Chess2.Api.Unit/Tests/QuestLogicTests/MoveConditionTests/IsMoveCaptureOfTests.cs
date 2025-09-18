using Chess2.Api.GameLogic.Models;
using Chess2.Api.QuestLogic.MoveConditions;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.QuestLogicTests.MoveConditionTests;

public class IsMoveCaptureOfTests
{
    [Fact]
    public void Evaluate_returns_true_for_capture_of_specified_piece_type()
    {
        var move = MoveFaker.Capture(
            GameColor.White,
            captureTypes: [PieceType.Horsey, PieceType.Antiqueen]
        );
        new IsMoveCaptureOf(PieceType.Horsey).Evaluate(move).Should().BeTrue();
    }

    [Fact]
    public void Evaluate_returns_false_for_capture_of_different_piece_type()
    {
        var move = MoveFaker.Capture(GameColor.White, captureType: PieceType.Pawn);
        new IsMoveCaptureOf(PieceType.Horsey).Evaluate(move).Should().BeFalse();
    }
}
