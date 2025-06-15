using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.MovementBehaviours;
using Chess2.Api.TestInfrastructure.Utils;

namespace Chess2.Api.Unit.Tests.GameLogicTests.MovementBehaviourTests;

public class SlideBehaviourTests : MovementBehaviourTestsBase
{
    [Theory]
    [ClassData(typeof(SlideBehaviourTestData))]
    public void SlideBehaviour_evaluates_expected_position(
        Point from,
        Point offset,
        IEnumerable<Point> expectedPoints,
        IEnumerable<Point> blockingPieces
    )
    {
        var board = BoardUtils.CreateBoardWithPieces(from, blockingPieces: blockingPieces);
        TestMovementEvaluatesTo(new SlideBehaviour(offset), board, from, expectedPoints);
    }
}

public class SlideBehaviourTestData
    : TheoryData<Point, Point, IEnumerable<Point>, IEnumerable<Point>>
{
    public SlideBehaviourTestData()
    {
        // Slide horizontally to the right from (3,3)
        Add(
            new(3, 3),
            new(1, 0),
            [new(4, 3), new(5, 3), new(6, 3), new(7, 3), new(8, 3), new(9, 3)],
            []
        );

        // Slide vertically upward from (3,3)
        Add(
            new(3, 3),
            new(0, 1),
            [new(3, 4), new(3, 5), new(3, 6), new(3, 7), new(3, 8), new(3, 9)],
            []
        );

        // Slide diagonally up-right from (4,4)
        Add(new(4, 4), new(-1, 1), [new(3, 5), new(2, 6), new(1, 7), new(0, 8)], []);

        // Slide horizontally to the left from (3,3)
        Add(new(3, 3), new(-1, 0), [new(2, 3), new(1, 3), new(0, 3)], []);

        // Slide diagonally down-right from (5,5)
        Add(new(5, 5), new(1, 1), [new(6, 6), new(7, 7), new(8, 8), new(9, 9)], []);

        // Slide diagonally up-left from (6,6)
        Add(
            new(6, 6),
            new(-1, -1),
            [new(5, 5), new(4, 4), new(3, 3), new(2, 2), new(1, 1), new(0, 0)],
            []
        );

        // Slide diagonally down-right from (1,1)
        Add(
            new(1, 1),
            new(1, 1),
            [
                new(2, 2),
                new(3, 3),
                new(4, 4),
                new(5, 5),
                new(6, 6),
                new(7, 7),
                new(8, 8),
                new(9, 9),
            ],
            []
        );

        // Slide vertically downward from (5,5)
        Add(new(5, 5), new(0, -1), [new(5, 4), new(5, 3), new(5, 2), new(5, 1), new(5, 0)], []);

        // Edge case: rightward slide from (9,9) goes out of bounds immediately
        Add(new(9, 9), new(1, 0), [], []);

        // Edge case: leftward slide from (0,0) goes out of bounds immediately
        Add(new(0, 0), new(-1, 0), [], []);

        // Edge case: upward slide from top row (5,9) goes out of bounds immediately
        Add(new(5, 9), new(0, 1), [], []);

        // Edge case: downward slide from bottom row (5,0) goes out of bounds immediately
        Add(new(5, 0), new(0, -1), [], []);

        // Slide stops at blocker at (6,3)
        Add(new(3, 3), new(1, 0), [new(4, 3), new(5, 3), new(6, 3)], [new(6, 3)]);

        // Slide stops at blocker at (2,6)
        Add(new(4, 4), new(-1, 1), [new(3, 5), new(2, 6)], [new(2, 6)]);

        // Slide stops at blocker at (7,7)
        Add(new(5, 5), new(1, 1), [new(6, 6), new(7, 7)], [new(7, 7)]);

        // Slide stops at blocker at (3,4)
        Add(new(3, 3), new(0, 1), [new(3, 4)], [new(3, 4)]);

        // Slide stops at blocker at (0,3)
        Add(new(3, 3), new(-1, 0), [new(2, 3), new(1, 3), new(0, 3)], [new(0, 3)]);
    }
}
