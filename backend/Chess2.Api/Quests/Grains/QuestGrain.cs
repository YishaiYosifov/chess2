using Chess2.Api.Profile.Entities;
using Chess2.Api.Profile.Errors;
using Chess2.Api.QuestLogic;
using Chess2.Api.QuestLogic.Models;
using Chess2.Api.QuestLogic.QuestDefinitions;
using Chess2.Api.Quests.DTOs;
using Chess2.Api.Quests.Errors;
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
    public QuestInstance? Quest { get; set; }

    [Id(3)]
    public bool CanReplace { get; set; } = true;

    [Id(4)]
    public bool RewardCollected { get; set; }

    [Id(5)]
    public int Streak { get; set; }

    public void CompleteQuest()
    {
        if (Quest is null)
            return;

        Streak++;
        CanReplace = false;
    }

    public void ResetProgressForNewQuest(QuestInstance quest)
    {
        Quest = quest;
        CanReplace = true;
        RewardCollected = false;
    }

    public void ResetStreakIfMissedDay(DateOnly today)
    {
        if (today.DayNumber - Quest?.CreationDate.DayNumber > 1)
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
        if (State.RewardCollected)
            return QuestErrors.NoRewardToCollect;

        var quest = GetOrSelectQuest();
        if (!quest.IsCompleted)
            return QuestErrors.NoRewardToCollect;

        var user = await _userManager.FindByIdAsync(this.GetPrimaryKeyString());
        if (user is null)
            return ProfileErrors.NotFound;

        user.QuestPoints += (int)quest.Difficulty;
        await _userManager.UpdateAsync(user);

        State.MarkRewardCollected();
        await WriteStateAsync();

        return (int)quest.Difficulty;
    }

    public async Task OnGameOverAsync(GameQuestSnapshot snapshot)
    {
        var quest = GetOrSelectQuest();
        if (quest.IsCompleted)
            return;

        quest.ApplySnapshot(snapshot);
        if (quest.IsCompleted)
            State.CompleteQuest();

        await WriteStateAsync();
    }

    private QuestInstance GetOrSelectQuest()
    {
        var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().DateTime);
        if (State.Quest is not null && State.Quest.CreationDate == today)
            return State.Quest;

        return SelectNewQuest();
    }

    private QuestInstance SelectNewQuest()
    {
        var difficulty = _random.NextWeighted(
            new Dictionary<int, QuestDifficulty>()
            {
                [50] = QuestDifficulty.Easy,
                [30] = QuestDifficulty.Medium,
                [20] = QuestDifficulty.Hard,
            }
        );
        var availableQuestVariants = _quests
            .SelectMany(quest =>
                quest
                    .Variants.Where(variant =>
                        variant.Difficulty == difficulty
                        && variant.Description != State.Quest?.Description
                    )
                    .ToList()
            )
            .ToList();
        var questVariant = _random.NextItem(availableQuestVariants);

        var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().DateTime);
        var questInstance = questVariant.CreateInstance(today);

        State.ResetStreakIfMissedDay(today);
        State.ResetProgressForNewQuest(questInstance);

        return questInstance;
    }

    private QuestDto ToDto(QuestInstance quest) =>
        new(
            Difficulty: quest.Difficulty,
            Description: quest.Description,
            Target: quest.Target,
            Progress: quest.Progress,
            CanReplace: State.CanReplace,
            RewardCollected: State.RewardCollected,
            Streak: State.Streak
        );
}
