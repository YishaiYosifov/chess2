using AnarchyChess.Api.QuestLogic.QuestConditions;
using AnarchyChess.Api.QuestLogic.QuestMetrics;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AwesomeAssertions;
using NSubstitute;

namespace AnarchyChess.Api.Unit.Tests.QuestLogicTests.ConditionTests;

public class LessThanEqualGateTests
{
    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(5, 10, true)]
    [InlineData(10, 10, true)]
    [InlineData(15, 10, false)]
    public void Evaluate_returns_true_if_bellow_or_equal_to_value(
        int innerProgress,
        int lessThanEqualProgress,
        bool expectedResult
    )
    {
        var snapshot = new GameQuestSnapshotFaker().Generate();
        var inner = Substitute.For<IQuestMetric>();
        inner.Evaluate(snapshot).Returns(innerProgress);

        LessThanEqualCondition condition = new(inner, lessThanEqualProgress);

        bool result = condition.Evaluate(snapshot);

        result.Should().Be(expectedResult);
    }
}
