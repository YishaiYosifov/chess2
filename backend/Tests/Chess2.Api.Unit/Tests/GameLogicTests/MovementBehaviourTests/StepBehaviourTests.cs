using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.MovementBehaviours;
using Chess2.Api.TestInfrastructure.Factories;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.GameLogicTests.MovementBehaviourTests;

public class StepBehaviourTests : MovementBehaviourTestsBase
{
    [Theory]
    [ClassData(typeof(StepBehaviourTestData))]
    public void StepBehaviour_evaluates_expected_position(
        AlgebraicPoint from,
        Offset offset,
        IEnumerable<AlgebraicPoint> expectedPoints,
        IEnumerable<AlgebraicPoint> blockingPieces
    )
    {
        var piece = PieceFactory.White();
        var board = new ChessBoard([]);
        board.PlacePiece(from, piece);
        foreach (var p in blockingPieces ?? [])
            board.PlacePiece(p, PieceFactory.Black());

        var behaviour = new StepBehaviour(offset);
        var result = behaviour.Evaluate(board, from, piece).ToList();

        result.Should().BeEquivalentTo(expectedPoints);
    }
}

public class StepBehaviourTestData
    : TheoryData<AlgebraicPoint, Offset, IEnumerable<AlgebraicPoint>, IEnumerable<AlgebraicPoint>>
{
    public StepBehaviourTestData()
    {
        // Within boundaries: step right by (1,0)
        Add(new("d4"), new(1, 0), [new(new("e4"))], []);

        // Within boundaries: step up by (0,1)
        Add(new("d4"), new(0, 1), [new("d5")], []);

        // Out of boundaries: step left by (-1,0) from "a1"
        Add(new("a1"), new(-1, 0), [], []);

        // Out of boundaries: step down by (0,-1) from "a1"
        Add(new("a1"), new(0, -1), [], []);

        // Within boundaries: step diagonally (1,1)
        Add(new("f6"), new(1, 1), [new("g7")], []);

        // Blocking piece at destination: should still yield the occupied square
        Add(new(new("d4")), new(1, 0), [new("e4")], [new("e4")]);

        // Block piece at a diagonal
        Add(new("c3"), new(1, 1), [new("d4")], [new("d4")]);
    }
}
