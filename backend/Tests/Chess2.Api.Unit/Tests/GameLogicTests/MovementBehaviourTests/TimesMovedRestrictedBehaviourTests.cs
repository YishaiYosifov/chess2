using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.MovementBehaviours;
using Chess2.Api.TestInfrastructure.Fakes;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.GameLogicTests.MovementBehaviourTests;

public class TimesMovedRestrictedBehaviourTests : MovementBehaviourTestsBase
{
    private readonly IMovementBehaviour _mockInnerBehaviour = Substitute.For<IMovementBehaviour>();
    private readonly IEnumerable<Point> _innerPoints =
    [
        new Point(1, 3),
        new Point(2, 2),
        new Point(3, 1),
        new Point(3, 1),
        new Point(1, 1),
    ];

    public TimesMovedRestrictedBehaviourTests()
    {
        _mockInnerBehaviour
            .Evaluate(Arg.Any<ChessBoard>(), Arg.Any<Point>(), Arg.Any<Piece>())
            .Returns(_innerPoints);
    }

    [Theory]
    [InlineData(0, true)] // allowed: 0 <= 3
    [InlineData(3, true)] // allowed: 3 <= 3
    [InlineData(4, false)] // disallowed: 4 > 3
    public void RestrictsBasedOnTimesMoved(int timesMoved, bool shouldMove)
    {
        var restricted = new TimesMovedRestrictedBehaviour(_mockInnerBehaviour, maxTimesMoved: 3);
        var start = new Point(0, 0);
        var piece = new PieceFaker().RuleFor(x => x.TimesMoved, timesMoved).Generate();

        TestMovementEvaluatesTo(restricted, start, shouldMove ? _innerPoints : [], piece);
    }
}
