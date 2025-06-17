using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.MovementBehaviours;
using Chess2.Api.TestInfrastructure.Factories;
using FluentAssertions;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.GameLogicTests.MovementBehaviourTests;

public class TimesMovedRestrictedBehaviourTests
{
    private readonly IMovementBehaviour _mockInnerBehaviour = Substitute.For<IMovementBehaviour>();
    private readonly IEnumerable<AlgebraicPoint> _innerPoints =
    [
        new AlgebraicPoint("b5"),
        new AlgebraicPoint("c6"),
        new AlgebraicPoint("d7"),
        new AlgebraicPoint("d7"),
        new AlgebraicPoint("b7"),
    ];

    public TimesMovedRestrictedBehaviourTests()
    {
        _mockInnerBehaviour
            .Evaluate(Arg.Any<ChessBoard>(), Arg.Any<AlgebraicPoint>(), Arg.Any<Piece>())
            .Returns(_innerPoints);
    }

    [Theory]
    [InlineData(0, true)] // allowed: 0 <= 3
    [InlineData(3, true)] // allowed: 3 <= 3
    [InlineData(4, false)] // disallowed: 4 > 3
    public void RestrictsBasedOnTimesMoved(int timesMoved, bool shouldMove)
    {
        var restricted = new TimesMovedRestrictedBehaviour(_mockInnerBehaviour, maxTimesMoved: 3);
        var start = new AlgebraicPoint("a1");
        var piece = PieceFactory.White(timesMoved: timesMoved);
        var board = new ChessBoard();
        board.PlacePiece(start, piece);

        var result = restricted.Evaluate(board, start, piece).ToList();

        result.Should().BeEquivalentTo(shouldMove ? _innerPoints : []);
    }
}
