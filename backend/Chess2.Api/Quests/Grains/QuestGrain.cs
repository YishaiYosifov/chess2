using Chess2.Api.Infrastructure;
using Chess2.Api.QuestLogic;
using Chess2.Api.QuestLogic.Models;
using Chess2.Api.Quests.DTOs;
using Chess2.Api.Quests.Errors;
using Chess2.Api.Quests.Services;
using ErrorOr;
using Orleans.Concurrency;

namespace Chess2.Api.Quests.Grains;

[Alias("Chess2.Api.Quests.Grains.IQuestGrain")]
public interface IQuestGrain : IGrainWithStringKey
{
    [Alias("CollectRewardAsync")]
    Task<ErrorOr<int>> CollectRewardAsync(CancellationToken token = default);

    [Alias("GetGuestAsync")]
    Task<QuestDto> GetQuestAsync(CancellationToken token = default);

    [OneWay]
    [Alias("OnGameOverAsync")]
    Task OnGameOverAsync(GameQuestSnapshot snapshot, CancellationToken token = default);

    [Alias("ReplaceQuestAsync")]
    Task<ErrorOr<QuestDto>> ReplaceQuestAsync(CancellationToken token = default);
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
    [PersistentState(QuestGrain.StateName, StorageNames.QuestState)]
        IPersistentState<QuestGrainStorage> state,
    IQuestService questService,
    IRandomQuestProvider questProvider,
    TimeProvider timeProvider
) : Grain, IQuestGrain
{
    public const string StateName = "quest";

    private readonly IPersistentState<QuestGrainStorage> _state = state;
    private readonly IQuestService _questService = questService;
    private readonly IRandomQuestProvider _questProvider = questProvider;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<QuestDto> GetQuestAsync(CancellationToken token = default)
    {
        var quest = GetOrSelectQuest();
        await _state.WriteStateAsync(token);
        return ToDto(quest);
    }

    public async Task<ErrorOr<QuestDto>> ReplaceQuestAsync(CancellationToken token = default)
    {
        if (!_state.State.CanReplace)
            return QuestErrors.CanotReplace;

        var quest = SelectNewQuest();
        _state.State.CanReplace = false;
        await _state.WriteStateAsync(token);

        return ToDto(quest);
    }

    public async Task<ErrorOr<int>> CollectRewardAsync(CancellationToken token = default)
    {
        if (_state.State.RewardCollected)
            return QuestErrors.NoRewardToCollect;

        var quest = GetOrSelectQuest();
        if (!quest.IsCompleted)
            return QuestErrors.NoRewardToCollect;

        var userId = this.GetPrimaryKeyString();
        await _questService.IncrementQuestPointsAsync(userId, (int)quest.Difficulty, token);

        _state.State.MarkRewardCollected();
        await _state.WriteStateAsync(token);

        return (int)quest.Difficulty;
    }

    public async Task OnGameOverAsync(GameQuestSnapshot snapshot, CancellationToken token = default)
    {
        var quest = GetOrSelectQuest();
        if (quest.IsCompleted)
            return;

        quest.ApplySnapshot(snapshot);
        if (quest.IsCompleted)
            _state.State.CompleteQuest();

        await _state.WriteStateAsync(token);
    }

    private QuestInstance GetOrSelectQuest()
    {
        var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().DateTime);
        if (_state.State.Quest is not null && _state.State.Quest.CreationDate == today)
            return _state.State.Quest;

        return SelectNewQuest();
    }

    private QuestInstance SelectNewQuest()
    {
        var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().DateTime);
        var quest = _questProvider.GetRandomQuestInstance(except: _state.State.Quest);
        _state.State.ResetStreakIfMissedDay(today);
        _state.State.ResetProgressForNewQuest(quest);

        return quest;
    }

    private QuestDto ToDto(QuestInstance quest) =>
        new(
            Difficulty: quest.Difficulty,
            Description: quest.Description,
            Target: quest.Target,
            Progress: quest.Progress,
            CanReplace: _state.State.CanReplace,
            RewardCollected: _state.State.RewardCollected,
            Streak: _state.State.Streak
        );
}
