using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.QuestLogic.MoveConditions;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace AnarchyChess.Api.Unit.Tests.QuestLogicTests.MoveConditionTests;

public class IsMoveOfPieceTests
{
    [Fact]
    public void Evaluate_returns_true_for_matching_piece_type()
    {
        var move = new MoveFaker(pieceType: PieceType.Horsey);
        new IsMoveOfPiece(PieceType.Bishop, PieceType.Rook, PieceType.Horsey)
            .Evaluate(move)
            .Should()
            .BeTrue();
    }

    [Fact]
    public void Evaluate_returns_false_for_non_matching_piece_type()
    {
        var move = new MoveFaker(pieceType: PieceType.Horsey);
        new IsMoveOfPiece(PieceType.Bishop, PieceType.Rook).Evaluate(move).Should().BeFalse();
    }
}
