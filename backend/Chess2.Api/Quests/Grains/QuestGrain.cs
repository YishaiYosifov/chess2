using Chess2.Api.Profile.Entities;
using Chess2.Api.Quests.DTOs;
using Chess2.Api.Quests.Models;
using Chess2.Api.Quests.QuestDefinitions;
using Chess2.Api.Shared.Services;
using Microsoft.AspNetCore.Identity;
using Orleans.Concurrency;

namespace Chess2.Api.Quests.Grains;

[Alias("Chess2.Api.Quests.Grains.IQuestGrain")]
public interface IQuestGrain : IGrainWithStringKey
{
    [Alias("GetGuestAsync")]
    Task<QuestDto> GetQuestAsync();

    [OneWay]
    [Alias("OnGameOverAsync")]
    Task OnGameOverAsync(GameQuestSnapshot snapshot);
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
}

public class QuestGrain(
    IEnumerable<IQuestDefinition> availableQuests,
    UserManager<AuthedUser> userManager,
    IRandomProvider random
) : Grain<QuestGrainStorage>, IQuestGrain
{
    private readonly IEnumerable<IQuestDefinition> _availableQuests = availableQuests;
    private readonly UserManager<AuthedUser> _userManager = userManager;
    private readonly IRandomProvider _random = random;

    public async Task<QuestDto> GetQuestAsync()
    {
        var quest = await GetOrSelectQuestAsync();
        return new QuestDto(
            Description: quest.Description,
            Target: quest.Target,
            Progress: State.Progress
        );
    }

    public async Task OnGameOverAsync(GameQuestSnapshot snapshot)
    {
        var quest = await GetOrSelectQuestAsync();
        if (State.Progress >= quest.Target)
            return;

        var progressMade = quest.Progressor.EvaluateProgressMade(snapshot);
        if (progressMade <= 0)
            return;

        State.Progress = Math.Min(quest.Target, State.Progress + progressMade);
        if (State.Progress >= quest.Target)
            await CompleteQuestAsync(quest);

        await WriteStateAsync();
    }

    private async Task<QuestVariant> GetOrSelectQuestAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (State.Quest is not null && State.Date == today)
            return State.Quest;

        var difficulty = _random.NextWeighted(
            new Dictionary<int, QuestDifficulty>()
            {
                [50] = QuestDifficulty.Easy,
                [30] = QuestDifficulty.Medium,
                [20] = QuestDifficulty.Hard,
            }
        );
        var availableQuests = _availableQuests.SelectMany(quest =>
            quest.Variants.Where(x => x.Difficulty == difficulty).ToList()
        );

        var quest = _random.NextItem(availableQuests);
        State.Quest = quest;
        State.Progress = 0;
        State.Date = DateOnly.FromDateTime(DateTime.UtcNow);
        await WriteStateAsync();

        return quest;
    }

    private async Task CompleteQuestAsync(QuestVariant quest)
    {
        var user = await _userManager.FindByIdAsync(this.GetPrimaryKeyString());
        if (user is null)
            return;

        user.QuestPoints += (int)quest.Difficulty;
        await _userManager.UpdateAsync(user);
    }
}
