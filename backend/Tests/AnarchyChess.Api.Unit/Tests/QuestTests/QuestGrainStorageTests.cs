using AnarchyChess.Api.QuestLogic;
using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.Quests.Grains;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AwesomeAssertions;

namespace AnarchyChess.Api.Unit.Tests.QuestTests;

public class QuestGrainStorageTests
{
    private static QuestInstance CreateTestQuest(int daysAgo = 0) =>
        new(
            description: "test quest",
            difficulty: QuestDifficulty.Easy,
            target: 1,
            creationDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-daysAgo)),
            shouldResetOnFailure: false,
            conditions: [],
            metrics: null
        );

    [Fact]
    public void CompleteQuest_disallows_replace_when_quest_is_set()
    {
        var quest = CreateTestQuest();
        QuestGrainStorage storage = new()
        {
            Quest = quest,
            Streak = 2,
            CanReplace = true,
        };

        storage.CompleteQuest();

        storage.Streak.Should().Be(2);
        storage.CanReplace.Should().BeFalse();
    }

    [Fact]
    public void CompleteQuest_does_nothing_when_quest_is_null()
    {
        QuestGrainStorage storage = new() { CanReplace = true };

        storage.CompleteQuest();

        storage.CanReplace.Should().BeTrue();
    }

    [Fact]
    public void SelectNewQuest_sets_quest_and_resets_flags()
    {
        var oldQuest = CreateTestQuest();
        QuestGrainStorage storage = new()
        {
            Quest = oldQuest,
            CanReplace = false,
            RewardCollected = true,
            Streak = 3,
        };

        var newQuest = CreateTestQuest();
        storage.SelectNewQuest(newQuest);

        storage.Quest.Should().Be(newQuest);
        storage.CanReplace.Should().BeTrue();
        storage.RewardCollected.Should().BeFalse();
    }

    [Fact]
    public void SelectNewQuest_resets_streak_if_last_quest_was_not_completed()
    {
        var oldQuest = CreateTestQuest();
        QuestGrainStorage storage = new() { Quest = oldQuest, Streak = 5 };

        var newQuest = CreateTestQuest();
        storage.SelectNewQuest(newQuest);

        storage.Streak.Should().Be(0);
    }

    [Fact]
    public void SelectNewQuest_resets_streak_if_two_or_more_days_passed()
    {
        var oldQuest = CreateTestQuest(daysAgo: 2);
        oldQuest.ApplySnapshot(new GameQuestSnapshotFaker().Generate());
        QuestGrainStorage storage = new() { Quest = oldQuest, Streak = 5 };

        var newQuest = CreateTestQuest();
        storage.SelectNewQuest(newQuest);

        storage.Streak.Should().Be(0);
    }

    [Fact]
    public void SelectNewQuest_preserves_streak_if_one_day_passed_and_completed()
    {
        var oldQuest = CreateTestQuest(daysAgo: 1);
        oldQuest.ApplySnapshot(new GameQuestSnapshotFaker().Generate());
        QuestGrainStorage storage = new() { Quest = oldQuest, Streak = 5 };

        var newQuest = CreateTestQuest();
        storage.SelectNewQuest(newQuest);

        storage.Streak.Should().Be(5);
    }

    [Fact]
    public void MarkRewardCollected_sets_flag_to_true_and_increments_streak()
    {
        QuestGrainStorage storage = new() { RewardCollected = false, Streak = 5 };

        storage.MarkRewardCollected();

        storage.RewardCollected.Should().BeTrue();
        storage.Streak.Should().Be(6);
    }
}
