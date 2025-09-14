using Chess2.Api.Quests.QuestConditions;
using Chess2.Api.Quests.QuestMetrics;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.QuestTests.QuestProgressorTests.GateTests;

public class GreaterThanEqualGateTests
{
    [Theory]
    [InlineData(0, 0, 1)] // progress == value: 1
    [InlineData(5, 10, 0)] // progress < value: 0
    [InlineData(10, 10, 1)] // progress == value: 0
    [InlineData(15, 10, 1)] // progress > value: 1
    public void EvaluateProgressMade_returns_1_if_above_or_equal_to_max(
        int innerProgress,
        int greaterThanEqualProgress,
        int expectedProgress
    )
    {
        var snapshot = new GameQuestSnapshotFaker().Generate();
        var inner = Substitute.For<IQuestMetric>();
        inner.Evaluate(snapshot).Returns(innerProgress);

        GreaterThanEqualCondition gate = new(inner, greaterThanEqualProgress);

        int progress = gate.EvaluateProgressMade(snapshot);

        progress.Should().Be(expectedProgress);
    }
}
