using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.MovementBehaviours;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.GameLogicTests.PieceMovementRuleTests;

public abstract class MovementBasedPieceRulesTestBase
{
    protected AlgebraicPoint Origin { get; } = new("a1");
    protected AlgebraicPoint[] Destinations { get; }
    protected IMovementBehaviour[] MovementMocks { get; }

    protected MovementBasedPieceRulesTestBase()
    {
        var movementMock1 = Substitute.For<IMovementBehaviour>();
        AlgebraicPoint[] movement1Dests = [new("b2"), new("c3"), new("d4")];
        movementMock1
            .Evaluate(Arg.Any<ChessBoard>(), Origin, Arg.Any<Piece>())
            .Returns(movement1Dests);

        var movementMock2 = Substitute.For<IMovementBehaviour>();
        AlgebraicPoint[] movement2Dests = [new("e5"), new("f6"), new("g7")];
        movementMock2
            .Evaluate(Arg.Any<ChessBoard>(), Origin, Arg.Any<Piece>())
            .Returns(movement2Dests);

        MovementMocks = [movementMock1, movementMock2];
        Destinations = [.. movement1Dests, .. movement2Dests];
    }

    protected IEnumerable<Move> CreateMoves(Piece piece, params AlgebraicPoint[] toPoints) =>
        toPoints.Select(to => new Move(Origin, to, piece));
}
