using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.PieceMovementRules;
using FluentAssertions;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.GameLogicTests.PieceMovementRuleTests;

public class ForcedMoveRuleTests : RuleBasedPieceRuleTestBase
{
    [Fact]
    public void Evaluate_applies_priority_when_predicate_matches()
    {
        var rule = new ForcedMoveRule(
            BaseRuleMock,
            ForcedMovePriority.EnPassant,
            (b, move) => move == Move2
        );

        var result = rule.Evaluate(Board, Origin, Piece).ToList();

        Move[] expected =
        [
            Move1,
            Move2 with
            {
                ForcedPriority = ForcedMovePriority.EnPassant,
            },
            Move3,
        ];
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Evaluate_does_not_apply_priority_when_predicate_fails()
    {
        var rule = new ForcedMoveRule(
            BaseRuleMock,
            ForcedMovePriority.UnderagePawn,
            (b, move) => false
        );

        var result = rule.Evaluate(Board, Origin, Piece).ToList();

        result.Should().BeEquivalentTo([Move1, Move2, Move3]);
    }

    [Fact]
    public void Evaluate_applies_priority_to_multiple_moves()
    {
        var rule = new ForcedMoveRule(
            BaseRuleMock,
            ForcedMovePriority.EnPassant,
            (b, move) => move == Move1 || move == Move3
        );

        var result = rule.Evaluate(Board, Origin, Piece).ToList();

        Move[] expected =
        [
            Move1 with
            {
                ForcedPriority = ForcedMovePriority.EnPassant,
            },
            Move2,
            Move3 with
            {
                ForcedPriority = ForcedMovePriority.EnPassant,
            },
        ];
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Evaluate_returns_empty_when_base_rule_returns_nothing()
    {
        var baseRule = Substitute.For<IPieceMovementRule>();
        baseRule.Evaluate(Board, Origin, Piece).Returns([]);

        var rule = new ForcedMoveRule(baseRule, ForcedMovePriority.UnderagePawn, (b, move) => true);

        var result = rule.Evaluate(Board, Origin, Piece).ToList();

        result.Should().BeEmpty();
    }
}
