using Chess2.Api.Quests.QuestConditions;
using Chess2.Api.Quests.QuestMetrics;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.QuestTests.QuestProgressorTests.GateTests;

public class AndGateTests
{
    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(1, 0, 0)]
    [InlineData(0, 1, 0)]
    [InlineData(1, 1, 1)]
    [InlineData(2, 3, 1)]
    public void EvaluateProgressMade_returns_expected_result(
        int leftProgress,
        int rightProgress,
        int expectedProgress
    )
    {
        var snapshot = new GameQuestSnapshotFaker().Generate();

        var left = Substitute.For<IQuestMetric>();
        left.Evaluate(snapshot).Returns(leftProgress);

        var right = Substitute.For<IQuestMetric>();
        right.Evaluate(snapshot).Returns(rightProgress);

        AndGate gate = new(left, right);

        int progress = gate.EvaluateProgressMade(snapshot);

        progress.Should().Be(expectedProgress);
    }

    [Fact]
    public void EvaluateProgressMade_does_not_evaluate_right_if_left_is_zero()
    {
        var snapshot = new GameQuestSnapshotFaker().Generate();

        var left = Substitute.For<IQuestMetric>();
        left.Evaluate(snapshot).Returns(0);

        var right = Substitute.For<IQuestMetric>();

        AndGate gate = new(left, right);

        int progress = gate.EvaluateProgressMade(snapshot);

        progress.Should().Be(0);
        right.DidNotReceive().Evaluate(snapshot);
    }
}
