using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameLogic.MovementBehaviours;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AwesomeAssertions;
using NSubstitute;

namespace AnarchyChess.Api.Unit.Tests.GameLogicTests.MovementBehaviourTests;

public class ConditionalBehaviourTests
{
    private readonly IMovementBehaviour _mockTrueBranch = Substitute.For<IMovementBehaviour>();
    private readonly IEnumerable<AlgebraicPoint> _trueBranchPoints =
    [
        new AlgebraicPoint("t1"),
        new AlgebraicPoint("r2"),
        new AlgebraicPoint("u3"),
        new AlgebraicPoint("e4"),
    ];

    private readonly IMovementBehaviour _mockFalseBranch = Substitute.For<IMovementBehaviour>();
    private readonly IEnumerable<AlgebraicPoint> _falseBranchPoints =
    [
        new AlgebraicPoint("f1"),
        new AlgebraicPoint("a2"),
        new AlgebraicPoint("l3"),
        new AlgebraicPoint("s4"),
        new AlgebraicPoint("e5"),
    ];

    private readonly Piece _piece = PieceFactory.White();
    private readonly AlgebraicPoint _from = new("a1");
    private readonly ChessBoard _board = new();

    public ConditionalBehaviourTests()
    {
        _mockTrueBranch
            .Evaluate(_board, Arg.Any<AlgebraicPoint>(), Arg.Any<Piece>())
            .Returns(_trueBranchPoints);

        _mockFalseBranch
            .Evaluate(_board, Arg.Any<AlgebraicPoint>(), Arg.Any<Piece>())
            .Returns(_falseBranchPoints);

        _board.PlacePiece(_from, _piece);
    }

    [Fact]
    public void ConditionalBehaviour_uses_true_branch_when_condition_is_true()
    {
        var behaviour = new ConditionalBehaviour(
            (b, p, m) => true,
            _mockTrueBranch,
            _mockFalseBranch
        );

        var result = behaviour.Evaluate(_board, _from, _piece).ToList();

        result.Should().BeEquivalentTo(_trueBranchPoints);
    }

    [Fact]
    public void ConditionalBehaviour_uses_false_branch_when_condition_is_false()
    {
        var behaviour = new ConditionalBehaviour(
            (b, p, m) => false,
            _mockTrueBranch,
            _mockFalseBranch
        );

        var result = behaviour.Evaluate(_board, _from, _piece).ToList();

        result.Should().BeEquivalentTo(_falseBranchPoints);
    }

    [Fact]
    public void ConditionalBehaviour_returns_empty_when_condition_is_true_and_true_branch_is_null()
    {
        var behaviour = new ConditionalBehaviour((b, p, m) => true, null, _mockFalseBranch);

        var result = behaviour.Evaluate(_board, _from, _piece).ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public void ConditionalBehaviour_returns_empty_when_condition_is_false_and_false_branch_is_null()
    {
        var behaviour = new ConditionalBehaviour((b, p, m) => false, _mockFalseBranch, null);

        var result = behaviour.Evaluate(_board, _from, _piece).ToList();

        result.Should().BeEmpty();
    }
}
