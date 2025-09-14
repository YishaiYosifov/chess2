using System.Diagnostics.CodeAnalysis;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Quests.DTOs;
using Chess2.Api.Quests.Errors;
using Chess2.Api.Quests.Models;
using Chess2.Api.Quests.QuestDefinitions;
using Chess2.Api.Shared.Services;
using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Orleans.Concurrency;

namespace Chess2.Api.Quests.Grains;

[Alias("Chess2.Api.Quests.Grains.IQuestGrain")]
public interface IQuestGrain : IGrainWithStringKey
{
    [Alias("CollectRewardAsync")]
    Task<ErrorOr<int>> CollectRewardAsync();

    [Alias("GetGuestAsync")]
    Task<QuestDto> GetQuestAsync();

    [OneWay]
    [Alias("OnGameOverAsync")]
    Task OnGameOverAsync(GameQuestSnapshot snapshot);

    [Alias("ReplaceQuestAsync")]
    Task<ErrorOr<QuestDto>> ReplaceQuestAsync();
}

[GenerateSerializer]
[Alias("Chess2.Api.Quests.Grains.QuestGrainStorage")]
public class QuestGrainStorage
{
    [Id(0)]
    public QuestVariant? Quest { get; set; }

    [Id(1)]
    public int Progress { get; set; }

    [Id(2)]
    public DateOnly Date { get; set; }

    [Id(3)]
    public bool CanReplace { get; set; } = true;

    [Id(4)]
    public bool RewardCollected { get; set; }

    [Id(5)]
    public int Streak { get; set; }

    [MemberNotNullWhen(true, nameof(Quest))]
    public bool IsQuestCompleted => Quest is not null && Progress >= Quest.Target;

    public void CompleteQuest()
    {
        if (Quest is null)
            return;

        Progress = Quest.Target;
        Streak++;
        CanReplace = false;
    }

    public void ResetProgressForNewQuest(QuestVariant quest, DateOnly today)
    {
        Quest = quest;
        Progress = 0;
        Date = today;
        CanReplace = true;
        RewardCollected = false;
    }

    public void IncrementProgress(int amount)
    {
        if (Quest is null)
            return;

        Progress = Math.Min(Quest.Target, Progress + amount);
    }

    public void ResetStreakIfMissedDay(DateOnly today)
    {
        if (today.DayNumber - Date.DayNumber > 1)
            Streak = 0;
    }

    public void MarkRewardCollected() => RewardCollected = true;
}

public class QuestGrain(
    IEnumerable<IQuestDefinition> quests,
    UserManager<AuthedUser> userManager,
    TimeProvider timeProvider,
    IRandomProvider random
) : Grain<QuestGrainStorage>, IQuestGrain
{
    private readonly IEnumerable<IQuestDefinition> _quests = quests;
    private readonly UserManager<AuthedUser> _userManager = userManager;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly IRandomProvider _random = random;

    public async Task<QuestDto> GetQuestAsync()
    {
        var quest = GetOrSelectQuest();
        await WriteStateAsync();

        return ToDto(quest);
    }

    public async Task<ErrorOr<QuestDto>> ReplaceQuestAsync()
    {
        if (!State.CanReplace)
            return QuestErrors.CanotReplace;

        var quest = SelectNewQuest();
        State.CanReplace = false;
        await WriteStateAsync();

        return ToDto(quest);
    }

    public async Task<ErrorOr<int>> CollectRewardAsync()
    {
        if (State.RewardCollected || !State.IsQuestCompleted)
            return QuestErrors.NoRewardToCollect;

        var user = await _userManager.FindByIdAsync(this.GetPrimaryKeyString());
        if (user is null)
            return QuestErrors.NoRewardToCollect;

        user.QuestPoints += (int)State.Quest.Difficulty;
        await _userManager.UpdateAsync(user);

        State.MarkRewardCollected();
        await WriteStateAsync();

        return (int)State.Quest.Difficulty;
    }

    public async Task OnGameOverAsync(GameQuestSnapshot snapshot)
    {
        var quest = GetOrSelectQuest();
        if (State.Progress >= quest.Target)
            return;

        var progressMade = quest.Progressors.EvaluateProgressMade(snapshot);
        if (progressMade <= 0)
            return;

        State.IncrementProgress(progressMade);
        if (State.Progress >= quest.Target)
            State.CompleteQuest();

        await WriteStateAsync();
    }

    private QuestVariant GetOrSelectQuest()
    {
        var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().DateTime);
        if (State.Quest is not null && State.Date == today)
            return State.Quest;

        return SelectNewQuest();
    }

    private QuestVariant SelectNewQuest()
    {
        var difficulty = _random.NextWeighted(
            new Dictionary<int, QuestDifficulty>()
            {
                [50] = QuestDifficulty.Easy,
                [30] = QuestDifficulty.Medium,
                [20] = QuestDifficulty.Hard,
            }
        );
        var availableQuests = _quests
            .SelectMany(quest =>
                quest
                    .Variants.Where(variant =>
                        variant.Difficulty == difficulty
                        && variant.Description != State.Quest?.Description
                    )
                    .ToList()
            )
            .ToList();
        var quest = _random.NextItem(availableQuests);

        var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().DateTime);
        State.ResetStreakIfMissedDay(today);
        State.ResetProgressForNewQuest(quest, today);

        return quest;
    }

    private QuestDto ToDto(QuestVariant quest) =>
        new(
            Difficulty: quest.Difficulty,
            Description: quest.Description,
            Target: quest.Target,
            Progress: State.Progress,
            CanReplace: State.CanReplace,
            RewardCollected: State.RewardCollected,
            Streak: State.Streak
        );
}
