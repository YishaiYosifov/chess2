using AnarchyChess.Api.Game.Grains;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Infrastructure;
using AnarchyChess.Api.QuestLogic;
using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.Quests.DTOs;
using AnarchyChess.Api.Quests.Errors;
using AnarchyChess.Api.Quests.Services;
using ErrorOr;
using Orleans.Streams;

namespace AnarchyChess.Api.Quests.Grains;

[Alias("AnarchyChess.Api.Quests.Grains.IQuestGrain")]
public interface IQuestGrain : IGrainWithStringKey
{
    [Alias("CollectRewardAsync")]
    Task<ErrorOr<int>> CollectRewardAsync(CancellationToken token = default);

    [Alias("GetGuestAsync")]
    Task<QuestDto> GetQuestAsync(CancellationToken token = default);

    [Alias("ReplaceQuestAsync")]
    Task<ErrorOr<QuestDto>> ReplaceQuestAsync(CancellationToken token = default);
}

[GenerateSerializer]
[Alias("AnarchyChess.Api.Quests.Grains.QuestGrainStorage")]
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

        CanReplace = false;
    }

    public void SelectNewQuest(QuestInstance quest)
    {
        // reset streak if last quest was missed, failed, not claimed, or is too old
        if (
            Quest is null
            || !Quest.IsCompleted
            || !RewardCollected
            || quest.CreationDate.DayNumber - Quest.CreationDate.DayNumber >= 2
        )
            Streak = 0;

        Quest = quest;
        CanReplace = true;
        RewardCollected = false;
    }

    public void MarkRewardCollected()
    {
        RewardCollected = true;
        Streak++;
    }
}

[ImplicitStreamSubscription(nameof(GameEndedEvent))]
public class QuestGrain(
    ILogger<QuestGrain> logger,
    [PersistentState(QuestGrain.StateName)] IPersistentState<QuestGrainStorage> state,
    IQuestService questService,
    IRandomQuestProvider questProvider,
    TimeProvider timeProvider
) : Grain, IQuestGrain, IAsyncObserver<GameEndedEvent>
{
    public const string StateName = "quest";

    private readonly ILogger<QuestGrain> _logger = logger;
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

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var streamProvider = this.GetStreamProvider(Streaming.StreamProvider);
        var stream = streamProvider.GetStream<GameEndedEvent>(
            nameof(GameEndedEvent),
            this.GetPrimaryKeyString()
        );
        await stream.SubscribeAsync(this);

        await base.OnActivateAsync(cancellationToken);
    }

    public async Task OnNextAsync(GameEndedEvent @event, StreamSequenceToken? token = null)
    {
        var quest = GetOrSelectQuest();
        if (quest.IsCompleted)
            return;

        if (@event.EndStatus.Result is GameResult.Aborted)
            return;

        var snapshot = await GetQuestSnapshotFromGameEnd(@event);
        if (snapshot is null)
            return;

        quest.ApplySnapshot(snapshot);
        if (quest.IsCompleted)
            _state.State.CompleteQuest();

        await _state.WriteStateAsync();
    }

    public Task OnErrorAsync(Exception ex)
    {
        _logger.LogError(ex, "Error in quest grain game stream");
        return Task.CompletedTask;
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
        var quest = _questProvider.GetRandomQuestInstance(except: _state.State.Quest);
        _state.State.SelectNewQuest(quest);

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

    private async Task<GameQuestSnapshot?> GetQuestSnapshotFromGameEnd(GameEndedEvent @event)
    {
        var game = GrainFactory.GetGrain<IGameGrain>(@event.GameToken);

        var gameStateResult = await game.GetStateAsync();
        if (gameStateResult.IsError)
        {
            _logger.LogWarning(
                "Could not find state for quest on game {GameToken}, {Errors}",
                @event.GameToken,
                gameStateResult.Errors
            );
            return null;
        }
        var gameState = gameStateResult.Value;

        PlayerRoster players = new(gameState.WhitePlayer, gameState.BlackPlayer);
        if (!players.TryGetPlayerById(this.GetPrimaryKeyString(), out var player))
        {
            _logger.LogWarning(
                "Could not find player {UserId} for quest on game {GameToken}",
                this.GetPrimaryKeyString(),
                @event.GameToken
            );
            return null;
        }

        var movesResult = await game.GetMovesAsync();
        if (movesResult.IsError)
        {
            _logger.LogWarning(
                "Could not find moves for quest on game {GameToken}, {Errors}",
                @event.GameToken,
                movesResult.Errors
            );
            return null;
        }

        return new(
            PlayerColor: player.Color,
            MoveHistory: movesResult.Value,
            ResultData: @event.EndStatus,
            FinalGameState: gameState
        );
    }
}
