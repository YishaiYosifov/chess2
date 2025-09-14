using Chess2.Api.Quests.QuestProgressors;
using Chess2.Api.Quests.QuestProgressors.Gates;
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

        var left = Substitute.For<IQuestProgressor>();
        left.EvaluateProgressMade(snapshot).Returns(leftProgress);

        var right = Substitute.For<IQuestProgressor>();
        right.EvaluateProgressMade(snapshot).Returns(rightProgress);

        AndGate gate = new(left, right);

        int progress = gate.EvaluateProgressMade(snapshot);

        progress.Should().Be(expectedProgress);
    }

    [Fact]
    public void EvaluateProgressMade_does_not_evaluate_right_if_left_is_zero()
    {
        var snapshot = new GameQuestSnapshotFaker().Generate();

        var left = Substitute.For<IQuestProgressor>();
        left.EvaluateProgressMade(snapshot).Returns(0);

        var right = Substitute.For<IQuestProgressor>();

        AndGate gate = new(left, right);

        int progress = gate.EvaluateProgressMade(snapshot);

        progress.Should().Be(0);
        right.DidNotReceive().EvaluateProgressMade(snapshot);
    }
}
