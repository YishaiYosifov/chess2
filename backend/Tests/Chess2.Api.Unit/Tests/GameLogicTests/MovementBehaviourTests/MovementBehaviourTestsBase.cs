using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.MovementBehaviours;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.GameLogicTests.MovementBehaviourTests;

public abstract class MovementBehaviourTestsBase
{
    protected static void TestMovementEvaluatesTo(
        IMovementBehaviour behaviour,
        ChessBoard board,
        Point from,
        IEnumerable<Point> expectedPoints
    )
    {
        if (!board.TryGetPieceAt(from, out var piece))
            throw new InvalidOperationException("A piece should exist at the from point");

        var result = behaviour.Evaluate(board, from, piece).ToList();

        result.Should().BeEquivalentTo(expectedPoints);
    }
}
