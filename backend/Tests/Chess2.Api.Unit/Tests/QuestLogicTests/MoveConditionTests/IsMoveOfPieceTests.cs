using Chess2.Api.GameLogic.Models;
using Chess2.Api.QuestLogic.MoveConditions;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.QuestLogicTests.MoveConditionTests;

public class IsMoveOfPieceTests
{
    [Fact]
    public void Evaluate_returns_true_for_matching_piece_type()
    {
        var move = new MoveFaker(pieceType: PieceType.Bishop);
        new IsMoveOfPiece(PieceType.Bishop).Evaluate(move).Should().BeTrue();
    }

    [Fact]
    public void Evaluate_returns_false_for_non_matching_piece_type()
    {
        var move = new MoveFaker(pieceType: PieceType.Bishop);
        new IsMoveOfPiece(PieceType.Horsey).Evaluate(move).Should().BeFalse();
    }
}
