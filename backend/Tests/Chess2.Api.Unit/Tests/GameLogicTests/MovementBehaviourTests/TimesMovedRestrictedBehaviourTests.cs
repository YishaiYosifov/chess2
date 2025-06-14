using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.MovementBehaviours;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.GameLogicTests.MovementBehaviourTests;

public class TimesMovedRestrictedBehaviourTests
{
    private readonly IMovementBehaviour _mockInnerBehaviour = Substitute.For<IMovementBehaviour>();

    [Fact]
    public void Evaluate_returns_empty_when_piece_has_moved_more_than_max_times()
    {
        var behaviour = new TimesMovedRestrictedBehaviour(_mockInnerBehaviour, maxTimesMoved: 2);
        var piece = new PieceFaker().RuleFor(x => x.TimesMoved, 3).Generate();
        var board = new ChessBoard(new() { [new Point(4, 4)] = piece });

        var result = behaviour.Evaluate(board, new Point(4, 4), piece);

        result.Should().BeEmpty();
        _mockInnerBehaviour
            .DidNotReceive()
            .Evaluate(Arg.Any<ChessBoard>(), Arg.Any<Point>(), Arg.Any<Piece>());
    }

    [Fact]
    public void Evaluate_delegates_to_inner_behaviour_when_times_moved_is_within_limit()
    {
        var expectedPoints = new[] { new Point(5, 5) };

        _mockInnerBehaviour
            .Evaluate(Arg.Any<ChessBoard>(), Arg.Any<Point>(), Arg.Any<Piece>())
            .Returns(expectedPoints);

        var behaviour = new TimesMovedRestrictedBehaviour(_mockInnerBehaviour, maxTimesMoved: 3);

        var piece = new PieceFaker().RuleFor(x => x.TimesMoved, 2).Generate();
        var board = new ChessBoard(new() { [new Point(4, 4)] = piece });

        var result = behaviour.Evaluate(board, new Point(4, 4), piece);

        result.Should().BeEquivalentTo(expectedPoints);
        _mockInnerBehaviour.Received(1).Evaluate(board, new Point(4, 4), piece);
    }

    [Fact]
    public void Evaluate_returns_empty_when_times_moved_equals_max_and_inner_returns_empty()
    {
        var piece = new PieceFaker().RuleFor(x => x.TimesMoved, 1).Generate();
        var board = new ChessBoard(new() { [new Point(2, 2)] = piece });

        _mockInnerBehaviour
            .Evaluate(Arg.Any<ChessBoard>(), Arg.Any<Point>(), Arg.Any<Piece>())
            .Returns([]);

        var behaviour = new TimesMovedRestrictedBehaviour(_mockInnerBehaviour, maxTimesMoved: 1);

        var result = behaviour.Evaluate(board, new Point(2, 2), piece);

        result.Should().BeEmpty();
        _mockInnerBehaviour.Received(1).Evaluate(board, new Point(2, 2), piece);
    }
}
