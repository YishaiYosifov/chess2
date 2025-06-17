using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.MovementBehaviours;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.GameLogicTests.PieceBehaviourTests;

public abstract class MovementBasedPieceBehaviourTestBase
{
    protected AlgebraicPoint Origin { get; } = new("a1");
    protected List<AlgebraicPoint> Destinations { get; } = [new("b2"), new("c3"), new("d4")];

    protected IMovementBehaviour MockMovement { get; } = Substitute.For<IMovementBehaviour>();

    protected MovementBasedPieceBehaviourTestBase()
    {
        MockMovement
            .Evaluate(Arg.Any<ChessBoard>(), Arg.Any<AlgebraicPoint>(), Arg.Any<Piece>())
            .Returns(Destinations);
    }
}
