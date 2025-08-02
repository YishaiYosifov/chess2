using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.PieceMovementRules;
using FluentAssertions;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.GameLogicTests.PieceMovementRuleTests;

public class PromotionRuleTests : RuleBasedPieceRuleTestBase
{
    private readonly List<PieceType> _expectedPromotionTargets =
    [
        .. Enum.GetValues<PieceType>()
            .Where(p => p != PieceType.King && p != PieceType.Pawn && p != PieceType.UnderagePawn),
    ];

    [Fact]
    public void Evaluate_yields_nonPromotion_moves_unchanged()
    {
        var rule = new PromotionRule((board, move) => move == Move2, BaseRuleMock);

        var result = rule.Evaluate(Board, Origin, Piece).ToList();

        result.Should().HaveCount(2 + _expectedPromotionTargets.Count);
        result.Should().Contain([Move1, Move3]);

        var promotionMoves = result.Where(m => m.To == Move2.To && m.PromotesTo != null).ToList();
        promotionMoves.Select(m => m.PromotesTo).Should().BeEquivalentTo(_expectedPromotionTargets);
    }

    [Fact]
    public void Evaluate_yields_moves_unchanged_when_predicate_fails()
    {
        var rule = new PromotionRule((_, _) => false, BaseRuleMock);

        var result = rule.Evaluate(Board, Origin, Piece).ToList();

        result.Should().BeEquivalentTo([Move1, Move2, Move3]);
    }

    [Fact]
    public void Evaluate_returns_empty_when_base_rules_return_no_moves()
    {
        var emptyBaseRule = Substitute.For<IPieceMovementRule>();
        emptyBaseRule.Evaluate(Board, Origin, Piece).Returns([]);

        var rule = new PromotionRule((_, _) => true, emptyBaseRule);

        var result = rule.Evaluate(Board, Origin, Piece).ToList();

        result.Should().BeEmpty();
    }
}
