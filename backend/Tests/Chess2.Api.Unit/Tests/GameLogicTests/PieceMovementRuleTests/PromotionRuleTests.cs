using Chess2.Api.GameLogic.PieceMovementRules;
using Chess2.Api.LiveGame;
using FluentAssertions;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.GameLogicTests.PieceMovementRuleTests;

public class PromotionRuleTests : RuleBasedPieceRuleTestBase
{
    [Fact]
    public void Evaluate_yields_non_promotion_moves_unchanged()
    {
        PromotionRule rule = new(
            (board, move) => move == Moves[1],
            GameConstants.PromotablePieces,
            RuleMocks
        );

        var result = rule.Evaluate(Board, Origin, Piece).ToList();

        result.Should().HaveCount(Moves.Length - 1 + GameConstants.PromotablePieces.Count);
        result.Should().Contain([Moves[0], .. Moves[2..]]);

        var promotionMoves = result
            .Where(m => m.To == Moves[1].To && m.PromotesTo != null)
            .ToList();

        promotionMoves
            .Select(m => m.PromotesTo)
            .Should()
            .BeEquivalentTo(GameConstants.PromotablePieces);
    }

    [Fact]
    public void Evaluate_yields_moves_unchanged_when_predicate_fails()
    {
        var rule = new PromotionRule((_, _) => false, GameConstants.PromotablePieces, RuleMocks);

        var result = rule.Evaluate(Board, Origin, Piece).ToList();

        result.Should().BeEquivalentTo(Moves);
    }

    [Fact]
    public void Evaluate_returns_empty_when_base_rules_return_no_moves()
    {
        var emptyBaseRule = Substitute.For<IPieceMovementRule>();
        emptyBaseRule.Evaluate(Board, Origin, Piece).Returns([]);

        var rule = new PromotionRule((_, _) => true, GameConstants.PromotablePieces, emptyBaseRule);

        var result = rule.Evaluate(Board, Origin, Piece).ToList();

        result.Should().BeEmpty();
    }
}
