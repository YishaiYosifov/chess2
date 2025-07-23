using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.PieceMovementRules;
using Chess2.Api.TestInfrastructure.Factories;
using FluentAssertions;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.GameLogicTests.PieceMovementRuleTests;

public class ForcedMoveRuleTests
{
    private readonly List<Move> _moves;
    private readonly AlgebraicPoint _origin = new("a1");
    private readonly Piece _piece = PieceFactory.White();
    private readonly Move _move1 = new(new("a1"), new("b2"), PieceFactory.White());
    private readonly Move _move2 = new(new("a1"), new("b3"), PieceFactory.White());
    private readonly Move _move3 = new(new("a1"), new("b4"), PieceFactory.White());
    private readonly ChessBoard _board = new();

    private readonly IPieceMovementRule _baseRuleMock = Substitute.For<IPieceMovementRule>();

    public ForcedMoveRuleTests()
    {
        _board.PlacePiece(_origin, _piece);
        _moves = [_move1, _move2, _move3];
        _baseRuleMock
            .Evaluate(Arg.Any<ChessBoard>(), Arg.Any<AlgebraicPoint>(), Arg.Any<Piece>())
            .Returns(_moves);
    }

    [Fact]
    public void Evaluate_applies_priority_when_predicate_matches()
    {
        var rule = new ForcedMoveRule(
            _baseRuleMock,
            ForcedMovePriority.EnPassant,
            (b, move) => move == _move2
        );

        var result = rule.Evaluate(_board, _origin, _piece).ToList();

        Move[] expected =
        [
            _move1,
            _move2 with
            {
                ForcedPriority = ForcedMovePriority.EnPassant,
            },
            _move3,
        ];
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Evaluate_does_not_apply_priority_when_predicate_fails()
    {
        var rule = new ForcedMoveRule(
            _baseRuleMock,
            ForcedMovePriority.ChildPawn,
            (b, move) => false
        );

        var result = rule.Evaluate(_board, _origin, _piece).ToList();

        result.Should().BeEquivalentTo([_move1, _move2, _move3]);
    }

    [Fact]
    public void Evaluate_applies_priority_to_multiple_moves()
    {
        var rule = new ForcedMoveRule(
            _baseRuleMock,
            ForcedMovePriority.EnPassant,
            (b, move) => move == _move1 || move == _move3
        );

        var result = rule.Evaluate(_board, _origin, _piece).ToList();

        Move[] expected =
        [
            _move1 with
            {
                ForcedPriority = ForcedMovePriority.EnPassant,
            },
            _move2,
            _move3 with
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
        baseRule.Evaluate(_board, _origin, _piece).Returns([]);

        var rule = new ForcedMoveRule(baseRule, ForcedMovePriority.ChildPawn, (b, move) => true);

        var result = rule.Evaluate(_board, _origin, _piece).ToList();

        result.Should().BeEmpty();
    }
}
