using Chess2.Api.Quests.Models;

namespace Chess2.Api.Quests.QuestDefinitions;

public interface IQuestDefinition
{
    public IEnumerable<QuestVariant> Variants { get; }
}
