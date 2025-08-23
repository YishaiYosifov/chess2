using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.PieceMovementRules;
using FluentAssertions;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.GameLogicTests.PieceMovementRuleTests;

public class ForcedMoveRuleTests : RuleBasedPieceRuleTestBase
{
    [Theory]
    [InlineData(1)]
    [InlineData(0, 3)]
    public void Evaluate_applies_priority_correctly(params int[] moveIndices)
    {
        ForcedMoveRule rule = new(
            ForcedMovePriority.EnPassant,
            (b, m) => moveIndices.Contains(Array.IndexOf(Moves, m)),
            RuleMocks
        );

        var result = rule.Evaluate(Board, Origin, Piece).ToList();

        var expected = Moves
            .Select(
                (m, i) =>
                    moveIndices.Contains(i)
                        ? m with
                        {
                            ForcedPriority = ForcedMovePriority.EnPassant,
                        }
                        : m
            )
            .ToArray();

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Evaluate_does_not_apply_priority_when_predicate_fails()
    {
        ForcedMoveRule rule = new(ForcedMovePriority.UnderagePawn, (b, move) => false, RuleMocks);

        var result = rule.Evaluate(Board, Origin, Piece).ToList();

        result.Should().BeEquivalentTo(Moves);
    }

    [Fact]
    public void Evaluate_applies_priority_to_multiple_moves()
    {
        ForcedMoveRule rule = new(
            ForcedMovePriority.EnPassant,
            (b, move) => move == Moves[0] || move == Moves[3],
            RuleMocks
        );

        var result = rule.Evaluate(Board, Origin, Piece).ToList();

        Move[] expected =
        [
            Moves[0] with
            {
                ForcedPriority = ForcedMovePriority.EnPassant,
            },
            Moves[1],
            Moves[2],
            Moves[3] with
            {
                ForcedPriority = ForcedMovePriority.EnPassant,
            },
            .. Moves[4..],
        ];
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Evaluate_returns_empty_when_base_rule_returns_nothing()
    {
        var baseRule = Substitute.For<IPieceMovementRule>();
        baseRule.Evaluate(Board, Origin, Piece).Returns([]);

        ForcedMoveRule rule = new(ForcedMovePriority.UnderagePawn, (b, move) => true, baseRule);

        var result = rule.Evaluate(Board, Origin, Piece).ToList();

        result.Should().BeEmpty();
    }
}
