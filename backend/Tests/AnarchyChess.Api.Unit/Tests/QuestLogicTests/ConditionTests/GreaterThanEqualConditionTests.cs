using AnarchyChess.Api.QuestLogic.QuestConditions;
using AnarchyChess.Api.QuestLogic.QuestMetrics;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using NSubstitute;

namespace AnarchyChess.Api.Unit.Tests.QuestLogicTests.ConditionTests;

public class GreaterThanEqualConditionTests
{
    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(5, 10, false)]
    [InlineData(10, 10, true)]
    [InlineData(15, 10, true)]
    public void Evaluate_returns_true_if_above_or_equal_to_max(
        int innerProgress,
        int greaterThanEqualProgress,
        bool expectedResult
    )
    {
        var snapshot = new GameQuestSnapshotFaker().Generate();
        var inner = Substitute.For<IQuestMetric>();
        inner.Evaluate(snapshot).Returns(innerProgress);

        GreaterThanEqualCondition condition = new(inner, greaterThanEqualProgress);

        bool result = condition.Evaluate(snapshot);

        result.Should().Be(expectedResult);
    }
}
