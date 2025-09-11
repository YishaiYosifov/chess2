using Chess2.Api.Quests.DTOs;
using Chess2.Api.Quests.QuestDefinitions;
using Chess2.Api.Quests.QuestProgressors;
using Chess2.Api.Shared.Services;

namespace Chess2.Api.Quests.Grains;

[Alias("Chess2.Api.Quests.Grains.IQuestGrain")]
public interface IQuestGrain : IGrainWithStringKey
{
    [Alias("GetGuestAsync")]
    Task<QuestDto> GetGuestAsync();
}

[GenerateSerializer]
[Alias("Chess2.Api.Quests.Grains.QuestGrainStorage")]
public class QuestGrainStorage
{
    [Id(0)]
    public IQuestProgressor? Quest { get; set; }
}

public class QuestGrain(IEnumerable<IQuestDefinition> availableQuests, IRandomProvider random)
    : Grain<QuestGrainStorage>,
        IQuestGrain
{
    private readonly IEnumerable<IQuestDefinition> _availableQuests = availableQuests;
    private readonly IRandomProvider _random = random;

    public Task<QuestDto> GetGuestAsync()
    {
        throw new NotImplementedException();
        //var quest = GetQuest();
        //return Task.FromResult(
        //    new QuestDto(
        //        Description: quest.Description,
        //        Progress: quest.Progress,
        //        Target: quest.Target
        //    )
        //);
    }

    //private IQuestCondition GetQuest()
    //{
    //    if (State.Quest is not null)
    //        return State.Quest;

    //    var quest = _random.NextItem(_availableQuests);
    //    State.Quest = quest;
    //    return quest;
    //}
}
