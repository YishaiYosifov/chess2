using Chess2.Api.QuestLogic;
using Chess2.Api.QuestLogic.Models;
using Chess2.Api.QuestLogic.QuestConditions;
using Chess2.Api.QuestLogic.QuestMetrics;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.QuestLogicTests;

public class QuestInstanceTests
{
    [Fact]
    public void ApplySnapshot_increments_progress_by_one_when_all_conditions_true_and_no_metrics()
    {
        var condition = Substitute.For<IQuestCondition>();
        condition.Evaluate(Arg.Any<GameQuestSnapshot>()).Returns(true);

        QuestInstance quest = new(
            description: "test",
            difficulty: QuestDifficulty.Easy,
            target: 3,
            creationDate: DateOnly.FromDateTime(DateTime.UtcNow),
            conditions: [condition],
            metrics: null
        );

        var snapshot = new GameQuestSnapshotFaker().Generate();

        quest.ApplySnapshot(snapshot);

        quest.Progress.Should().Be(1);
        quest.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void ApplySnapshot_returns_zero_progress_when_any_condition_false()
    {
        var condition = Substitute.For<IQuestCondition>();
        condition.Evaluate(Arg.Any<GameQuestSnapshot>()).Returns(false);

        QuestInstance quest = new(
            description: "test",
            difficulty: QuestDifficulty.Medium,
            target: 5,
            creationDate: DateOnly.FromDateTime(DateTime.UtcNow),
            conditions: [condition],
            metrics: null
        );

        var snapshot = new GameQuestSnapshotFaker().Generate();

        quest.ApplySnapshot(snapshot);

        quest.Progress.Should().Be(0);
        quest.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void ApplySnapshot_returns_zero_when_any_condition_is_false()
    {
        var conditionTrue = Substitute.For<IQuestCondition>();
        conditionTrue.Evaluate(Arg.Any<GameQuestSnapshot>()).Returns(true);

        var conditionFalse = Substitute.For<IQuestCondition>();
        conditionFalse.Evaluate(Arg.Any<GameQuestSnapshot>()).Returns(false);

        QuestInstance quest = new(
            description: "test",
            difficulty: QuestDifficulty.Medium,
            target: 3,
            creationDate: DateOnly.FromDateTime(DateTime.UtcNow),
            conditions: [conditionTrue, conditionFalse],
            metrics: null
        );

        var snapshot = new GameQuestSnapshotFaker().Generate();

        quest.ApplySnapshot(snapshot);

        quest.Progress.Should().Be(0);
        quest.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void ApplySnapshot_sums_metric_values_when_conditions_true()
    {
        var condition = Substitute.For<IQuestCondition>();
        condition.Evaluate(Arg.Any<GameQuestSnapshot>()).Returns(true);

        var metric1 = Substitute.For<IQuestMetric>();
        metric1.Evaluate(Arg.Any<GameQuestSnapshot>()).Returns(2);

        var metric2 = Substitute.For<IQuestMetric>();
        metric2.Evaluate(Arg.Any<GameQuestSnapshot>()).Returns(3);

        QuestInstance quest = new(
            description: "test",
            difficulty: QuestDifficulty.Medium,
            target: 10,
            creationDate: DateOnly.FromDateTime(DateTime.UtcNow),
            conditions: [condition],
            metrics: [metric1, metric2]
        );

        var snapshot = new GameQuestSnapshotFaker().Generate();

        quest.ApplySnapshot(snapshot);

        quest.Progress.Should().Be(5);
        quest.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void ApplySnapshot_caps_progress_at_target()
    {
        var condition = Substitute.For<IQuestCondition>();
        condition.Evaluate(Arg.Any<GameQuestSnapshot>()).Returns(true);

        var metric = Substitute.For<IQuestMetric>();
        metric.Evaluate(Arg.Any<GameQuestSnapshot>()).Returns(10);

        QuestInstance quest = new(
            description: "test",
            difficulty: QuestDifficulty.Hard,
            target: 7,
            creationDate: DateOnly.FromDateTime(DateTime.UtcNow),
            conditions: [condition],
            metrics: [metric]
        );

        var snapshot = new GameQuestSnapshotFaker().Generate();

        quest.ApplySnapshot(snapshot);

        quest.Progress.Should().Be(7);
        quest.IsCompleted.Should().BeTrue();
    }
}
