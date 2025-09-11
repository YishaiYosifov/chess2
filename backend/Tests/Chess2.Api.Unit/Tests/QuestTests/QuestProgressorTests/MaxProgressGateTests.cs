using Chess2.Api.GameLogic.Models;
using Chess2.Api.Quests.QuestProgressors;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.QuestTests.QuestProgressorTests;

public class MaxProgressGateTests
{
    [Theory]
    [InlineData(0, 0, 1)] // progress == max: 1
    [InlineData(5, 10, 1)] // progress < max: 1
    [InlineData(10, 10, 1)] // progress == max: 1
    [InlineData(15, 10, 0)] // progress > max: 0
    public void EvaluateProgressMade_returns_1_if_inner_progress_is_at_or_below_max_and_0_if_above(
        int innerProgress,
        int maxProgress,
        int expectedProgress
    )
    {
        var snapshot = new GameStateFaker().Generate();
        var inner = Substitute.For<IQuestProgressor>();
        inner.EvaluateProgressMade(snapshot, GameColor.White).Returns(innerProgress);

        MaxProgressGate gate = new(inner, maxProgress);

        int progress = gate.EvaluateProgressMade(snapshot, GameColor.White);

        progress.Should().Be(expectedProgress);
    }
}
