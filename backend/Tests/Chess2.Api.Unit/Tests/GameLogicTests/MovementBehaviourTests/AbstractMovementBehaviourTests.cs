using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.MovementBehaviours;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.GameLogicTests.MovementBehaviourTests;

public abstract class AbstractMovementBehaviourTests
{
    protected abstract IMovementBehaviour CreateBehaviour(Point offset);

    protected void TestBehaviour(Point from, Point offset, IEnumerable<Point> expectedPoints)
    {
        var piece = new PieceFaker().Generate();
        var board = new ChessBoard(new() { [from] = piece });
        var behaviour = CreateBehaviour(offset);

        var result = behaviour.Evaluate(board, from, piece).ToList();

        result.Should().BeEquivalentTo(expectedPoints);
    }
}
