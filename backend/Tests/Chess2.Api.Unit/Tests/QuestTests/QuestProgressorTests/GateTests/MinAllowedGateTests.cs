using Chess2.Api.Quests.QuestProgressors;
using Chess2.Api.Quests.QuestProgressors.Gates;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.QuestTests.QuestProgressorTests.GateTests;

public class MinAllowedGateTests
{
    [Theory]
    [InlineData(0, 0, 1)] // progress == max: 1
    [InlineData(5, 10, 0)] // progress < max: 0
    [InlineData(10, 10, 1)] // progress == max: 1
    [InlineData(15, 10, 1)] // progress > max: 1
    public void EvaluateProgressMade_returns_1_if_above_or_equal_to_max(
        int innerProgress,
        int maxProgress,
        int expectedProgress
    )
    {
        var snapshot = new GameQuestSnapshotFaker().Generate();
        var inner = Substitute.For<IQuestProgressor>();
        inner.EvaluateProgressMade(snapshot).Returns(innerProgress);

        MinAllowedGate gate = new(inner, maxProgress);

        int progress = gate.EvaluateProgressMade(snapshot);

        progress.Should().Be(expectedProgress);
    }
}
