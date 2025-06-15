using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.MovementBehaviours;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.GameLogicTests.MovementBehaviourTests;

public abstract class MovementBehaviourTestsBase
{
    protected static void TestMovementEvaluatesTo(
        IMovementBehaviour behaviour,
        Point from,
        IEnumerable<Point> expectedPoints,
        Piece? piece = null,
        IEnumerable<Point>? blockingPieces = null,
        ChessBoard? board = null
    )
    {
        board ??= new ChessBoard([]);
        if (piece is null)
        {
            piece = new PieceFaker().Generate();
            board.PlacePiece(from, piece);
        }

        foreach (var blockingPiece in blockingPieces ?? [])
            board.PlacePiece(blockingPiece, new PieceFaker().Generate());

        var result = behaviour.Evaluate(board, from, piece).ToList();

        result.Should().BeEquivalentTo(expectedPoints);
    }
}
