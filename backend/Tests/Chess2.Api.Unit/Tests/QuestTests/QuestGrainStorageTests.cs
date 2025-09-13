using Chess2.Api.Quests.Grains;
using Chess2.Api.Quests.Models;
using Chess2.Api.Quests.QuestProgressors;
using FluentAssertions;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.QuestTests;

public class QuestGrainStorageTests
{
    private readonly QuestVariant SampleQuest = new(
        Progressor: Substitute.For<IQuestProgressor>(),
        Description: "test quest",
        Target: 5,
        Difficulty: QuestDifficulty.Easy
    );

    [Fact]
    public void IsQuestCompleted_returns_false_when_quest_is_null()
    {
        QuestGrainStorage storage = new();

        storage.IsQuestCompleted.Should().BeFalse();
    }

    [Fact]
    public void IsQuestCompleted_returns_false_when_progress_less_than_target()
    {
        QuestGrainStorage storage = new() { Quest = SampleQuest, Progress = 3 };

        storage.IsQuestCompleted.Should().BeFalse();
    }

    [Fact]
    public void IsQuestCompleted_returns_true_when_progress_equals_target()
    {
        QuestGrainStorage storage = new() { Quest = SampleQuest, Progress = 5 };

        storage.IsQuestCompleted.Should().BeTrue();
    }

    [Fact]
    public void CompleteQuest_sets_progress_to_target_and_increments_streak_and_disables_replace()
    {
        QuestGrainStorage storage = new()
        {
            Quest = SampleQuest,
            Progress = 2,
            Streak = 1,
            CanReplace = true,
        };

        storage.CompleteQuest();

        storage.Progress.Should().Be(SampleQuest.Target);
        storage.Streak.Should().Be(2);
        storage.CanReplace.Should().BeFalse();
    }

    [Fact]
    public void CompleteQuest_does_nothing_when_quest_is_null()
    {
        QuestGrainStorage storage = new()
        {
            Progress = 2,
            Streak = 1,
            CanReplace = true,
        };

        storage.CompleteQuest();

        storage.Progress.Should().Be(2);
        storage.Streak.Should().Be(1);
        storage.CanReplace.Should().BeTrue();
    }

    [Fact]
    public void ResetProgressForNewQuest_resets_all_properties()
    {
        QuestGrainStorage storage = new()
        {
            Quest = SampleQuest,
            Progress = 3,
            Date = new DateOnly(2025, 9, 12),
            CanReplace = false,
            RewardCollected = true,
        };

        QuestVariant newQuest = new(
            Progressor: Substitute.For<IQuestProgressor>(),
            Description: "New Quest",
            Target: 10,
            Difficulty: QuestDifficulty.Medium
        );
        var today = new DateOnly(2025, 9, 13);

        storage.ResetProgressForNewQuest(newQuest, today);

        storage.Quest.Should().Be(newQuest);
        storage.Progress.Should().Be(0);
        storage.Date.Should().Be(today);
        storage.CanReplace.Should().BeTrue();
        storage.RewardCollected.Should().BeFalse();
    }

    [Fact]
    public void IncrementProgress_increases_progress_but_not_above_target()
    {
        QuestGrainStorage storage = new() { Quest = SampleQuest, Progress = 3 };

        storage.IncrementProgress(2);
        storage.Progress.Should().Be(SampleQuest.Target);

        storage.IncrementProgress(5);
        storage.Progress.Should().Be(SampleQuest.Target);
    }

    [Fact]
    public void IncrementProgress_does_nothing_when_quest_is_null()
    {
        QuestGrainStorage storage = new() { Progress = 3 };

        storage.IncrementProgress(2);

        storage.Progress.Should().Be(3);
    }

    [Fact]
    public void ResetStreakIfMissedDay_resets_streak_when_more_than_one_day_passed()
    {
        QuestGrainStorage storage = new() { Date = new DateOnly(2025, 9, 10), Streak = 3 };

        storage.ResetStreakIfMissedDay(new DateOnly(2025, 9, 12));

        storage.Streak.Should().Be(0);
    }

    [Fact]
    public void ResetStreakIfMissedDay_does_not_reset_streak_if_only_one_day_passed()
    {
        QuestGrainStorage storage = new() { Date = new DateOnly(2025, 9, 10), Streak = 3 };

        storage.ResetStreakIfMissedDay(new DateOnly(2025, 9, 11));

        storage.Streak.Should().Be(3);
    }

    [Fact]
    public void MarkRewardCollected_sets_reward_collected_to_true()
    {
        QuestGrainStorage storage = new() { RewardCollected = false };

        storage.MarkRewardCollected();

        storage.RewardCollected.Should().BeTrue();
    }
}
