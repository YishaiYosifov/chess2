using Chess2.Api.GameLogic.Models;
using Chess2.Api.QuestLogic.MoveConditions;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.QuestLogicTests.MoveConditionTests;

public class IsMoveCaptureTests
{
    [Fact]
    public void Evaluate_returns_true_for_enough_captures()
    {
        var move = MoveFaker.Capture(
            GameColor.White,
            captureTypes: [PieceType.Pawn, PieceType.Knook]
        );
        new IsMoveCapture(ofAtLeast: 2).Evaluate(move).Should().BeTrue();
    }

    [Fact]
    public void Evaluate_returns_true_for_not_enough_captures()
    {
        var move = MoveFaker.Capture(GameColor.White, captureTypes: [PieceType.Pawn]);
        new IsMoveCapture(ofAtLeast: 2).Evaluate(move).Should().BeFalse();
    }
}
