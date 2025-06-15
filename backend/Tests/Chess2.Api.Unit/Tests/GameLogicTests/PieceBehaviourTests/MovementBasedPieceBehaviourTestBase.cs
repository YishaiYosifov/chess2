using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.MovementBehaviours;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.GameLogicTests.PieceBehaviourTests;

public abstract class MovementBasedPieceBehaviourTestBase
{
    protected Point Origin { get; } = new(0, 0);
    protected List<Point> Destinations { get; } = [new(1, 1), new(2, 2), new(3, 3)];
    protected IMovementBehaviour MockMovement { get; } = Substitute.For<IMovementBehaviour>();

    protected MovementBasedPieceBehaviourTestBase()
    {
        MockMovement
            .Evaluate(Arg.Any<ChessBoard>(), Arg.Any<Point>(), Arg.Any<Piece>())
            .Returns(Destinations);
    }
}
