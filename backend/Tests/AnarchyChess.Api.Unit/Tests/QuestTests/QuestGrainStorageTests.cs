using AnarchyChess.Api.QuestLogic;
using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.Quests.Grains;
using FluentAssertions;

namespace AnarchyChess.Api.Unit.Tests.QuestTests;

public class QuestGrainStorageTests
{
    private static QuestInstance CreateTestQuest() =>
        new(
            description: "test quest",
            difficulty: QuestDifficulty.Easy,
            target: 5,
            creationDate: DateOnly.FromDateTime(DateTime.UtcNow),
            shouldResetOnFailure: false,
            conditions: [],
            metrics: null
        );

    [Fact]
    public void CompleteQuest_increments_streak_and_disables_replace_when_quest_is_set()
    {
        var quest = CreateTestQuest();
        var storage = new QuestGrainStorage
        {
            Quest = quest,
            Streak = 2,
            CanReplace = true,
        };

        storage.CompleteQuest();

        storage.Streak.Should().Be(3);
        storage.CanReplace.Should().BeFalse();
    }

    [Fact]
    public void CompleteQuest_does_nothing_when_quest_is_null()
    {
        QuestGrainStorage storage = new() { Streak = 2, CanReplace = true };

        storage.CompleteQuest();

        storage.Streak.Should().Be(2);
        storage.CanReplace.Should().BeTrue();
    }

    [Fact]
    public void ResetProgressForNewQuest_sets_quest_and_resets_flags()
    {
        var oldQuest = CreateTestQuest();
        QuestGrainStorage storage = new()
        {
            Quest = oldQuest,
            CanReplace = false,
            RewardCollected = true,
        };

        var newQuest = CreateTestQuest();
        storage.ResetProgressForNewQuest(newQuest);

        storage.Quest.Should().Be(newQuest);
        storage.CanReplace.Should().BeTrue();
        storage.RewardCollected.Should().BeFalse();
    }

    [Fact]
    public void ResetStreakIfMissedDay_resets_streak_if_more_than_one_day_passed()
    {
        var quest = CreateTestQuest();
        var storage = new QuestGrainStorage() { Quest = quest, Streak = 5 };

        var today = quest.CreationDate.AddDays(2);
        storage.ResetStreakIfMissedDay(today);

        storage.Streak.Should().Be(0);
    }

    [Fact]
    public void ResetStreakIfMissedDay_does_not_reset_streak_if_one_day_or_less_passed()
    {
        var quest = CreateTestQuest();
        var storage = new QuestGrainStorage { Quest = quest, Streak = 5 };

        var today = quest.CreationDate.AddDays(1);
        storage.ResetStreakIfMissedDay(today);

        storage.Streak.Should().Be(5);
    }

    [Fact]
    public void MarkRewardCollected_sets_flag_to_true()
    {
        var storage = new QuestGrainStorage { RewardCollected = false };

        storage.MarkRewardCollected();

        storage.RewardCollected.Should().BeTrue();
    }
}
