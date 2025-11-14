using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.QuestLogic.MoveConditions;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace AnarchyChess.Api.Unit.Tests.QuestLogicTests.MoveConditionTests;

public class IsMovePromotionTests
{
    [Fact]
    public void Evaluate_returns_true_for_promotion_move()
    {
        var move = new MoveFaker().RuleFor(x => x.PromotesTo, PieceType.Queen);
        new IsMovePromotion().Evaluate(move).Should().BeTrue();
    }

    [Fact]
    public void Evaluate_returns_false_for_non_promotion_move()
    {
        var move = new MoveFaker().RuleFor(x => x.PromotesTo, (PieceType?)null);
        new IsMovePromotion().Evaluate(move).Should().BeFalse();
    }
}
