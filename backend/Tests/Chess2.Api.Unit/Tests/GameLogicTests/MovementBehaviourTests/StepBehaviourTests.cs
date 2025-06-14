using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.MovementBehaviours;

namespace Chess2.Api.Unit.Tests.GameLogicTests.MovementBehaviourTests;

public class StepBehaviourTests : AbstractMovementBehaviourTests
{
    [Theory]
    [ClassData(typeof(StepBehaviourTestData))]
    public void StepBehaviour_evaluates_expected_position(
        Point from,
        Point offset,
        IEnumerable<Point> expectedPoints
    ) => TestBehaviour(from, offset, expectedPoints);

    protected override IMovementBehaviour CreateBehaviour(Point offset) =>
        new StepBehaviour(offset);
}

public class StepBehaviourTestData : TheoryData<Point, Point, IEnumerable<Point>>
{
    public StepBehaviourTestData()
    {
        // Within boundaries: step right by (1,0)
        Add(new Point(3, 3), new Point(1, 0), [new Point(4, 3)]);

        // Within boundaries: step up by (0,1)
        Add(new Point(3, 3), new Point(0, 1), [new Point(3, 4)]);

        // Out of boundaries: step left by (-1,0) from (0,0)
        Add(new Point(0, 0), new Point(-1, 0), []);

        // Out of boundaries: step down by (0,-1) from (0,0)
        Add(new Point(0, 0), new Point(0, -1), []);

        // Within boundaries: step diagonally (1,1)
        Add(new Point(5, 5), new Point(1, 1), [new Point(6, 6)]);
    }
}
